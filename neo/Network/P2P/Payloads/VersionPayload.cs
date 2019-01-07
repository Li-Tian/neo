using Neo.IO;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 记录版本数据和区块高度的数据对象
    // </summary>
    /// <summary>
    /// Record version data and block height
    /// </summary>
    public class VersionPayload : ISerializable
    {
        // <summary>
        // 版本号
        // </summary>
        /// <summary>
        /// The version number
        /// </summary>
        public uint Version;
        // <summary>
        // 节点功能的描述符。固定值1
        // </summary>
        /// <summary>
        /// The descriptor of the function of node. The fixed value is one
        /// </summary>
        public ulong Services;
        // <summary>
        // 时间戳。自 epoch 开始的秒数。
        // </summary>
        /// <summary>
        /// The timestamp, which is the seconds from the epoch time
        /// </summary>
        public uint Timestamp;
        // <summary>
        // 服务监听端口
        // </summary>
        /// <summary>
        /// The listening port of server side
        /// </summary>
        public ushort Port;
        // <summary>
        // 代表LocalNode的一个随机数
        // </summary>
        /// <summary>
        /// A random number which stands for localNode
        /// </summary>
        public uint Nonce;
        // <summary>
        // 节点软件的名称和版本的描述信息
        // </summary>
        /// <summary>
        /// The name of the node software and the description of version
        /// </summary>
        public string UserAgent;
        // <summary>
        // 区块高度
        // </summary>
        /// <summary>
        /// The height of block
        /// </summary>
        public uint StartHeight;
        // <summary>
        // 是否具有转发功能。默认为true
        // </summary>
        /// <summary>
        /// If has the relay function. The default value is true
        /// </summary>
        public bool Relay;
        // <summary>
        // 数据块的大小
        // </summary>
        /// <summary>
        /// Size of data block
        /// </summary>
        public int Size => sizeof(uint) + sizeof(ulong) + sizeof(uint) + sizeof(ushort) + sizeof(uint) + UserAgent.GetVarSize() + sizeof(uint) + sizeof(bool);
        // <summary>
        // 构建一个VersionPayload对象
        // </summary>
        // <param name="port">接收端监听的端口</param>
        // <param name="nonce">本地节点的一个随机数</param>
        // <param name="userAgent">节点软件的名称和版本的描述信息</param>
        // <param name="startHeight">区块高度</param>
        // <returns>生成的VersionPayload对象</returns>
        /// <summary>
        /// Build a version payload object
        /// </summary>
        /// <param name="port">The port of listener</param>
        /// <param name="nonce">The random number of local node</param>
        /// <param name="userAgent">The name of node software and the description of the version</param>
        /// <param name="startHeight">The height of blocks</param>
        /// <returns>The VersionPayload build with these parameter</returns>
        public static VersionPayload Create(int port, uint nonce, string userAgent, uint startHeight)
        {
            return new VersionPayload
            {
                Version = LocalNode.ProtocolVersion,
                Services = NetworkAddressWithTime.NODE_NETWORK,
                Timestamp = DateTime.Now.ToTimestamp(),
                Port = (ushort)port,
                Nonce = nonce,
                UserAgent = userAgent,
                StartHeight = startHeight,
                Relay = true
            };
        }
        // <summary>
        // 反序列化方法
        // </summary>
        // <param name="reader">2进制读取器</param>
        /// <summary>
        /// Serialization method
        /// </summary>
        /// <param name="reader">binary input reader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Version = reader.ReadUInt32();
            Services = reader.ReadUInt64();
            Timestamp = reader.ReadUInt32();
            Port = reader.ReadUInt16();
            Nonce = reader.ReadUInt32();
            UserAgent = reader.ReadVarString(1024);
            StartHeight = reader.ReadUInt32();
            Relay = reader.ReadBoolean();
        }
        // <summary>
        // 序列化方法
        // </summary>
        // <param name="writer">2进制输出器</param>
        /// <summary>
        /// Serialization method
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Services);
            writer.Write(Timestamp);
            writer.Write(Port);
            writer.Write(Nonce);
            writer.WriteVarString(UserAgent);
            writer.Write(StartHeight);
            writer.Write(Relay);
        }
    }
}
