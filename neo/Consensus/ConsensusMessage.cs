using Neo.IO;
using Neo.IO.Caching;
using System;
using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  Abstract consensus messagee
    /// </summary>
    internal abstract class ConsensusMessage : ISerializable
    {
        /// <summary>
        /// Consensus message reflection cache
        /// </summary>
        private static ReflectionCache<byte> ReflectionCache = ReflectionCache<byte>.CreateFromEnum<ConsensusMessageType>();

        /// <summary>
        /// Consensus message type
        /// </summary>
        public readonly ConsensusMessageType Type;

        /// <summary>
        /// Current view number
        /// </summary>
        public byte ViewNumber;

        /// <summary>
        /// Message size
        /// </summary>
        public virtual int Size => sizeof(ConsensusMessageType) + sizeof(byte);

        /// <summary>
        /// Create ConsensusMessage attached message type
        /// </summary>
        /// <param name="type"></param>
        protected ConsensusMessage(ConsensusMessageType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Deserialize from the reader
        /// </summary>
        /// <param name="reader">binary reader</param>
        public virtual void Deserialize(BinaryReader reader)
        {
            if (Type != (ConsensusMessageType)reader.ReadByte())
                throw new FormatException();
            ViewNumber = reader.ReadByte();
        }

        /// <summary>
        /// Deserialize from the `data` parameter
        /// </summary>
        /// <param name="data">source data</param>
        /// <returns></returns>
        public static ConsensusMessage DeserializeFrom(byte[] data)
        {
            ConsensusMessage message = ReflectionCache.CreateInstance<ConsensusMessage>(data[0]);
            if (message == null) throw new FormatException();

            using (MemoryStream ms = new MemoryStream(data, false))
            using (BinaryReader r = new BinaryReader(ms))
            {
                message.Deserialize(r);
            }
            return message;
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
        /// </list>
        /// </summary>
        /// <param name="writer">binary writer</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(ViewNumber);
        }
    }
}
