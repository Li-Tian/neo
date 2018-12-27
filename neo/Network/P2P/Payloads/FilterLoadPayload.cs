using Neo.Cryptography;
using Neo.IO;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 过滤器加载的传输数据包
    // </summary>
    /// <summary>
    /// The payload which is loaded from filter
    /// </summary>
    public class FilterLoadPayload : ISerializable
    {
        // <summary>
        // 过滤器初始化的位阵列数据
        // </summary>
        /// <summary>
        /// The initial byteArray of filter
        /// </summary>
        public byte[] Filter;
        // <summary>
        // 互相独立的哈希函数的个数
        // </summary>
        /// <summary>
        /// The number of independent hash functions
        /// </summary>
        public byte K;
        // <summary>
        // 微调参数
        // </summary>
        /// <summary>
        /// The tweak parameter
        /// </summary>
        public uint Tweak;
        // <summary>
        // 过滤器加载负载大小
        // </summary>
        /// <summary>
        /// The size of payload of filter
        /// </summary>
        public int Size => Filter.GetVarSize() + sizeof(byte) + sizeof(uint);

        // <summary>
        // 根据一个布隆过滤器创建对应的过滤器加载传输数据包
        // </summary>
        // <param name="filter">布隆过滤器</param>
        // <returns>对应的过滤器加载的传输数据包</returns>
        /// <summary>
        /// The filtered playload which is load from the bloomfilter
        /// </summary>
        /// <param name="filter">The bloomfilter</param>
        /// <returns>The payload load from filter</returns>
        public static FilterLoadPayload Create(BloomFilter filter)
        {
            byte[] buffer = new byte[filter.M / 8];
            filter.GetBits(buffer);
            return new FilterLoadPayload
            {
                Filter = buffer,
                K = (byte)filter.K,
                Tweak = filter.Tweak
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
            Filter = reader.ReadVarBytes(36000);
            K = reader.ReadByte();
            if (K > 50) throw new FormatException();
            Tweak = reader.ReadUInt32();
        }
        // <summary>
        // 序列化
        // </summary>
        // <param name="writer">二进制输出</param>
        /// <summary>
        /// Serialization
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.WriteVarBytes(Filter);
            writer.Write(K);
            writer.Write(Tweak);
        }
    }
}
