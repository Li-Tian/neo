using Neo.IO;
using Neo.IO.Caching;
using System;
using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  抽象的共识消息
    /// </summary>
    internal abstract class ConsensusMessage : ISerializable
    {
        /// <summary>
        /// 消息类型反射缓存
        /// </summary>
        private static ReflectionCache<byte> ReflectionCache = ReflectionCache<byte>.CreateFromEnum<ConsensusMessageType>();

        /// <summary>
        /// 共识消息类型
        /// </summary>
        public readonly ConsensusMessageType Type;

        /// <summary> 
        /// 当前视图编号
        /// </summary>
        public byte ViewNumber;

        /// <summary>
        /// 消息大小
        /// </summary>
        public int Size => sizeof(ConsensusMessageType) + sizeof(byte);

        /// <summary>
        /// 构建共识消息
        /// </summary>
        /// <param name="type">消息类型</param>
        protected ConsensusMessage(ConsensusMessageType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// 发序列化
        /// </summary>
        /// <param name="reader">二进制读取流</param>
        public virtual void Deserialize(BinaryReader reader)
        {
            if (Type != (ConsensusMessageType)reader.ReadByte())
                throw new FormatException();
            ViewNumber = reader.ReadByte();
        }

        /// <summary>
        /// 从data中反序列化
        /// </summary>
        /// <param name="data">数据源</param>
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
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>Type</term>
        /// <description>消息类型</description>
        /// </item>
        /// <item>
        /// <term>ViewNumber</term>
        /// <description>当前视图编号</description>
        /// </item>
        /// <item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(ViewNumber);
        }
    }
}
