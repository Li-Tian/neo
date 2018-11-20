using Neo.IO;
using Neo.IO.Json;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    /// <summary>
    /// 批量存放区块头信息
    /// </summary>
    public class HeaderHashList : StateBase, ICloneable<HeaderHashList>
    {
        /// <summary>
        /// 区块头hash列表，单次最多2000条
        /// </summary>
        public UInt256[] Hashes;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Hashes.GetVarSize();

        HeaderHashList ICloneable<HeaderHashList>.Clone()
        {
            return new HeaderHashList
            {
                Hashes = Hashes
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Hashes = reader.ReadSerializableArray<UInt256>();
        }

        void ICloneable<HeaderHashList>.FromReplica(HeaderHashList replica)
        {
            Hashes = replica.Hashes;
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>Hashes</term>
        /// <description>区块头hash列表</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Hashes);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["hashes"] = Hashes.Select(p => (JObject)p.ToString()).ToArray();
            return json;
        }
    }
}
