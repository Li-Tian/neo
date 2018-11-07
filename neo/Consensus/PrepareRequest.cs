using Neo.IO;
using Neo.Network.P2P.Payloads;
using System;
using System.IO;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareRequest message
    /// </summary>
    internal class PrepareRequest : ConsensusMessage
    {
        /// <summary>
        /// block nonce, random value
        /// </summary>
        public ulong Nonce;

        /// <summary>
        /// next block's consensus nodes multi-sign script hash
        /// </summary>        
        public UInt160 NextConsensus;

        /// <summary>
        /// consensus block txs' hash list
        /// </summary>   
        public UInt256[] TransactionHashes;

        /// <summary>
        /// consensus block's miner transanction. It contains block bonus for `primrary` node
        /// </summary>   
        public MinerTransaction MinerTransaction;

        /// <summary>
        /// consensus block's signatures, but it only store `primrary` signature
        /// </summary>   
        public byte[] Signature;

        public PrepareRequest()
            : base(ConsensusMessageType.PrepareRequest)
        {
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Nonce = reader.ReadUInt64();
            NextConsensus = reader.ReadSerializable<UInt160>();
            TransactionHashes = reader.ReadSerializableArray<UInt256>();
            if (TransactionHashes.Distinct().Count() != TransactionHashes.Length)
                throw new FormatException();
            MinerTransaction = reader.ReadSerializable<MinerTransaction>();
            if (MinerTransaction.Hash != TransactionHashes[0])
                throw new FormatException();
            Signature = reader.ReadBytes(64);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Nonce);
            writer.Write(NextConsensus);
            writer.Write(TransactionHashes);
            writer.Write(MinerTransaction);
            writer.Write(Signature);
        }
    }
}
