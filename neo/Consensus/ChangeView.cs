using System;
using System.IO;

namespace Neo.Consensus
{   
    /// <summary>
    ///  ChangeView 消息
    /// </summary>
    internal class ChangeView : ConsensusMessage
    {
        /// <summary>
        /// 新视图编号
        /// </summary>
        public byte NewViewNumber;

        public override int Size => base.Size + sizeof(byte);

        public ChangeView()
            : base(ConsensusMessageType.ChangeView)
        {
        }

        /// <summary>
        /// 从reader中反序列化
        /// </summary>
        /// <param name="reader">二进制读取流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            NewViewNumber = reader.ReadByte();
            if (NewViewNumber == 0) throw new FormatException();
        }

        /// <summary>
        /// 序列化消息内容
        /// <list type="bullet">
        /// <item>
        /// <term>Type</term>
        /// <description>消息类型</description>
        /// </item>
        /// <item>
        /// <term>ViewNumber</term>
        /// <description>当前视图编号</description>
        /// </item>
        /// <term>NewViewNumber</term>
        /// <description>新视图编号</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(NewViewNumber);
        }
    }
}
