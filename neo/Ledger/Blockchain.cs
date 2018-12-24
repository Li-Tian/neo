using Akka.Actor;
using Akka.Configuration;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Actors;
using Neo.IO.Caching;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Neo.Ledger
{
    // <summary>
    // 区块链核心Actor
    // </summary>
    /// <summary>
    /// The core Actor of blockChain
    /// </summary>
    public sealed class Blockchain : UntypedActor
    {
        // <summary>
        // 注册消息（自定义AKKA消息类型）
        // </summary>
        /// <summary>
        /// The register message(Customized AKKA message type)
        /// </summary>
        public class Register { }

        // <summary>
        // 智能合约应用执行完毕消息（自定义AKKA消息类型）
        // </summary>
        /// <summary>
        /// Smart contract execution completed message (customized AKKA message type)
        /// </summary>
        public class ApplicationExecuted {
            // <summary>
            // 所处理的交易
            // </summary>
            /// <summary>
            /// The transactions
            /// </summary>
            public Transaction Transaction;
            // <summary>
            // 执行结果
            // </summary>
            /// <summary>
            /// Result of execution
            /// </summary>
            public ApplicationExecutionResult[] ExecutionResults;
        }

        // <summary>
        // 区块持久化完毕消息（自定义AKKA消息类型）
        // </summary>
        /// <summary>
        /// Persistence completed message(customized AKKA message type)
        /// </summary>
        public class PersistCompleted {
            // <summary>
            // 持久化的block
            // </summary>
            /// <summary>
            /// Persistent block
            /// </summary>
            public Block Block;
        }

        // <summary>
        // 导入区块消息（自定义AKKA消息类型）
        // </summary>
        /// <summary>
        /// The block import message (customized AKKA message type)
        /// </summary>
        public class Import {
            // <summary>
            // 待导入的区块列表
            // </summary>
            /// <summary>
            /// The list of blocks which is going to be imported
            /// </summary>
            public IEnumerable<Block> Blocks;
        }
        // <summary>
        // 区块导入完毕消息（自定义AKKA消息类型）
        // </summary>
        /// <summary>
        /// The importing completed message(customized AKKA message type)
        /// </summary>
        public class ImportCompleted { }

        // <summary>
        // 出块时间
        // </summary>
        /// <summary>
        /// The time for each block produce
        /// </summary>
        public static readonly uint SecondsPerBlock = Settings.Default.SecondsPerBlock;

        // <summary>
        // 块奖励调整周期。衰减周期
        // </summary>
        /// <summary>
        /// The decrement interval of reward for each block m
        /// </summary>
        public const uint DecrementInterval = 2000000;

        // <summary>
        // 最大验证人个数
        // </summary>
        /// <summary>
        /// Maximum number of validators
        /// </summary>
        public const uint MaxValidators = 1024;

        // <summary>
        // 区块奖励
        // </summary>
        /// <summary>
        /// The reward of block mining
        /// </summary>
        public static readonly uint[] GenerationAmount = { 8, 7, 6, 5, 4, 3, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        // <summary>
        // 区块间隔时间(TimeSpan类型)
        // </summary>
        /// <summary>
        /// The time for each block produce (TimeSpan)
        /// </summary>
        public static readonly TimeSpan TimePerBlock = TimeSpan.FromSeconds(SecondsPerBlock);

        // <summary>
        // 备用验证人列表
        // </summary>
        /// <summary>
        /// The list of standby validators
        /// </summary>
        public static readonly ECPoint[] StandbyValidators = Settings.Default.StandbyValidators.OfType<string>().Select(p => ECPoint.DecodePoint(p.HexToBytes(), ECCurve.Secp256r1)).ToArray();

#pragma warning disable CS0612
        // <summary>
        // NEO代币定义
        // </summary>
        /// <summary>
        /// The definition of NEO coin
        /// </summary>
        public static readonly RegisterTransaction GoverningToken = new RegisterTransaction
        {
            AssetType = AssetType.GoverningToken,
            Name = "[{\"lang\":\"zh-CN\",\"name\":\"小蚁股\"},{\"lang\":\"en\",\"name\":\"AntShare\"}]",
            Amount = Fixed8.FromDecimal(100000000),
            Precision = 0,
            Owner = ECCurve.Secp256r1.Infinity,
            Admin = (new[] { (byte)OpCode.PUSHT }).ToScriptHash(),
            Attributes = new TransactionAttribute[0],
            Inputs = new CoinReference[0],
            Outputs = new TransactionOutput[0],
            Witnesses = new Witness[0]
        };

        // <summary>
        // GAS代币定义
        // </summary>
        /// <summary>
        /// The definication of GAS coin
        /// </summary>
        public static readonly RegisterTransaction UtilityToken = new RegisterTransaction
        {
            AssetType = AssetType.UtilityToken,
            Name = "[{\"lang\":\"zh-CN\",\"name\":\"小蚁币\"},{\"lang\":\"en\",\"name\":\"AntCoin\"}]",
            Amount = Fixed8.FromDecimal(GenerationAmount.Sum(p => p) * DecrementInterval),
            Precision = 8,
            Owner = ECCurve.Secp256r1.Infinity,
            Admin = (new[] { (byte)OpCode.PUSHF }).ToScriptHash(),
            Attributes = new TransactionAttribute[0],
            Inputs = new CoinReference[0],
            Outputs = new TransactionOutput[0],
            Witnesses = new Witness[0]
        };
#pragma warning restore CS0612

        // <summary>
        // 创世块定义
        // </summary>
        /// <summary>
        /// The definition of GenesisBlock
        /// </summary>
        public static readonly Block GenesisBlock = new Block
        {
            PrevHash = UInt256.Zero,
            Timestamp = (new DateTime(2016, 7, 15, 15, 8, 21, DateTimeKind.Utc)).ToTimestamp(),
            Index = 0,
            ConsensusData = 2083236893, //向比特币致敬
            NextConsensus = GetConsensusAddress(StandbyValidators),
            Witness = new Witness
            {
                InvocationScript = new byte[0],
                VerificationScript = new[] { (byte)OpCode.PUSHT }
            },
            Transactions = new Transaction[]
            {
                new MinerTransaction
                {
                    Nonce = 2083236893,
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new TransactionOutput[0],
                    Witnesses = new Witness[0]
                },
                GoverningToken,
                UtilityToken,
                new IssueTransaction
                {
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new[]
                    {
                        new TransactionOutput
                        {
                            AssetId = GoverningToken.Hash,
                            Value = GoverningToken.Amount,
                            ScriptHash = Contract.CreateMultiSigRedeemScript(StandbyValidators.Length / 2 + 1, StandbyValidators).ToScriptHash()
                        }
                    },
                    Witnesses = new[]
                    {
                        new Witness
                        {
                            InvocationScript = new byte[0],
                            VerificationScript = new[] { (byte)OpCode.PUSHT }
                        }
                    }
                }
            }
        };

        private static readonly object lockObj = new object();
        private readonly NeoSystem system;
        private readonly List<UInt256> header_index = new List<UInt256>();
        private uint stored_header_count = 0;
        private readonly Dictionary<UInt256, Block> block_cache = new Dictionary<UInt256, Block>();
        private readonly Dictionary<uint, LinkedList<Block>> block_cache_unverified = new Dictionary<uint, LinkedList<Block>>();
        private readonly MemoryPool mem_pool = new MemoryPool(50_000);
        private readonly ConcurrentDictionary<UInt256, Transaction> mem_pool_unverified = new ConcurrentDictionary<UInt256, Transaction>();
        internal readonly RelayCache RelayCache = new RelayCache(100);
        private readonly HashSet<IActorRef> subscribers = new HashSet<IActorRef>();
        private Snapshot currentSnapshot;

        // <summary>
        // 持久化存储器
        // </summary>
        /// <summary>
        /// The storage for persistence
        /// </summary>
        public Store Store { get; }
        
        // <summary>
        // 当前区块高度
        // </summary>
        /// <summary>
        /// Current block height
        /// </summary>
        public uint Height => currentSnapshot.Height;

        // <summary>
        // 区块头高度
        // </summary>
        /// <summary>
        /// The height of block header
        /// </summary>
        public uint HeaderHeight => (uint)header_index.Count - 1;

        // <summary>
        // 当前区块hash
        // </summary>
        /// <summary>
        /// The hash of current block
        /// </summary>
        public UInt256 CurrentBlockHash => currentSnapshot.CurrentBlockHash;

        // <summary>
        // 当前区块头hash
        // </summary>
        /// <summary>
        /// The hash of current block header
        /// </summary>
        public UInt256 CurrentHeaderHash => header_index[header_index.Count - 1];

        private static Blockchain singleton;

        // <summary>
        // 获取单例的区块链
        // </summary>
        /// <summary>
        /// Get the singleton of blocks
        /// </summary>
        public static Blockchain Singleton
        {
            get
            {
                // TODO 有待改进
                while (singleton == null) Thread.Sleep(10);
                return singleton;
            }
        }

        static Blockchain()
        {
            GenesisBlock.RebuildMerkleRoot();
        }

        // <summary>
        // 构造函数.构造一个区块链核心
        // </summary>
        // <param name="system">neo actor系统</param>
        // <param name="store">持久化存储</param>
        /// <summary>
        /// Constructor which create a core blockchain
        /// </summary>
        /// <param name="system">NEO actor system</param>
        /// <param name="store">The storage for persistence</param>
        public Blockchain(NeoSystem system, Store store)
        {
            this.system = system;
            this.Store = store;
            lock (lockObj)
            {
                if (singleton != null)
                    throw new InvalidOperationException();
                header_index.AddRange(store.GetHeaderHashList().Find().OrderBy(p => (uint)p.Key).SelectMany(p => p.Value.Hashes));
                stored_header_count += (uint)header_index.Count;
                if (stored_header_count == 0)
                {
                    header_index.AddRange(store.GetBlocks().Find().OrderBy(p => p.Value.TrimmedBlock.Index).Select(p => p.Key));
                }
                else
                {
                    HashIndexState hashIndex = store.GetHeaderHashIndex().Get();
                    if (hashIndex.Index >= stored_header_count)
                    {
                        DataCache<UInt256, BlockState> cache = store.GetBlocks();
                        for (UInt256 hash = hashIndex.Hash; hash != header_index[(int)stored_header_count - 1];)
                        {
                            header_index.Insert((int)stored_header_count, hash);
                            hash = cache[hash].TrimmedBlock.PrevHash;
                        }
                    }
                }
                if (header_index.Count == 0)
                    Persist(GenesisBlock);
                else
                    UpdateCurrentSnapshot();
                singleton = this;
            }
        }

        // <summary>
        // 根据区块hash查询区块是否存在
        // </summary>
        // <param name="hash">区块hash</param>
        // <returns>如果这个区块已经存在返回true, 否则返回False</returns>
        /// <summary>
        /// Query if the block exists.
        /// </summary>
        /// <param name="hash">The block hash</param>
        /// <returns>If block exists return True, otherwise return False</returns>
        public bool ContainsBlock(UInt256 hash)
        {
            if (block_cache.ContainsKey(hash)) return true;
            return Store.ContainsBlock(hash);
        }

        // <summary>
        // 根据交易hash查询交易是否存在
        // </summary>
        // <param name="hash">交易的hash</param>
        // <returns>如果该交易存在返回true, 否则返回false</returns>
        /// <summary>
        /// Query if the transaction exists according to the transaction hash
        /// </summary>
        /// <param name="hash">The hash of transaction</param>
        /// <returns>If the transaction exists return true, otherwise return false</returns>
        public bool ContainsTransaction(UInt256 hash)
        {
            if (mem_pool.ContainsKey(hash)) return true;
            return Store.ContainsTransaction(hash);
        }

        private void Distribute(object message)
        {
            foreach (IActorRef subscriber in subscribers)
                subscriber.Tell(message);
        }

        // <summary>
        // 根据区块hash查询区块
        // </summary>
        // <param name="hash">区块hash</param>
        // <returns>区块</returns>
        /// <summary>
        /// Get block according to the block hash
        /// </summary>
        /// <param name="hash">The hash of blocks</param>
        /// <returns>Block</returns>
        public Block GetBlock(UInt256 hash)
        {
            if (block_cache.TryGetValue(hash, out Block block))
                return block;
            return Store.GetBlock(hash);
        }

        // <summary>
        // 根据区块高度查询区块
        // </summary>
        // <param name="index">区块高度</param>
        // <returns>区块</returns>
        /// <summary>
        /// Get block according to the blocks height
        /// </summary>
        /// <param name="index">The height index of blocks</param>
        /// <returns>Block</returns>
        public UInt256 GetBlockHash(uint index)
        {
            if (header_index.Count <= index) return null;
            return header_index[(int)index];
        }

        // <summary>
        // 获取共识地址
        // </summary>
        // <param name="validators">验证人列表，或共识节点</param>
        // <returns>共识节点的三分之二多方签名合约的脚本hash</returns>
        /// <summary>
        /// Get the consensus address
        /// </summary>
        /// <param name="validators">The list of validtors or the consensus nodes</param>
        /// <returns>Get hash of contract which has 2/3 consensus nodes multiple signed  </returns>
        public static UInt160 GetConsensusAddress(ECPoint[] validators)
        {
            return Contract.CreateMultiSigRedeemScript(validators.Length - (validators.Length - 1) / 3, validators).ToScriptHash();
        }

        // <summary>
        // 获取内存池交易
        // </summary>
        // <returns>内存池所有交易的序列</returns>
        /// <summary>
        /// Get the transactions from memery pool
        /// </summary>
        /// <returns>All transaction from memery pool</returns>
        public IEnumerable<Transaction> GetMemoryPool()
        {
            return mem_pool;
        }

        // <summary>
        // 获取区块快照
        // </summary>
        // <returns>区块的快照</returns>
        /// <summary>
        /// Get the snapshot of blocks
        /// </summary>
        /// <returns>The snapshot of blocks</returns>
        public Snapshot GetSnapshot()
        {
            return Store.GetSnapshot();
        }

        // <summary>
        // 根据交易hash查询交易.先去从内存池中拿， 如果没有， 则去从持久化存储中去哪.
        // </summary>
        // <param name="hash">交易的hash</param>
        // <returns>通过交易获取的交易</returns>
        /// <summary>
        /// Get the transaction according to the hash. First get the transaction from memory pool. If not exist, get it from the persistant storage
        /// </summary>
        /// <param name="hash">The hash of transaction</param>
        /// <returns>The transaction get according to hash</returns>
        public Transaction GetTransaction(UInt256 hash)
        {
            if (mem_pool.TryGetValue(hash, out Transaction transaction))
                return transaction;
            return Store.GetTransaction(hash);
        }

        // <summary>
        // 根据交易hash查询未验证的交易
        // </summary>
        // <param name="hash">未验证的交易hash</param>
        // <returns>查询到的交易</returns>
        /// <summary>
        /// Get the unverified transaction according  to the transaction hash
        /// </summary>
        /// <param name="hash">The hash of transaction</param>
        /// <returns>The transaction get from memory pool</returns>
        internal Transaction GetUnverifiedTransaction(UInt256 hash)
        {
            mem_pool_unverified.TryGetValue(hash, out Transaction transaction);
            return transaction;
        }

        private void OnImport(IEnumerable<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                if (block.Index <= Height) continue;
                if (block.Index != Height + 1)
                    throw new InvalidOperationException();
                Persist(block);
                SaveHeaderHashList();
            }
            Sender.Tell(new ImportCompleted());
        }

        private void AddUnverifiedBlockToCache(Block block)
        {
            if (!block_cache_unverified.TryGetValue(block.Index, out LinkedList<Block> blocks))
            {
                blocks = new LinkedList<Block>();
                block_cache_unverified.Add(block.Index, blocks);
            }

            blocks.AddLast(block);
        }
        
        private RelayResultReason OnNewBlock(Block block)
        {
            if (block.Index <= Height)
                return RelayResultReason.AlreadyExists;
            if (block_cache.ContainsKey(block.Hash))
                return RelayResultReason.AlreadyExists;
            if (block.Index - 1 >= header_index.Count)
            {
                AddUnverifiedBlockToCache(block);
                return RelayResultReason.UnableToVerify;
            }
            if (block.Index == header_index.Count)
            {
                if (!block.Verify(currentSnapshot))
                    return RelayResultReason.Invalid;
            }
            else
            {
                if (!block.Hash.Equals(header_index[(int)block.Index]))
                    return RelayResultReason.Invalid;
            }
            if (block.Index == Height + 1)
            {
                Block block_persist = block;
                List<Block> blocksToPersistList = new List<Block>();                                
                while (true)
                {
                    blocksToPersistList.Add(block_persist);
                    if (block_persist.Index + 1 >= header_index.Count) break;
                    UInt256 hash = header_index[(int)block_persist.Index + 1];
                    if (!block_cache.TryGetValue(hash, out block_persist)) break;
                }

                int blocksPersisted = 0;
                foreach (Block blockToPersist in blocksToPersistList)
                {
                    block_cache_unverified.Remove(blockToPersist.Index);
                    Persist(blockToPersist);

                    if (blocksPersisted++ < blocksToPersistList.Count - 2) continue;
                    // Relay most recent 2 blocks persisted

                    if (blockToPersist.Index + 100 >= header_index.Count)
                        system.LocalNode.Tell(new LocalNode.RelayDirectly { Inventory = block });
                }
                SaveHeaderHashList();

                if (block_cache_unverified.TryGetValue(Height + 1, out LinkedList<Block> unverifiedBlocks))
                {
                    foreach (var unverifiedBlock in unverifiedBlocks)
                        Self.Tell(unverifiedBlock, ActorRefs.NoSender);   
                    block_cache_unverified.Remove(Height + 1);
                }
            }
            else
            {
                block_cache.Add(block.Hash, block);
                if (block.Index + 100 >= header_index.Count)
                    system.LocalNode.Tell(new LocalNode.RelayDirectly { Inventory = block });
                if (block.Index == header_index.Count)
                {
                    header_index.Add(block.Hash);
                    using (Snapshot snapshot = GetSnapshot())
                    {
                        snapshot.Blocks.Add(block.Hash, new BlockState
                        {
                            SystemFeeAmount = 0,
                            TrimmedBlock = block.Header.Trim()
                        });
                        snapshot.HeaderHashIndex.GetAndChange().Hash = block.Hash;
                        snapshot.HeaderHashIndex.GetAndChange().Index = block.Index;
                        SaveHeaderHashList(snapshot);
                        snapshot.Commit();
                    }
                    UpdateCurrentSnapshot();
                }
            }
            return RelayResultReason.Succeed;
        }

        private RelayResultReason OnNewConsensus(ConsensusPayload payload)
        {
            if (!payload.Verify(currentSnapshot)) return RelayResultReason.Invalid;
            system.Consensus?.Tell(payload);
            RelayCache.Add(payload);
            system.LocalNode.Tell(new LocalNode.RelayDirectly { Inventory = payload });
            return RelayResultReason.Succeed;
        }

        private void OnNewHeaders(Header[] headers)
        {
            using (Snapshot snapshot = GetSnapshot())
            {
                foreach (Header header in headers)
                {
                    if (header.Index - 1 >= header_index.Count) break;
                    if (header.Index < header_index.Count) continue;
                    if (!header.Verify(snapshot)) break;
                    header_index.Add(header.Hash);
                    snapshot.Blocks.Add(header.Hash, new BlockState
                    {
                        SystemFeeAmount = 0,
                        TrimmedBlock = header.Trim()
                    });
                    snapshot.HeaderHashIndex.GetAndChange().Hash = header.Hash;
                    snapshot.HeaderHashIndex.GetAndChange().Index = header.Index;
                }
                SaveHeaderHashList(snapshot);
                snapshot.Commit();
            }
            UpdateCurrentSnapshot();
            system.TaskManager.Tell(new TaskManager.HeaderTaskCompleted(), Sender);
        }

        private RelayResultReason OnNewTransaction(Transaction transaction)
        {
            if (transaction.Type == TransactionType.MinerTransaction)
                return RelayResultReason.Invalid;
            if (ContainsTransaction(transaction.Hash))
                return RelayResultReason.AlreadyExists;
            if (!transaction.Verify(currentSnapshot, GetMemoryPool()))
                return RelayResultReason.Invalid;
            if (!Plugin.CheckPolicy(transaction))
                return RelayResultReason.Unknown;

            if (!mem_pool.TryAdd(transaction.Hash, transaction))
                return RelayResultReason.OutOfMemory;

            system.LocalNode.Tell(new LocalNode.RelayDirectly { Inventory = transaction });
            return RelayResultReason.Succeed;
        }

        private void OnPersistCompleted(Block block)
        {
            block_cache.Remove(block.Hash);
            foreach (Transaction tx in block.Transactions)
                mem_pool.TryRemove(tx.Hash, out _);
            mem_pool_unverified.Clear();
            foreach (Transaction tx in mem_pool
                .OrderByDescending(p => p.NetworkFee / p.Size)
                .ThenByDescending(p => p.NetworkFee)
                .ThenByDescending(p => new BigInteger(p.Hash.ToArray())))
            {
                mem_pool_unverified.TryAdd(tx.Hash, tx);
                Self.Tell(tx, ActorRefs.NoSender);
            }
            mem_pool.Clear();
            PersistCompleted completed = new PersistCompleted { Block = block };
            system.Consensus?.Tell(completed);
            Distribute(completed);
        }
        // <summary>
        // 消息处理函数（AKKA框架函数）
        // </summary>
        // <param name="message">收到的消息</param>
        /// <summary>
        /// The message handler which is a AKKA method
        /// </summary>
        /// <param name="message">The message received</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Register _:
                    OnRegister();
                    break;
                case Import import:
                    OnImport(import.Blocks);
                    break;
                case Header[] headers:
                    OnNewHeaders(headers);
                    break;
                case Block block:
                    Sender.Tell(OnNewBlock(block));
                    break;
                case Transaction transaction:
                    Sender.Tell(OnNewTransaction(transaction));
                    break;
                case ConsensusPayload payload:
                    Sender.Tell(OnNewConsensus(payload));
                    break;
                case Terminated terminated:
                    subscribers.Remove(terminated.ActorRef);
                    break;
            }
        }

        private void OnRegister()
        {
            subscribers.Add(Sender);
            Context.Watch(Sender);
        }

        private void Persist(Block block)
        {
            using (Snapshot snapshot = GetSnapshot())
            {
                snapshot.PersistingBlock = block;
                snapshot.Blocks.Add(block.Hash, new BlockState
                {
                    SystemFeeAmount = snapshot.GetSysFeeAmount(block.PrevHash) + (long)block.Transactions.Sum(p => p.SystemFee),
                    TrimmedBlock = block.Trim()
                });
                foreach (Transaction tx in block.Transactions)
                {
                    snapshot.Transactions.Add(tx.Hash, new TransactionState
                    {
                        BlockIndex = block.Index,
                        Transaction = tx
                    });
                    snapshot.UnspentCoins.Add(tx.Hash, new UnspentCoinState
                    {
                        Items = Enumerable.Repeat(CoinState.Confirmed, tx.Outputs.Length).ToArray()
                    });
                    foreach (TransactionOutput output in tx.Outputs)
                    {
                        AccountState account = snapshot.Accounts.GetAndChange(output.ScriptHash, () => new AccountState(output.ScriptHash));
                        if (account.Balances.ContainsKey(output.AssetId))
                            account.Balances[output.AssetId] += output.Value;
                        else
                            account.Balances[output.AssetId] = output.Value;
                        if (output.AssetId.Equals(GoverningToken.Hash) && account.Votes.Length > 0)
                        {
                            foreach (ECPoint pubkey in account.Votes)
                                snapshot.Validators.GetAndChange(pubkey, () => new ValidatorState(pubkey)).Votes += output.Value;
                            snapshot.ValidatorsCount.GetAndChange().Votes[account.Votes.Length - 1] += output.Value;
                        }
                    }
                    foreach (var group in tx.Inputs.GroupBy(p => p.PrevHash))
                    {
                        TransactionState tx_prev = snapshot.Transactions[group.Key];
                        foreach (CoinReference input in group)
                        {
                            snapshot.UnspentCoins.GetAndChange(input.PrevHash).Items[input.PrevIndex] |= CoinState.Spent;
                            TransactionOutput out_prev = tx_prev.Transaction.Outputs[input.PrevIndex];
                            AccountState account = snapshot.Accounts.GetAndChange(out_prev.ScriptHash);
                            if (out_prev.AssetId.Equals(GoverningToken.Hash))
                            {
                                snapshot.SpentCoins.GetAndChange(input.PrevHash, () => new SpentCoinState
                                {
                                    TransactionHash = input.PrevHash,
                                    TransactionHeight = tx_prev.BlockIndex,
                                    Items = new Dictionary<ushort, uint>()
                                }).Items.Add(input.PrevIndex, block.Index);
                                if (account.Votes.Length > 0)
                                {
                                    foreach (ECPoint pubkey in account.Votes)
                                    {
                                        ValidatorState validator = snapshot.Validators.GetAndChange(pubkey);
                                        validator.Votes -= out_prev.Value;
                                        if (!validator.Registered && validator.Votes.Equals(Fixed8.Zero))
                                            snapshot.Validators.Delete(pubkey);
                                    }
                                    snapshot.ValidatorsCount.GetAndChange().Votes[account.Votes.Length - 1] -= out_prev.Value;
                                }
                            }
                            account.Balances[out_prev.AssetId] -= out_prev.Value;
                        }
                    }
                    List<ApplicationExecutionResult> execution_results = new List<ApplicationExecutionResult>();
                    switch (tx)
                    {
#pragma warning disable CS0612
                        case RegisterTransaction tx_register:
                            snapshot.Assets.Add(tx.Hash, new AssetState
                            {
                                AssetId = tx_register.Hash,
                                AssetType = tx_register.AssetType,
                                Name = tx_register.Name,
                                Amount = tx_register.Amount,
                                Available = Fixed8.Zero,
                                Precision = tx_register.Precision,
                                Fee = Fixed8.Zero,
                                FeeAddress = new UInt160(),
                                Owner = tx_register.Owner,
                                Admin = tx_register.Admin,
                                Issuer = tx_register.Admin,
                                Expiration = block.Index + 2 * 2000000,
                                IsFrozen = false
                            });
                            break;
#pragma warning restore CS0612
                        case IssueTransaction _:
                            foreach (TransactionResult result in tx.GetTransactionResults().Where(p => p.Amount < Fixed8.Zero))
                                snapshot.Assets.GetAndChange(result.AssetId).Available -= result.Amount;
                            break;
                        case ClaimTransaction _:
                            foreach (CoinReference input in ((ClaimTransaction)tx).Claims)
                            {
                                if (snapshot.SpentCoins.TryGet(input.PrevHash)?.Items.Remove(input.PrevIndex) == true)
                                    snapshot.SpentCoins.GetAndChange(input.PrevHash);
                            }
                            break;
#pragma warning disable CS0612
                        case EnrollmentTransaction tx_enrollment:
                            snapshot.Validators.GetAndChange(tx_enrollment.PublicKey, () => new ValidatorState(tx_enrollment.PublicKey)).Registered = true;
                            break;
#pragma warning restore CS0612
                        case StateTransaction tx_state:
                            foreach (StateDescriptor descriptor in tx_state.Descriptors)
                                switch (descriptor.Type)
                                {
                                    case StateType.Account:
                                        ProcessAccountStateDescriptor(descriptor, snapshot);
                                        break;
                                    case StateType.Validator:
                                        ProcessValidatorStateDescriptor(descriptor, snapshot);
                                        break;
                                }
                            break;
#pragma warning disable CS0612
                        case PublishTransaction tx_publish:
                            snapshot.Contracts.GetOrAdd(tx_publish.ScriptHash, () => new ContractState
                            {
                                Script = tx_publish.Script,
                                ParameterList = tx_publish.ParameterList,
                                ReturnType = tx_publish.ReturnType,
                                ContractProperties = (ContractPropertyState)Convert.ToByte(tx_publish.NeedStorage),
                                Name = tx_publish.Name,
                                CodeVersion = tx_publish.CodeVersion,
                                Author = tx_publish.Author,
                                Email = tx_publish.Email,
                                Description = tx_publish.Description
                            });
                            break;
#pragma warning restore CS0612
                        case InvocationTransaction tx_invocation:
                            using (ApplicationEngine engine = new ApplicationEngine(TriggerType.Application, tx_invocation, snapshot.Clone(), tx_invocation.Gas))
                            {
                                engine.LoadScript(tx_invocation.Script);
                                if (engine.Execute())
                                {
                                    engine.Service.Commit();
                                }
                                execution_results.Add(new ApplicationExecutionResult
                                {
                                    Trigger = TriggerType.Application,
                                    ScriptHash = tx_invocation.Script.ToScriptHash(),
                                    VMState = engine.State,
                                    GasConsumed = engine.GasConsumed,
                                    Stack = engine.ResultStack.ToArray(),
                                    Notifications = engine.Service.Notifications.ToArray()
                                });
                            }
                            break;
                    }
                    if (execution_results.Count > 0)
                        Distribute(new ApplicationExecuted
                        {
                            Transaction = tx,
                            ExecutionResults = execution_results.ToArray()
                        });
                }
                snapshot.BlockHashIndex.GetAndChange().Hash = block.Hash;
                snapshot.BlockHashIndex.GetAndChange().Index = block.Index;
                if (block.Index == header_index.Count)
                {
                    header_index.Add(block.Hash);
                    snapshot.HeaderHashIndex.GetAndChange().Hash = block.Hash;
                    snapshot.HeaderHashIndex.GetAndChange().Index = block.Index;
                }
                foreach (IPersistencePlugin plugin in Plugin.PersistencePlugins)
                    plugin.OnPersist(snapshot);
                snapshot.Commit();
            }
            UpdateCurrentSnapshot();
            OnPersistCompleted(block);
        }
        // <summary>
        // AKKA框架的自定义回调函数
        // </summary>
        /// <summary>
        /// The customized PostStop method of AKKA which release the resource
        /// </summary>
        protected override void PostStop()
        {
            base.PostStop();
            currentSnapshot?.Dispose();
        }

        internal static void ProcessAccountStateDescriptor(StateDescriptor descriptor, Snapshot snapshot)
        {
            UInt160 hash = new UInt160(descriptor.Key);
            AccountState account = snapshot.Accounts.GetAndChange(hash, () => new AccountState(hash));
            switch (descriptor.Field)
            {
                case "Votes":
                    Fixed8 balance = account.GetBalance(GoverningToken.Hash);
                    foreach (ECPoint pubkey in account.Votes)
                    {
                        ValidatorState validator = snapshot.Validators.GetAndChange(pubkey);
                        validator.Votes -= balance;
                        if (!validator.Registered && validator.Votes.Equals(Fixed8.Zero))
                            snapshot.Validators.Delete(pubkey);
                    }
                    ECPoint[] votes = descriptor.Value.AsSerializableArray<ECPoint>().Distinct().ToArray();
                    if (votes.Length != account.Votes.Length)
                    {
                        ValidatorsCountState count_state = snapshot.ValidatorsCount.GetAndChange();
                        if (account.Votes.Length > 0)
                            count_state.Votes[account.Votes.Length - 1] -= balance;
                        if (votes.Length > 0)
                            count_state.Votes[votes.Length - 1] += balance;
                    }
                    account.Votes = votes;
                    foreach (ECPoint pubkey in account.Votes)
                        snapshot.Validators.GetAndChange(pubkey, () => new ValidatorState(pubkey)).Votes += balance;
                    break;
            }
        }

        internal static void ProcessValidatorStateDescriptor(StateDescriptor descriptor, Snapshot snapshot)
        {
            ECPoint pubkey = ECPoint.DecodePoint(descriptor.Key, ECCurve.Secp256r1);
            ValidatorState validator = snapshot.Validators.GetAndChange(pubkey, () => new ValidatorState(pubkey));
            switch (descriptor.Field)
            {
                case "Registered":
                    validator.Registered = BitConverter.ToBoolean(descriptor.Value, 0);
                    break;
            }
        }

        // <summary>
        // 创建blockchain ActorRef
        // </summary>
        // <param name="system">neo actor系统</param>
        // <param name="store">持久化存储</param>
        // <returns>返回一个不可修改的线程安全的对象引用</returns>
        /// <summary>
        /// Create the blockchain Actor Ref
        /// </summary>
        /// <param name="system">NEO actor system</param>
        /// <param name="store">The storage for persistence</param>
        /// <returns>return a actorRef which is immutable and thread safe</returns>
        public static Props Props(NeoSystem system, Store store)
        {
            return Akka.Actor.Props.Create(() => new Blockchain(system, store)).WithMailbox("blockchain-mailbox");
        }

        private void SaveHeaderHashList(Snapshot snapshot = null)
        {
            if ((header_index.Count - stored_header_count < 2000))
                return;
            bool snapshot_created = snapshot == null;
            if (snapshot_created) snapshot = GetSnapshot();
            try
            {
                while (header_index.Count - stored_header_count >= 2000)
                {
                    snapshot.HeaderHashList.Add(stored_header_count, new HeaderHashList
                    {
                        Hashes = header_index.Skip((int)stored_header_count).Take(2000).ToArray()
                    });
                    stored_header_count += 2000;
                }
                if (snapshot_created) snapshot.Commit();
            }
            finally
            {
                if (snapshot_created) snapshot.Dispose();
            }
        }

        private void UpdateCurrentSnapshot()
        {
            Interlocked.Exchange(ref currentSnapshot, GetSnapshot())?.Dispose();
        }
    }

    internal class BlockchainMailbox : PriorityMailbox
    {
        public BlockchainMailbox(Akka.Actor.Settings settings, Config config)
            : base(settings, config)
        {
        }

        protected override bool IsHighPriority(object message)
        {
            switch (message)
            {
                case Header[] _:
                case Block _:
                case ConsensusPayload _:
                case Terminated _:
                    return true;
                default:
                    return false;
            }
        }
    }
}
