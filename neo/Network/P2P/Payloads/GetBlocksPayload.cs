using Neo.IO;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 获取区块负载类
    /// </summary>
    public class GetBlocksPayload : ISerializable
    {
        /// <summary>
        /// 开始区块的哈希值
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
        /// 创建一个获取区块负载
        /// </summary>
        /// <param name="hash_start">开始区块的哈希值</param>
        /// <param name="hash_stop">结束区块的哈希值</param>
        /// <returns>创建完成的获取区块负载</returns>
        public static GetBlocksPayload Create(UInt256 hash_start, UInt256 hash_stop = null)
        {
            return new GetBlocksPayload
            {
                HashStart = new[] { hash_start },
                HashStop = hash_stop ?? UInt256.Zero
            };
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            HashStart = reader.ReadSerializableArray<UInt256>(16);
            HashStop = reader.ReadSerializable<UInt256>();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(HashStart);
            writer.Write(HashStop);
        }
    }
}
