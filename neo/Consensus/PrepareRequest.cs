using Neo.IO;
using Neo.Network.P2P.Payloads;
using System;
using System.IO;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareRequest消息.
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
        /// 消息大小：
        /// <code>base.Size + sizeof(ulong) + NextConsensus.Size + TransactionHashes.GetVarSize() + MinerTransaction.Size + Signature.Length</code>
        /// </summary>
        public override int Size => base.Size + sizeof(ulong) + NextConsensus.Size + TransactionHashes.GetVarSize() + MinerTransaction.Size + Signature.Length;

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
        /// Serialize this message
        /// <list type="bullet">
        /// <item>
        /// <term>Type</term>
        /// <description>message type</description>
        /// </item>
        /// <item>
        /// <term>ViewNumber</term>
        /// <description>view number</description>
        /// </item>
        /// <item>
        /// <term>Nonce</term>
        /// <description>block nonce</description>
        /// </item>
        /// <term>NextConsensus</term>
        /// <description>The script hash of the next round consensus nodes' multi-sign contract</description>
        /// </item>
        /// <term>TransactionHashes</term>
        /// <description>Hash list of the proposal block's transactions</description>
        /// </item>
        /// <term>MinerTransaction</term>
        /// <description>Miner transanction</description>
        /// </item>
        /// <term>Signature</term>
        /// <description>Signature of the proposal block</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">binaray writer</param>
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
