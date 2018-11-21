using Neo.IO;
using Neo.IO.Caching;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    /// <summary>
    /// 简化版本的block，主要方便存储在leveldb中
    /// </summary>
    public class TrimmedBlock : BlockBase
    {
        /// <summary>
        /// 交易hash列表
        /// </summary>
        public UInt256[] Hashes;

        /// <summary>
        /// 是否是block
        /// </summary>
        public bool IsBlock => Hashes.Length > 0;

        /// <summary>
        /// 从缓存中获取完整的交易，构建出完整的block
        /// </summary>
        /// <param name="cache">缓存的交易</param>
        /// <returns>完整的block</returns>
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

        /// <summary>
        /// 区块头
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

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Hashes.GetVarSize();


        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Hashes = reader.ReadSerializableArray<UInt256>();
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
        /// <description>交易hash列表</description>
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
