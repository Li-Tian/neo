using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareResponse message
    /// </summary>
    internal class PrepareResponse : ConsensusMessage
    {
        /// <summary>
        /// Signature of the proposal block
        /// </summary>
        public byte[] Signature;

        public PrepareResponse()
            : base(ConsensusMessageType.PrepareResponse)
        {
        }

        /// <summary>
        /// Deserialize from the reader
        /// </summary>
        /// <param name="reader">binary reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Signature = reader.ReadBytes(64);
        }

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Signature);
        }
    }
}
