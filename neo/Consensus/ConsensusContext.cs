using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    /// 共识过程上下文，记录当前共识活动信息
    /// </summary>
    internal class ConsensusContext : IDisposable
    {
        /// <summary>
        /// 共识协议版本号，目前为0
        /// </summary>
        public const uint Version = 0;

        /// <summary>
        /// 所处共识过程状态
        /// </summary>
        public ConsensusState State;

        /// <summary>
        /// 上一个block的hash
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// 提案block的区块高度
        /// </summary>
        public uint BlockIndex;

        /// <summary>
        /// 当前视图的编号
        /// </summary>
        public byte ViewNumber;

        /// <summary>
        /// 持久层快照
        /// </summary>
        public Snapshot Snapshot;

        /// <summary>
        /// 本轮共识节点的公钥列表
        /// </summary>
        public ECPoint[] Validators;

        /// <summary>
        /// 当前节点编号，在Validators数组中序号
        /// </summary>
        public int MyIndex;

        /// <summary>
        /// 本轮共识的议长编号
        /// </summary>
        public uint PrimaryIndex;

        /// <summary>
        /// 当前提案block时间戳
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// 当前提案block的nonce
        /// </summary>
        public ulong Nonce;

        /// <summary>
        /// 当前提案block的NextConsensus, 指定下一轮共识节点
        /// </summary>
        public UInt160 NextConsensus;

        /// <summary>
        /// 当前提案block的交易hash列表
        /// </summary>
        public UInt256[] TransactionHashes;

        /// <summary>
        /// 当前提案block的交易
        /// </summary>
        public Dictionary<UInt256, Transaction> Transactions;

        /// <summary>
        /// 存放收到的提案block的签名数组
        /// </summary>
        public byte[][] Signatures;

        /// <summary>
        /// 收到的各节点期望视图编号，主要用在改变视图过程中。这个数组的每一位对应每个验证人节点期待的视图编号。而验证人是有相应编号的。
        /// </summary>
        public byte[] ExpectedView;

        /// <summary>
        /// 钥匙对
        /// </summary>
        private KeyPair KeyPair;


        /// <summary>
        /// 钱包
        /// </summary>
        private readonly Wallet wallet;

        /// <summary>
        /// 最低共识节点安全阈值个数，低于该阈值，共识过程将会出错
        /// </summary>
        public int M => Validators.Length - (Validators.Length - 1) / 3;

        public ConsensusContext(Wallet wallet)
        {
            this.wallet = wallet;
        }

        /// <summary>
        /// 修改上下文视图编号，即改变视图达成一致，同时重新计算议长编号
        /// </summary>
        /// <remarks>
        /// 若状态带有SignatureSent标志，则保留签名数组（视图过程中，可能收到别的已经提前发来的签名，后面会再进行过滤处理）
        /// </remarks>
        /// <param name="view_number">new view number</param>
        public void ChangeView(byte view_number)
        {
            State &= ConsensusState.SignatureSent; 
            ViewNumber = view_number;
            PrimaryIndex = GetPrimaryIndex(view_number);
            if (State == ConsensusState.Initial)
            {
                TransactionHashes = null;
                Signatures = new byte[Validators.Length][];
            }
            if (MyIndex >= 0)
                ExpectedView[MyIndex] = view_number;
            _header = null;
        }


        /// <summary>
        /// 用当前共识上下文的数据生成一个新区块。
        /// </summary>
        /// <remark>
        /// 
        /// </remark>
        public Block CreateBlock()
        {
            Block block = MakeHeader();
            if (block == null) return null;
            Contract contract = Contract.CreateMultiSigContract(M, Validators);
            ContractParametersContext sc = new ContractParametersContext(block);
            for (int i = 0, j = 0; i < Validators.Length && j < M; i++)
                if (Signatures[i] != null)
                {
                    sc.AddSignature(contract, Validators[i], Signatures[i]);
                    j++;
                }
            sc.Verifiable.Witnesses = sc.GetWitnesses();
            block.Transactions = TransactionHashes.Select(p => Transactions[p]).ToArray();
            return block;
        }

        /// <summary>
        /// 释放资源，释放持久化层快照
        /// </summary>
        public void Dispose()
        {
            Snapshot?.Dispose();
        }

        /// <summary>
        /// 计算议长编号 = (提案block的区块高度 - 当前视图编号) % 共识节点个数
        /// </summary>
        /// <param name="view_number">给定当前视图编号</param>
        /// <returns></returns>
        public uint GetPrimaryIndex(byte view_number)
        {
            int p = ((int)BlockIndex - view_number) % Validators.Length;
            return p >= 0 ? (uint)p : (uint)(p + Validators.Length);
        }

        /// <summary>
        /// 构建ChangeView消息的货物类
        /// </summary>
        /// <returns>共识消息货物</returns>
        public ConsensusPayload MakeChangeView()
        {
            return MakeSignedPayload(new ChangeView
            {
                NewViewNumber = ExpectedView[MyIndex]
            });
        }

        private Block _header = null;

        /// <summary>
        /// 结合上下文数据，构造出区块头
        /// </summary>
        /// <returns>Block</returns>
        public Block MakeHeader()
        {
            if (TransactionHashes == null) return null;
            if (_header == null)
            {
                _header = new Block
                {
                    Version = Version,
                    PrevHash = PrevHash,
                    MerkleRoot = MerkleTree.ComputeRoot(TransactionHashes),
                    Timestamp = Timestamp,
                    Index = BlockIndex,
                    ConsensusData = Nonce,
                    NextConsensus = NextConsensus,
                    Transactions = new Transaction[0]
                };
            }
            return _header;
        }

        /// <summary>
        /// 构建共识消息的货物类
        /// </summary>
        /// <param name="message">具体的共识消息</param>
        /// <returns>共识消息货物</returns>
        private ConsensusPayload MakeSignedPayload(ConsensusMessage message)
        {
            message.ViewNumber = ViewNumber;
            ConsensusPayload payload = new ConsensusPayload
            {
                Version = Version,
                PrevHash = PrevHash,
                BlockIndex = BlockIndex,
                ValidatorIndex = (ushort)MyIndex,
                Timestamp = Timestamp,
                Data = message.ToArray()
            };
            SignPayload(payload);
            return payload;
        }

        public void SignHeader()
        {
            Signatures[MyIndex] = MakeHeader()?.Sign(KeyPair);
        }

        private void SignPayload(ConsensusPayload payload)
        {
            ContractParametersContext sc;
            try
            {
                sc = new ContractParametersContext(payload);
                wallet.Sign(sc);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            sc.Verifiable.Witnesses = sc.GetWitnesses();
        }

        /// <summary>
        /// 构建PrepareRequset消息的共识货物ConsensusPayload类
        /// </summary>
        /// <returns>共识消息货物</returns>
        public ConsensusPayload MakePrepareRequest()
        {
            return MakeSignedPayload(new PrepareRequest
            {
                Nonce = Nonce,
                NextConsensus = NextConsensus,
                TransactionHashes = TransactionHashes,
                MinerTransaction = (MinerTransaction)Transactions[TransactionHashes[0]],
                Signature = Signatures[MyIndex]
            });
        }

        /// <summary>
        /// 构建PrepareResponse消息的共识货物ConsensusPayload类
        /// </summary>
        /// <param name="signature">对提案block的签名</param>
        /// <returns>共识消息货物</returns>
        public ConsensusPayload MakePrepareResponse(byte[] signature)
        {
            return MakeSignedPayload(new PrepareResponse
            {
                Signature = signature
            });
        }

        /// <summary>
        /// 重置上下文数据
        /// </summary>
        /// <remark>
        /// 1. 重新获取当前区块快照
        /// 2. 初始化状态
        /// 3. 重置区块高度为当前快照区块高区+1
        /// 4. 重置视图编号为0
        /// 5. 默认共识节点编号为-1，即此节点不是公式节点。重新计算议长的编号。
        /// 6. 清空签名数组和期望视图数组
        /// 7. 重新计算自身验证人节点编号，并得出KeyPair。
        /// </remark>
        public void Reset()
        {
            Snapshot?.Dispose();
            Snapshot = Blockchain.Singleton.GetSnapshot();
            State = ConsensusState.Initial;
            PrevHash = Snapshot.CurrentBlockHash;
            BlockIndex = Snapshot.Height + 1;
            ViewNumber = 0;
            Validators = Snapshot.GetValidators();
            MyIndex = -1;
            PrimaryIndex = BlockIndex % (uint)Validators.Length;
            TransactionHashes = null;
            Signatures = new byte[Validators.Length][];
            ExpectedView = new byte[Validators.Length];
            KeyPair = null;
            for (int i = 0; i < Validators.Length; i++)
            {
                WalletAccount account = wallet.GetAccount(Validators[i]);
                if (account?.HasKey == true)
                {
                    MyIndex = i;
                    KeyPair = account.GetKey();
                    break;
                }
            }
            _header = null;
        }

        /// <summary>
        /// 填充提案block的数据
        /// </summary>
        /// <remark>
        /// 1. 交易，从内存池加载交易，并进行插件过滤和排序
        /// 2. MinerTransaction和奖励费（奖励费 = Inputs.GAS - outputs.GAS - 总交易系统手续费）
        /// 3. NextConsensus，结合上面的交易，计算下一轮的共识节点，并计算得到
        /// </remark>
        public void Fill()
        {
            IEnumerable<Transaction> mem_pool = Blockchain.Singleton.GetMemoryPool();
            foreach (IPolicyPlugin plugin in Plugin.Policies)
                mem_pool = plugin.FilterForBlock(mem_pool);
            List<Transaction> transactions = mem_pool.ToList();
            Fixed8 amount_netfee = Block.CalculateNetFee(transactions);
            TransactionOutput[] outputs = amount_netfee == Fixed8.Zero ? new TransactionOutput[0] : new[] { new TransactionOutput
            {
                AssetId = Blockchain.UtilityToken.Hash,
                Value = amount_netfee,
                ScriptHash = wallet.GetChangeAddress()
            } };
            while (true)
            {
                ulong nonce = GetNonce();
                MinerTransaction tx = new MinerTransaction
                {
                    Nonce = (uint)(nonce % (uint.MaxValue + 1ul)),
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = outputs,
                    Witnesses = new Witness[0]
                };
                if (!Snapshot.ContainsTransaction(tx.Hash))
                {
                    Nonce = nonce;
                    transactions.Insert(0, tx);
                    break;
                }
            }
            TransactionHashes = transactions.Select(p => p.Hash).ToArray();
            Transactions = transactions.ToDictionary(p => p.Hash);
            NextConsensus = Blockchain.GetConsensusAddress(Snapshot.GetValidators(transactions).ToArray());
            Timestamp = Math.Max(DateTime.UtcNow.ToTimestamp(), Snapshot.GetHeader(PrevHash).Timestamp + 1);
        }

        /// <summary>
        /// 获取Nonce值
        /// </summary>
        /// <returns>返回随机值</returns>
        private static ulong GetNonce()
        {
            byte[] nonce = new byte[sizeof(ulong)];
            Random rand = new Random();
            rand.NextBytes(nonce);
            return nonce.ToUInt64(0);
        }


        /// <summary>
        /// 收到PrepareRequest包后，校验PrepareRequest所带的提案block数据
        /// </summary>
        /// <remarks>
        /// 检验的标准有以下3条：
        /// 1. 共识状态已经标出了RequestReceived，即已经收到了PrepareRequest包
        /// 2. 校验NextConsensus是否与当前快照中的验证人计算出的地址一致
        /// 3. 校验MinerTransaction的奖励计算是否正确。注，MinerTransaction是每个块的第一个交易，记录了网络费的分布情况。
        /// </remarks>
        /// <returns></returns>
        public bool VerifyRequest()
        {
            if (!State.HasFlag(ConsensusState.RequestReceived))
                return false;
            if (!Blockchain.GetConsensusAddress(Snapshot.GetValidators(Transactions.Values).ToArray()).Equals(NextConsensus))
                return false;
            Transaction tx_gen = Transactions.Values.FirstOrDefault(p => p.Type == TransactionType.MinerTransaction);
            Fixed8 amount_netfee = Block.CalculateNetFee(Transactions.Values);
            if (tx_gen?.Outputs.Sum(p => p.Value) != amount_netfee) return false;
            return true;
        }
    }
}
