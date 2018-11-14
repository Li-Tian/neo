using Neo.IO;
using Neo.Network.P2P.Payloads;
using System;
using System.IO;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareRequest message. it only can be sent by the Speaker
    /// </summary>
    internal class PrepareRequest : ConsensusMessage
    {
        /// <summary>
        /// Block nonce, random value
        /// </summary>
        public ulong Nonce;

        /// <summary>
        /// The script hash of the next round consensus nodes' multi-sign contract
        /// </summary>        
        public UInt160 NextConsensus;

        /// <summary>
        /// Hash list of the proposal block's transactions
        /// </summary>   
        public UInt256[] TransactionHashes;

        /// <summary>
        /// Miner transanction. It contains block reward for the `Primary` node
        /// </summary>   
        public MinerTransaction MinerTransaction;

        /// <summary>
        /// Signature of the proposal block
        /// </summary>   
        public byte[] Signature;

        /// <summary>
        /// Construct PrepareRequest
        /// </summary>
        public PrepareRequest()
            : base(ConsensusMessageType.PrepareRequest)
        {
        }

        /// <summary>
        /// Deserialize from the reader
        /// </summary>
        /// <param name="reader">binary reader</param>
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

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <param name="writer">binary writer</param>
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
