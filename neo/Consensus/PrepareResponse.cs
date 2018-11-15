using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareResponse message
    /// </summary>
    internal class PrepareResponse : ConsensusMessage
    {
        /// <summary>
        /// signature of the proposal block
        /// </summary>
        public byte[] Signature;

        public PrepareResponse()
            : base(ConsensusMessageType.PrepareResponse)
        {
        }

        /// <summary>
        /// deserialize from the reader
        /// </summary>
        /// <param name="reader">binary reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Signature = reader.ReadBytes(64);
        }

        /// <summary>
        /// serialize the message
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Signature);
        }
    }
}
