using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareResponse message, which only contains a signature
    /// </summary>
    internal class PrepareResponse : ConsensusMessage
    {
        /// <summary>
        /// Signature of the proposal block
        /// </summary>
        public byte[] Signature;

        public override int Size => base.Size + Signature.Length;

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
        /// <term>Signature</term>
        /// <description>block signature</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">binaray writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Signature);
        }
    }
}
