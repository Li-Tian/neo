using Neo.IO;
using Neo.IO.Json;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 交易引用
    /// </summary>
    public class CoinReference : IEquatable<CoinReference>, ISerializable
    {
        /// <summary>
        /// 指向的UTXO所在的交易的hash值
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// 指向的UTXO所在的交易的output的位置。从0开始。
        /// </summary>
        public ushort PrevIndex;

        /// <summary>
        /// 存储大小
        /// </summary>
        public int Size => PrevHash.Size + sizeof(ushort);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            PrevHash = reader.ReadSerializable<UInt256>();
            PrevIndex = reader.ReadUInt16();
        }


        /// <summary>
        /// 判断两个交易输入是否相等
        /// </summary>
        /// <param name="other">待比较的交易输入</param>
        /// <returns>
        /// 若待比较交易为null, 则返回false。
        /// 否则按所指向的交易哈希和所指向的交易的output index比较
        /// </returns>
        public bool Equals(CoinReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return PrevHash.Equals(other.PrevHash) && PrevIndex.Equals(other.PrevIndex);
        }

        /// <summary>
        /// 判断交易与该对象是否相等
        /// </summary>
        /// <param name="obj">待比较对象</param>
        /// <returns>
        /// 若待比较对象为null 或 不是CoinReference， 则返回false
        /// 否则按照 public bool Equals(CoinReference other) 方法比较
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            if (!(obj is CoinReference)) return false;
            return Equals((CoinReference)obj);
        }

        /// <summary>
        /// 获取hash code
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return PrevHash.GetHashCode() + PrevIndex.GetHashCode();
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(PrevHash);
            writer.Write(PrevIndex);
        }


        /// <summary>
        /// 转json对象
        /// </summary>
        /// <returns>json对象</returns>
        public JObject ToJson()
        {
            JObject json = new JObject();
            json["txid"] = PrevHash.ToString();
            json["vout"] = PrevIndex;
            return json;
        }
    }
}
