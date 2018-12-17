using Neo.Cryptography;
using Neo.IO;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 过滤器加载的传输数据包
    /// </summary>
    public class FilterLoadPayload : ISerializable
    {
        /// <summary>
        /// 过滤器初始化的位阵列数据
        /// </summary>
        public byte[] Filter;
        /// <summary>
        /// 互相独立的哈希函数的个数
        /// </summary>
        public byte K;
        /// <summary>
        /// 微调参数
        /// </summary>
        public uint Tweak;
        /// <summary>
        /// 过滤器加载负载大小
        /// </summary>
        public int Size => Filter.GetVarSize() + sizeof(byte) + sizeof(uint);

        /// <summary>
        /// 根据一个布隆过滤器创建对应的过滤器加载传输数据包
        /// </summary>
        /// <param name="filter">布隆过滤器</param>
        /// <returns>对应的过滤器加载的传输数据包</returns>
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
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Filter = reader.ReadVarBytes(36000);
            K = reader.ReadByte();
            if (K > 50) throw new FormatException();
            Tweak = reader.ReadUInt32();
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.WriteVarBytes(Filter);
            writer.Write(K);
            writer.Write(Tweak);
        }
    }
}
