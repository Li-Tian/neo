using Neo.IO;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 过滤器增加元素负载类
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

        void ISerializable.Deserialize(BinaryReader reader)
        {
            Data = reader.ReadVarBytes(520);
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.WriteVarBytes(Data);
        }
    }
}
