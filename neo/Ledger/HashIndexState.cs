using Neo.IO;
using Neo.IO.Json;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 区块索引
    /// </summary>
    public class HashIndexState : StateBase, ICloneable<HashIndexState>
    {
        /// <summary>
        /// 区块hash
        /// </summary>
        public UInt256 Hash = UInt256.Zero;

        /// <summary>
        /// 区块高度
        /// </summary>
        public uint Index = uint.MaxValue;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Hash.Size + sizeof(uint);

        HashIndexState ICloneable<HashIndexState>.Clone()
        {
            return new HashIndexState
            {
                Hash = Hash,
                Index = Index
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Hash = reader.ReadSerializable<UInt256>();
            Index = reader.ReadUInt32();
        }

        void ICloneable<HashIndexState>.FromReplica(HashIndexState replica)
        {
            Hash = replica.Hash;
            Index = replica.Index;
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>Hash</term>
        /// <description>区块hash</description>
        /// </item>
        /// <term>Index</term>
        /// <description>区块高度</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Hash);
            writer.Write(Index);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["hash"] = Hash.ToString();
            json["index"] = Index;
            return json;
        }
    }
}
