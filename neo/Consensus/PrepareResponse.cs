using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareResponse message
    /// </summary>
    internal class PrepareResponse : ConsensusMessage
    {
        /// <summary>
        /// consensus block signature
        /// </summary>
        public byte[] Signature;

        public PrepareResponse()
            : base(ConsensusMessageType.PrepareResponse)
        {
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Signature = reader.ReadBytes(64);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Signature);
        }
    }
}
