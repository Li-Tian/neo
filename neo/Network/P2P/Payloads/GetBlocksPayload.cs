using Neo.IO;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 获取区块的传输数据包
    // </summary>
    /// <summary>
    /// A payload for getting block
    /// </summary>
    public class GetBlocksPayload : ISerializable
    {
        // <summary>
        // 开始区块的哈希值列表。固定长度为1
        // </summary>
        /// <summary>
        /// The hash list of start blocks. The fixed length is 1
        /// </summary>
        public UInt256[] HashStart;
        // <summary>
        // 结束区块的哈希值
        // </summary>
        /// <summary>
        /// The hash value of the end block
        /// </summary>
        public UInt256 HashStop;
        // <summary>
        // 大小
        // </summary>
        /// <summary>
        /// The size of this payload
        /// </summary>
        public int Size => HashStart.GetVarSize() + HashStop.Size;
        // <summary>
        // 创建一个获取区块的数据包
        // </summary    >
        // <param name="hash_start">开始区块的哈希值</param>
        // <param name="hash_stop">结束区块的哈希值。不指定时，自动设置为0。将最多获取500个区块</param>
        // <returns>创建完成的获取区块的数据包</returns>
        /// <summary>
        /// Create a payload for geting blocks
        /// </summary>
        /// <param name="hash_start">The hash value of the start block</param>
        /// <param name="hash_stop">The hash value of the stop block. If not specified, set to 0 automatically. The most block number is 500</param>
        /// <returns>The playload of the complete hash blocks with the hash start and hash stop</returns>
        public static GetBlocksPayload Create(UInt256 hash_start, UInt256 hash_stop = null)
        {
            return new GetBlocksPayload
            {
                HashStart = new[] { hash_start },
                HashStop = hash_stop ?? UInt256.Zero
            };
        }
        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            HashStart = reader.ReadSerializableArray<UInt256>(16);
            HashStop = reader.ReadSerializable<UInt256>();
        }
        // <summary>
        // 序列化
        // </summary>
        // <param name="writer">二进制输出</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(HashStart);
            writer.Write(HashStop);
        }
    }
}
