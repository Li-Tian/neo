using Neo.IO;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 节点地址信息和最近活动时间
    /// </summary>
    public class NetworkAddressWithTime : ISerializable
    {
        /// <summary>
        /// 节点类型常量：普通网络节点。
        /// 比特币网络中节点类型很多，与之相比 NEO 网络目前只有一种节点。
        /// </summary>
        public const ulong NODE_NETWORK = 1;
        /// <summary>
        /// 最近一次的活动时间。从EPOCH(1970/1/1 00:00:00)开始，单位秒。
        /// </summary>
        public uint Timestamp;
        /// <summary>
        /// 节点类型。目前 NEO 只有普通网络节点。
        /// </summary>
        public ulong Services;
        /// <summary>
        /// 节点地址信息。包括IP地址和端口
        /// </summary>
        public IPEndPoint EndPoint;
        /// <summary>
        /// 获取传输时的长度（字节）
        /// </summary>
        public int Size => sizeof(uint) + sizeof(ulong) + 16 + sizeof(ushort);
        /// <summary>
        /// 创建一个地址与活动时间信息对象
        /// </summary>
        /// <param name="endpoint">地址信息</param>
        /// <param name="services">服务类型</param>
        /// <param name="timestamp">最近活动时间</param>
        /// <returns>地址与活动时间信息对象</returns>
        public static NetworkAddressWithTime Create(IPEndPoint endpoint, ulong services, uint timestamp)
        {
            return new NetworkAddressWithTime
            {
                Timestamp = timestamp,
                Services = services,
                EndPoint = endpoint
            };
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Timestamp = reader.ReadUInt32();
            Services = reader.ReadUInt64();
            byte[] data = reader.ReadBytes(16);
            if (data.Length != 16) throw new FormatException();
            IPAddress address = new IPAddress(data).Unmap();
            data = reader.ReadBytes(2);
            if (data.Length != 2) throw new FormatException();
            ushort port = data.Reverse().ToArray().ToUInt16(0);
            EndPoint = new IPEndPoint(address, port);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Timestamp);
            writer.Write(Services);
            writer.Write(EndPoint.Address.MapToIPv6().GetAddressBytes());
            writer.Write(BitConverter.GetBytes((ushort)EndPoint.Port).Reverse().ToArray());
        }
    }
}
