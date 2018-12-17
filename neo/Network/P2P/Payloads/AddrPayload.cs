using Neo.IO;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 节点收到其它节点发来的 getaddr 消息以后，回复 addr 消息时的传输数据包。
    /// addr 消息里包含本地节点已知的其它节点 IP 地址。
    /// </summary>
    public class AddrPayload : ISerializable
    {
        /// <summary>
        /// 一次最多发送记录数。固定值200。
        /// </summary>
        public const int MaxCountToSend = 200;
        /// <summary>
        /// 已知的其它节点地址信息。包括这些节点的IP地址，监听端口，上次活动时间。
        /// </summary>
        public NetworkAddressWithTime[] AddressList;
        /// <summary>
        /// 获取整个数据包的传输长度。单位:字节
        /// </summary>
        public int Size => AddressList.GetVarSize();

        /// <summary>
        /// 创建 AddrPayload 数据结构
        /// </summary>
        /// <param name="addresses">已知节点的信息列表</param>
        /// <returns>传输时使用的数据结构</returns>
        public static AddrPayload Create(params NetworkAddressWithTime[] addresses)
        {
            return new AddrPayload
            {
                AddressList = addresses
            };
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            AddressList = reader.ReadSerializableArray<NetworkAddressWithTime>(MaxCountToSend);
            if (AddressList.Length == 0)
                throw new FormatException();
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(AddressList);
        }
    }
}
