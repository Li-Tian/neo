using System;
using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  ChangeView message
    ///  ChangeView消息，用于转换视图的共识消息。继承于类ConsensusMessage。
    /// </summary>
    internal class ChangeView : ConsensusMessage
    {
        /// <summary>
        /// New view number
        /// </summary>
        public byte NewViewNumber;

        public override int Size => base.Size + sizeof(byte);

        public ChangeView()
            : base(ConsensusMessageType.ChangeView)
        {
        }

        /// <summary>
        /// Deserialize from reader
        /// </summary>
        /// <param name="reader">binary reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            NewViewNumber = reader.ReadByte();
            if (NewViewNumber == 0) throw new FormatException();
        }

        /// <summary>
        /// Serialize the message
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
        /// <term>NewViewNumber</term>
        /// <description>new view number</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">binary writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(NewViewNumber);
        }
    }
}
