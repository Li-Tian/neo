using Neo.IO;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 过滤器增加元素的传输数据包
    /// </summary>
    public class FilterAddPayload : ISerializable
    {
        /// <summary>
        /// 需要添加的新元素数据
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// 负载大小
        /// </summary>
        public int Size => Data.GetVarSize();
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Data = reader.ReadVarBytes(520);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.WriteVarBytes(Data);
        }
    }
}
