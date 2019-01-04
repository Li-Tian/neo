using Neo.IO;
using Neo.IO.Json;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    // <summary>
    // 批量存放区块头信息
    // </summary>
    /// <summary>
    /// Store the block headers information in a list
    /// </summary>
    public class HeaderHashList : StateBase, ICloneable<HeaderHashList>
    {
        // <summary>
        // 区块头hash列表，单次最多2000条
        // </summary>
        /// <summary>
        /// The hash list of block headers.Up to 2000 items at a time
        /// </summary>
        public UInt256[] Hashes;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of this object for storage
        /// </summary>
        public override int Size => base.Size + Hashes.GetVarSize();
        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>replica of object</returns>
        HeaderHashList ICloneable<HeaderHashList>.Clone()
        {
            return new HeaderHashList
            {
                Hashes = Hashes
            };
        }

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserilization
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Hashes = reader.ReadSerializableArray<UInt256>();
        }

        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本对象</param>
        /// <summary>
        /// Copy from the replication
        /// </summary>
        /// <param name="replica">The replication of HeaderHashList</param>
        void ICloneable<HeaderHashList>.FromReplica(HeaderHashList replica)
        {
            Hashes = replica.Hashes;
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>Hashes</term>
        // <description>区块头hash列表</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The version of state</description>
        /// </item>
        /// <item>
        /// <term>Hashes</term>
        /// <description>The list of hash headers</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">The binary output</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Hashes);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns></returns>
        /// <summary>
        /// Transfer this object to a json object
        /// </summary>
        /// <returns>The json object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["hashes"] = Hashes.Select(p => (JObject)p.ToString()).ToArray();
            return json;
        }
    }
}
