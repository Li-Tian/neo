using Neo.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 实体类，记录Inventory数据的哈希
    /// </summary>
    public class InvPayload : ISerializable
    {
        /// <summary>
        /// 最大哈希的个数
        /// </summary>
        public const int MaxHashesCount = 500;
        /// <summary>
        /// Inventory数据的类型
        /// </summary>
        public InventoryType Type;
        /// <summary>
        /// 存储哈希的数组
        /// </summary>
        public UInt256[] Hashes;
        /// <summary>
        /// 数据对象的大小
        /// </summary>
        public int Size => sizeof(InventoryType) + Hashes.GetVarSize();
        /// <summary>
        /// 构建一个InvPayload对象
        /// </summary>
        /// <param name="type">Inventory数据的类型</param>
        /// <param name="hashes">哈希数据</param>
        /// <returns>InvPayload对象</returns>
        public static InvPayload Create(InventoryType type, params UInt256[] hashes)
        {
            return new InvPayload
            {
                Type = type,
                Hashes = hashes
            };
        }
        /// <summary>
        /// 构建一组InvPayload对象。
        /// </summary>
        /// <param name="type">Inventory数据的类型</param>
        /// <param name="hashes">哈希数据</param>
        /// <returns>InvPayload对象数组</returns>
        /// <remarks>通过yield指令，每次返回最多500个哈希值。</remarks>
        public static IEnumerable<InvPayload> CreateGroup(InventoryType type, UInt256[] hashes)
        {
            for (int i = 0; i < hashes.Length; i += MaxHashesCount)
                yield return new InvPayload
                {
                    Type = type,
                    Hashes = hashes.Skip(i).Take(MaxHashesCount).ToArray()
                };
        }
        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            Type = (InventoryType)reader.ReadByte();
            if (!Enum.IsDefined(typeof(InventoryType), Type))
                throw new FormatException();
            Hashes = reader.ReadSerializableArray<UInt256>(MaxHashesCount);
        }
        /// <summary>
        /// 序列化方法
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(Hashes);
        }
    }
}
