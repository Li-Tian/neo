using Neo.Cryptography;
using Neo.IO;
using System;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    // <summary>
    // 智能合约存储的键
    // </summary>
    /// <summary>
    /// The key for storage of items in  smart contract 
    /// </summary>
    public class StorageKey : IEquatable<StorageKey>, ISerializable
    {
        // <summary>
        // 合约脚本hash
        // </summary>
        /// <summary>
        /// The contract script hash
        /// </summary>
        public UInt160 ScriptHash;

        // <summary>
        // 具体的键
        // </summary>
        /// <summary>
        /// The concrete key
        /// </summary>
        public byte[] Key;

        // <summary>
        // 序列化的字节大小
        // </summary>
        /// <summary>
        /// The size of serialization of this key
        /// </summary>
        int ISerializable.Size => ScriptHash.Size + (Key.Length / 16 + 1) * 17;
        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader"></param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            ScriptHash = reader.ReadSerializable<UInt160>();
            Key = reader.ReadBytesWithGrouping();
        }

        // <summary>
        // 比较两个合约存储的键是否相等。
        // </summary>
        // <param name="other">待对比的智能合约存储键</param>
        // <returns>是否相等</returns>
        // <remarks>合约脚本相等且键值相等。</remarks>
        /// <summary>
        /// Compare the two keys
        /// </summary>
        /// <param name="other">The other key</param>
        /// <returns>If contract script is same and the key value is equal then return true, otherwise return false</returns>
        public bool Equals(StorageKey other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return ScriptHash.Equals(other.ScriptHash) && Key.SequenceEqual(other.Key);
        }

        // <summary>
        // 与另外的元素进行对比，是否相等
        // </summary>
        // <param name="obj">待对比元素</param>
        // <returns>若obj是null或不是StorageKey 返回false，否则进行对比</returns>
        /// <summary>
        /// Compare with other object
        /// </summary>
        /// <param name="obj">The object to be compared with</param>
        /// <returns>If the object is null or not storageKey return false, otherwise use the StorageKey's Equals method to compare </returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is StorageKey)) return false;
            return Equals((StorageKey)obj);
        }


        // <summary>
        // 获取hash code
        // </summary>
        // <returns>等于脚本的hash code 加上key的murmur32值</returns>
        /// <summary>
        /// Get the hash code
        /// </summary>
        /// <returns>Hash code of this key which is hash code of contract script hash the murmur32 of key</returns>
        public override int GetHashCode()
        {
            return ScriptHash.GetHashCode() + (int)Key.Murmur32(0);
        }

        // <summary>
        // 序列化
        // </summary>
        // <param name="writer">输出</param>
        /// <summary>
        /// The serialization
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(ScriptHash);
            writer.WriteBytesWithGrouping(Key);
        }
    }
}
