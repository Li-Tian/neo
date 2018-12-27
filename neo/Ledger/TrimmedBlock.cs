using Neo.IO;
using Neo.IO.Caching;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    // <summary>
    // 简化版本的block，主要方便存储在leveldb中
    // </summary>
    /// <summary>
    /// The trimmed block which is convenient for storing in levelDb
    /// </summary>
    public class TrimmedBlock : BlockBase
    {
        // <summary>
        // 交易hash列表
        // </summary>
        /// <summary>
        /// The hash list of transactions
        /// </summary>
        public UInt256[] Hashes;

        // <summary>
        // 是否是从完整的block简化而来。从区块头对象简化而来的版本没有交易的哈希。
        // </summary>
        /// <summary>
        /// If this object is simplified from complete block. There is no hash for the object which is simplified from block header
        /// </summary>
        public bool IsBlock => Hashes.Length > 0;

        // <summary>
        // 从缓存中获取完整的交易，构建出完整的block
        // </summary>
        // <param name="cache">缓存的交易</param>
        // <returns>完整的block</returns>
        /// <summary>
        /// Get the complete transaction from cache, and build the complete block
        /// </summary>
        /// <param name="cache">The cached transactions</param>
        /// <returns>The complete block</returns>
        public Block GetBlock(DataCache<UInt256, TransactionState> cache)
        {
            return new Block
            {
                Version = Version,
                PrevHash = PrevHash,
                MerkleRoot = MerkleRoot,
                Timestamp = Timestamp,
                Index = Index,
                ConsensusData = ConsensusData,
                NextConsensus = NextConsensus,
                Witness = Witness,
                Transactions = Hashes.Select(p => cache[p].Transaction).ToArray()
            };
        }

        private Header _header = null;

        // <summary>
        // 区块头
        // </summary>
        /// <summary>
        /// The block header
        /// </summary>
        public Header Header
        {
            get
            {
                if (_header == null)
                {
                    _header = new Header
                    {
                        Version = Version,
                        PrevHash = PrevHash,
                        MerkleRoot = MerkleRoot,
                        Timestamp = Timestamp,
                        Index = Index,
                        ConsensusData = ConsensusData,
                        NextConsensus = NextConsensus,
                        Witness = Witness
                    };
                }
                return _header;
            }
        }

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage
        /// </summary>
        public override int Size => base.Size + Hashes.GetVarSize();


        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Hashes = reader.ReadSerializableArray<UInt256>();
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
        // <description>交易hash列表</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>
        // 
        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>Versioin of state</description>
        /// </item>
        /// <item>
        /// <term>Hashes</term>
        /// <description>The hash list of transactions</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Hashes);
        }


        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>
        /// <summary>
        /// The json object
        /// </summary>
        /// <returns>The json Object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["hashes"] = Hashes.Select(p => (JObject)p.ToString()).ToArray();
            return json;
        }
    }
}
