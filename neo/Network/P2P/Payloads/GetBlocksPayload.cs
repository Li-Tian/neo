using Neo.IO;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 获取区块的传输数据包
    /// </summary>
    public class GetBlocksPayload : ISerializable
    {
        /// <summary>
        /// 开始区块的哈希值列表。固定长度为1
        /// </summary>
        public UInt256[] HashStart;
        /// <summary>
        /// 结束区块的哈希值
        /// </summary>
        public UInt256 HashStop;
        /// <summary>
        /// 大小
        /// </summary>
        public int Size => HashStart.GetVarSize() + HashStop.Size;
        /// <summary>
        /// 创建一个获取区块的数据包
        /// </summary>
        /// <param name="hash_start">开始区块的哈希值</param>
        /// <param name="hash_stop">结束区块的哈希值。不指定时，自动设置为0。将最多获取500个区块</param>
        /// <returns>创建完成的获取区块的数据包</returns>
        public static GetBlocksPayload Create(UInt256 hash_start, UInt256 hash_stop = null)
        {
            return new GetBlocksPayload
            {
                HashStart = new[] { hash_start },
                HashStop = hash_stop ?? UInt256.Zero
            };
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            HashStart = reader.ReadSerializableArray<UInt256>(16);
            HashStop = reader.ReadSerializable<UInt256>();
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(HashStart);
            writer.Write(HashStop);
        }
    }
}
