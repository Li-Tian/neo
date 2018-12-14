using Neo.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 区块头的传输数据包，定义了区块头的结构等
    /// </summary>
    public class HeadersPayload : ISerializable
    {
        /// <summary>
        /// 最大区块头数量
        /// </summary>
        public const int MaxHeadersCount = 2000;

        /// <summary>
        /// 区块头数组
        /// </summary>
        public Header[] Headers;

        /// <summary>
        /// 区块头数组的大小
        /// </summary>
        public int Size => Headers.GetVarSize();

        /// <summary>
        /// 根据可枚举区块头集合创建一个区块头传输数据包
        /// </summary>
        /// <param name="headers">可枚举区块头集合</param>
        /// <returns>创建的区块头传输数据包</returns>
        public static HeadersPayload Create(IEnumerable<Header> headers)
        {
            return new HeadersPayload
            {
                Headers = headers.ToArray()
            };
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Headers = reader.ReadSerializableArray<Header>(MaxHeadersCount);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Headers);
        }
    }
}
