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
    internal class ConsensusContext : IDisposable
    {
        /// <summary>
        /// Consensus message version, it's fixed to 0 currently
        /// </summary>
        public const uint Version = 0;

        /// <summary>
        /// Context state
        /// </summary>
        public ConsensusState State;

        /// <summary>
        /// The previous block hash
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// The proposal block height
        /// </summary>
        public uint BlockIndex;

        /// <summary>
        /// Current view number
        /// </summary>
        public byte ViewNumber;

        /// <summary>
        /// Blockchain snapshot
        /// </summary>
        public Snapshot Snapshot;

        /// <summary>
        /// Consensus nodes in the current round
        /// </summary>
        public ECPoint[] Validators;

        /// <summary>
        /// My index in the validators
        /// </summary>
        public int MyIndex;

        /// <summary>
        /// The Speaker index
        /// </summary>
        public uint PrimaryIndex;

        /// <summary>
        /// Timestamp
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// Block nonce
        /// </summary>
        public ulong Nonce;

        /// <summary>
        /// Script hash of the next round consensus nodes' multi-signs contract
        /// </summary>
        public UInt160 NextConsensus;

        /// <summary>
        /// Hash list of Transactions
        /// </summary>
        public UInt256[] TransactionHashes;

        /// <summary>
        /// The proposal block transactions
        /// </summary>
        public Dictionary<UInt256, Transaction> Transactions;

        /// <summary>
        /// Signature array, store the signatures from different validators
        /// </summary>
        public byte[][] Signatures;

        /// <summary>
        /// The expected view number of validators
        /// </summary>
        public byte[] ExpectedView;

        /// <summary>
        /// Key pair
        /// </summary>
        public KeyPair KeyPair;
        private readonly Wallet wallet;

        /// <summary>
        /// The safe consensus threshold. Below this threshold, the network is exposed to fault.
        /// </summary>
        public int M => Validators.Length - (Validators.Length - 1) / 3;

        
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
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// Free resource
        /// </summary>
        public void Dispose()
        {
            Snapshot?.Dispose();
        }

        /// <summary>
        /// Get the Speaker index = (BlockIndex - view_number) % Validators.Length
        /// </summary>
        /// <param name="view_number">current view number</param>
        /// <returns></returns>
        public uint GetPrimaryIndex(byte view_number)
        {
            int p = ((int)BlockIndex - view_number) % Validators.Length;
            return p >= 0 ? (uint)p : (uint)(p + Validators.Length);
        }

        /// <summary>
        /// Create ChangeView message payload
        /// </summary>
        /// <returns>ConsensusPayload</returns>
        public ConsensusPayload MakeChangeView()
        {
            return MakeSignedPayload(new ChangeView
            {
                NewViewNumber = ExpectedView[MyIndex]
            });
        }

        private Block _header = null;

        /// <summary>
        /// Contruct the block header 
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
        /// Create PrepareRequest message paylaod
        /// </summary>
        /// <returns>ConsensusPayload</returns>
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
        /// 1. Update snapshot and PreHash, BlockIndex
        /// 2. Reset ViewNumber zero
        /// 3. Get the latest validators
        /// 4. Calculate the PriamryIndex and MyIndex
        /// 5. Clear Signatures, ExpectedView
        /// </remarks>
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
        /// Fill the proposal block, contains txs, MinerTransaction, NextConsensus
        /// </summary>
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
        /// Get block nonce
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
        /// Verify the PrepareRequest's proposal block
        /// </summary>
        /// <remarks>
        /// Check if NextConsensus is correct and MinerTransaction.output.value is equal to txs network fee
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
