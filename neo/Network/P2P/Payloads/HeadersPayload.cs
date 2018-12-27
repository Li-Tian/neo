using Neo.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 区块头的传输数据包，定义了区块头的结构等
    // </summary>
    /// <summary>
    /// The playload of the headers, which define the struct of block headers
    /// </summary>
    public class HeadersPayload : ISerializable
    {
        // <summary>
        // 最大区块头数量
        // </summary>
        /// <summary>
        /// The max number of headers
        /// </summary>
        public const int MaxHeadersCount = 2000;

        // <summary>
        // 区块头数组
        // </summary>
        /// <summary>
        /// The array of headers
        /// </summary>
        public Header[] Headers;

        // <summary>
        // 区块头数组的大小
        // </summary>
        /// <summary>
        /// The size of block header array
        /// </summary>
        public int Size => Headers.GetVarSize();

        // <summary>
        // 根据可枚举区块头集合创建一个区块头传输数据包
        // </summary>
        // <param name="headers">可枚举区块头集合</param>
        // <returns>创建的区块头传输数据包</returns>
        /// <summary>
        /// Create a payload for headers accoring to enumerable headers
        /// </summary>
        /// <param name="headers">The enumerable headers</param>
        /// <returns>The payload for headers </returns>
        public static HeadersPayload Create(IEnumerable<Header> headers)
        {
            return new HeadersPayload
            {
                Headers = headers.ToArray()
            };
        }
        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入</param>
        /// <summary>
        /// The Deserilization method
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Headers = reader.ReadSerializableArray<Header>(MaxHeadersCount);
        }

        // <summary>
        // 序列化
        // </summary>
        // <param name="writer">二进制输出</param>
        /// <summary>
        /// The serialization
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Headers);
        }
    }
}
