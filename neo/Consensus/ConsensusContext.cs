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
    /// Consensus context, it records the data in current consensus activity.
    /// </summary>
    internal class ConsensusContext : IConsensusContext
    {
        /// <summary>
        /// Consensus message version, it's fixed to 0 currently
        /// </summary>
        public const uint Version = 0;
        /// <summary>
        /// Context state
        /// </summary>
        public ConsensusState State { get; set; }
        /// <summary>
        /// The previous block's hash
        /// </summary>
        public UInt256 PrevHash { get; set; }
        /// <summary>
        /// The proposal block's height
        /// </summary>
        public uint BlockIndex { get; set; }
        /// <summary>
        /// Current view number
        /// </summary>
        public byte ViewNumber { get; set; }
        /// <summary>
        ///  The public keys of consensus nodes in current round
        /// </summary>
        public ECPoint[] Validators { get; set; }
        /// <summary>
        /// My index in the validators array
        /// </summary>
        public int MyIndex { get; set; }
        /// <summary>
        /// The Speaker index in the validators array
        /// </summary>
        public uint PrimaryIndex { get; set; }
        /// <summary>
        /// The proposal block's Timestamp
        /// </summary>
        public uint Timestamp { get; set; }
        /// <summary>
        /// The proposal block's nonce
        /// </summary>
        public ulong Nonce { get; set; }
        /// <summary>
        ///The proposal block's NextConsensus, which binding the consensus nodes in the next round 
        /// </summary>
        public UInt160 NextConsensus { get; set; }
        /// <summary>
        ///The hash list of current proposal block's txs
        /// </summary>
        public UInt256[] TransactionHashes { get; set; }
        /// <summary>
        /// The proposal block's txs
        /// </summary>
        public Dictionary<UInt256, Transaction> Transactions { get; set; }
        /// <summary>
        /// Store the proposal block's signatures recevied
        /// </summary>
        public byte[][] Signatures { get; set; }
        /// <summary>
        /// The expected view number of consensus nodes, mainly used in ChangeView processing. The index of the array is crresponding to the index of nodes.
        /// </summary>
        public byte[] ExpectedView { get; set; }
        /// <summary>
        /// Snapshot of persistence layer
        /// </summary>
        private Snapshot snapshot;
        /// <summary>
        /// Key pair
        /// </summary>
        private KeyPair keyPair;
        /// <summary>
        /// Wallet
        /// </summary>
        private readonly Wallet wallet;

        /// <summary>
        /// The safe consensus threshold. Below this threshold, the network is exposed to fault.
        /// </summary>
        public int M => Validators.Length - (Validators.Length - 1) / 3;
        public Header PrevHeader => snapshot.GetHeader(PrevHash);
        public bool TransactionExists(UInt256 hash) => snapshot.ContainsTransaction(hash);
        public bool VerifyTransaction(Transaction tx) => tx.Verify(snapshot, Transactions.Values);

        
        public ConsensusContext(Wallet wallet)
        {
            this.wallet = wallet;
        }

        /// <summary>
        /// Change view number
        /// </summary>
        /// <remarks>
        /// 1. Update the context ViewNumber, PrimaryIndex and ExpectedView[Myindex]
        /// 2. If the node has the SignatureSent flag, reserve the signatures array 
        /// (Mybe some signatures are arrived before the PrepareRequset received), else reset it
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
        /// Create a full block with the context data
        /// </summary>
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
        /// Free resource, free the snapshot of persistence layer
        /// </summary>
        public void Dispose()
        {
            snapshot?.Dispose();
        }

        /// <summary>
        /// Get the Primary/Speaker index = (the height of proposal block -  view number) % the number of consensus nodes
        /// </summary>
        /// <param name="view_number">view number</param>
        /// <returns>The Primary/Speaker index</returns>
        public uint GetPrimaryIndex(byte view_number)
        {
            int p = ((int)BlockIndex - view_number) % Validators.Length;
            return p >= 0 ? (uint)p : (uint)(p + Validators.Length);
        }

        /// <summary>
        ///  Create ChangeView message
        /// </summary>
        /// <returns>The consensus message payload with ChangeView</returns>
        public ConsensusPayload MakeChangeView()
        {
            return MakeSignedPayload(new ChangeView
            {
                NewViewNumber = ExpectedView[MyIndex]
            });
        }

        private Block _header = null;

        /// <summary>
        /// Construct the block header with context data
        /// </summary>
        /// <returns>block header</returns>
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
        /// Create ConsensusPayload which contains the ConsensusMessage
        /// </summary>
        /// <param name="message">consensus message</param>
        /// <returns>ConsensusPayload</returns>
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


        /// <summary>
        /// Sign the block header
        /// </summary>
        public void SignHeader()
        {
            Signatures[MyIndex] = MakeHeader()?.Sign(keyPair);
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
        /// Create PrepareRequest message paylaod
        /// </summary>
        /// <returns>ConsensusPayload</returns>
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
        /// Create PrepareReponse message paylaod
        /// </summary>
        /// <param name="signature">signaure of the proposal block</param>
        /// <returns>ConsensusPayload</returns>
        public ConsensusPayload MakePrepareResponse(byte[] signature)
        {
            return MakeSignedPayload(new PrepareResponse
            {
                Signature = signature
            });
        }

        /// <summary>
        /// Reset the context
        /// </summary>
        /// <remarks>
        /// 1. Rearquire the blockchain snapshot 
        /// 2. Initial the context state
        /// 3. Update snapshot and PreHash, BlockIndex
        /// 2. Reset ViewNumber zero
        /// 3. Get the latest validators
        /// 4. Calculate the PriamryIndex, MyIndex and keyPair
        /// 5. Clear Signatures, ExpectedView
        /// </remarks>
        public void Reset()
        {
            snapshot?.Dispose();
            snapshot = Blockchain.Singleton.GetSnapshot();
            State = ConsensusState.Initial;
            PrevHash = snapshot.CurrentBlockHash;
            BlockIndex = snapshot.Height + 1;
            ViewNumber = 0;
            Validators = snapshot.GetValidators();
            MyIndex = -1;
            PrimaryIndex = BlockIndex % (uint)Validators.Length;
            TransactionHashes = null;
            Signatures = new byte[Validators.Length][];
            ExpectedView = new byte[Validators.Length];
            keyPair = null;
            for (int i = 0; i < Validators.Length; i++)
            {
                WalletAccount account = wallet.GetAccount(Validators[i]);
                if (account?.HasKey == true)
                {
                    MyIndex = i;
                    keyPair = account.GetKey();
                    break;
                }
            }
            _header = null;
        }

        /// <summary>
        /// Fill the proposal block, contains txs, MinerTransaction, NextConsensus
        /// </summary>
        /// <remark>
        /// 1. Transaction, load from memory pool , sort and filter using plugin.
        /// 2. MinerTransaction and Reward (Reward = Inputs.GAS - outputs.GAS - txs.systemfee)
        /// 3. NextConsensus, calculated by combining the proposal block's txs with previous voting of the validotars
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
                if (!snapshot.ContainsTransaction(tx.Hash))
                {
                    Nonce = nonce;
                    transactions.Insert(0, tx);
                    break;
                }
            }
            TransactionHashes = transactions.Select(p => p.Hash).ToArray();
            Transactions = transactions.ToDictionary(p => p.Hash);
            NextConsensus = Blockchain.GetConsensusAddress(snapshot.GetValidators(transactions).ToArray());
            Timestamp = Math.Max(TimeProvider.Current.UtcNow.ToTimestamp(), PrevHeader.Timestamp + 1);
        }

        /// <summary>
        /// Get a new nonce
        /// </summary>
        /// <returns>random data</returns>
        private static ulong GetNonce()
        {
            byte[] nonce = new byte[sizeof(ulong)];
            Random rand = new Random();
            rand.NextBytes(nonce);
            return nonce.ToUInt64(0);
        }

        /// <summary>
        /// Verify the proposal block, after received the PrepareRequest message
        /// </summary>
        /// <remarks>
        /// 1. If hasn't received the `PrepareRequest` message, return false
        /// 2. Check whether the proposal block's NextConsensus is the same to the result, calculated by the current blockchain snapshot's validators
        /// 3. Check whether the MinerTransaction.output.value is equal to the proposal block's txs network fee
        /// </remarks>
        /// <returns>If valid, then return true</returns>
        public bool VerifyRequest()
        {
            if (!State.HasFlag(ConsensusState.RequestReceived))
                return false;
            if (!Blockchain.GetConsensusAddress(snapshot.GetValidators(Transactions.Values).ToArray()).Equals(NextConsensus))
                return false;
            Transaction tx_gen = Transactions.Values.FirstOrDefault(p => p.Type == TransactionType.MinerTransaction);
            Fixed8 amount_netfee = Block.CalculateNetFee(Transactions.Values);
            if (tx_gen?.Outputs.Sum(p => p.Value) != amount_netfee) return false;
            return true;
        }
    }
}
