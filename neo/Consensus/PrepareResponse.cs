using System.IO;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareResponse消息
    /// </summary>
    internal class PrepareResponse : ConsensusMessage
    {
        /// <summary>
        /// 对提案block的签名
        /// </summary>
        public byte[] Signature;

        public PrepareResponse()
            : base(ConsensusMessageType.PrepareResponse)
        {
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制读取流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Signature = reader.ReadBytes(64);
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
        /// <term>Signature</term>
        /// <description>提案block的签名</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Signature);
        }
    }
}
