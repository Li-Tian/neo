using System;
using System.IO;

namespace Neo.Consensus
{   
    /// <summary>
    ///  Changeview message
    /// </summary>
    internal class ChangeView : ConsensusMessage
    {
        /// <summary>
        /// New view number
        /// </summary>
        public byte NewViewNumber;

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
        /// </summary>
        /// <param name="writer">binary writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(NewViewNumber);
        }
    }
}
