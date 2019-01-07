using Neo.IO;
using Neo.IO.Json;
using System.IO;

namespace Neo.Ledger
{
    // <summary>
    // 区块索引
    // </summary>
    /// <summary>
    /// Block index
    /// </summary>
    public class HashIndexState : StateBase, ICloneable<HashIndexState>
    {
        // <summary>
        // 区块hash
        // </summary>
        /// <summary>
        /// Block hash
        /// </summary>
        public UInt256 Hash = UInt256.Zero;

        // <summary>
        // 区块高度
        // </summary>
        /// <summary>
        /// Block height
        /// </summary>
        public uint Index = uint.MaxValue;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// size
        /// </summary>
        public override int Size => base.Size + Hash.Size + sizeof(uint);
        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// clone method
        /// </summary>
        /// <returns>replica of object</returns>
        HashIndexState ICloneable<HashIndexState>.Clone()
        {
            return new HashIndexState
            {
                Hash = Hash,
                Index = Index
            };
        }

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialize method
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Hash = reader.ReadSerializable<UInt256>();
            Index = reader.ReadUInt32();
        }
        // <summary>
        // 从副本复制
        ///</summary>
        // <param name="replica">副本对象</param>
        /// <summary>
        /// copy a object from a replica
        /// </summary>
        /// <param name="replica">replica</param>
        void ICloneable<HashIndexState>.FromReplica(HashIndexState replica)
        {
            Hash = replica.Hash;
            Index = replica.Index;
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>Hash</term>
        // <description>区块hash</description>
        // </item>
        // <item>
        // <term>Index</term>
        // <description>区块高度</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// Serialize method
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>the version of state</description>
        /// </item>
        /// <item>
        /// <term>Hash</term>
        /// <description>block hash</description>
        /// </item>
        /// <item>
        /// <term>Index</term>
        /// <description>block height</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Hash);
            writer.Write(Index);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns>返回一个包含了Hash和Index的json对象</returns>
        /// <summary>
        /// Convert to a JObject object
        /// </summary>
        /// <returns>a JObject object contains hash and index</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["hash"] = Hash.ToString();
            json["index"] = Index;
            return json;
        }
    }
}
