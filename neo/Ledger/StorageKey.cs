using Neo.Cryptography;
using Neo.IO;
using System;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    /// <summary>
    /// 智能合约存储的键
    /// </summary>
    public class StorageKey : IEquatable<StorageKey>, ISerializable
    {
        /// <summary>
        /// 合约脚本hash
        /// </summary>
        public UInt160 ScriptHash;

        /// <summary>
        /// 具体的键
        /// </summary>
        public byte[] Key;

        int ISerializable.Size => ScriptHash.Size + (Key.Length / 16 + 1) * 17;

        void ISerializable.Deserialize(BinaryReader reader)
        {
            ScriptHash = reader.ReadSerializable<UInt160>();
            Key = reader.ReadBytesWithGrouping();
        }

        /// <summary>
        /// 比较两个合约存储的键是否相等
        /// </summary>
        /// <param name="other">待对比的智能合约存储键</param>
        /// <returns>是否相等</returns>
        public bool Equals(StorageKey other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return ScriptHash.Equals(other.ScriptHash) && Key.SequenceEqual(other.Key);
        }

        /// <summary>
        /// 与另外的元素进行对比，是否相等
        /// </summary>
        /// <param name="obj">待对比元素</param>
        /// <returns>若obj是null或不是StorageKey 返回false，否则进行对比</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is StorageKey)) return false;
            return Equals((StorageKey)obj);
        }


        /// <summary>
        /// 获取hash code
        /// </summary>
        /// <returns>等于脚本的hash code 加上key的murmur32值</returns>
        public override int GetHashCode()
        {
            return ScriptHash.GetHashCode() + (int)Key.Murmur32(0);
        }


        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(ScriptHash);
            writer.WriteBytesWithGrouping(Key);
        }
    }
}
