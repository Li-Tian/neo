using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Json;
using Neo.Ledger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 区块数据
    /// </summary>
    public class Block : BlockBase, IInventory, IEquatable<Block>
    {
        /// <summary>
        /// 交易集合
        /// </summary>
        public Transaction[] Transactions;

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

        InventoryType IInventory.InventoryType => InventoryType.Block;


        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Transactions.GetVarSize();

        /// <summary>
        /// 计算交易的网络手续费, network_fee = input.GAS - output.GAS - input.systemfee
        /// </summary>
        /// <param name="transactions">待计算的交易</param>
        /// <returns></returns>
        public static Fixed8 CalculateNetFee(IEnumerable<Transaction> transactions)
        {
            Transaction[] ts = transactions.Where(p => p.Type != TransactionType.MinerTransaction && p.Type != TransactionType.ClaimTransaction).ToArray();
            Fixed8 amount_in = ts.SelectMany(p => p.References.Values.Where(o => o.AssetId == Blockchain.UtilityToken.Hash)).Sum(p => p.Value);
            Fixed8 amount_out = ts.SelectMany(p => p.Outputs.Where(o => o.AssetId == Blockchain.UtilityToken.Hash)).Sum(p => p.Value);
            Fixed8 amount_sysfee = ts.Sum(p => p.SystemFee);
            return amount_in - amount_out - amount_sysfee;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Transactions = new Transaction[reader.ReadVarInt(0x10000)];
            if (Transactions.Length == 0) throw new FormatException();
            HashSet<UInt256> hashes = new HashSet<UInt256>();
            for (int i = 0; i < Transactions.Length; i++)
            {
                Transactions[i] = Transaction.DeserializeFrom(reader);
                if (i == 0)
                {
                    if (Transactions[0].Type != TransactionType.MinerTransaction)
                        throw new FormatException();
                }
                else
                {
                    if (Transactions[i].Type == TransactionType.MinerTransaction)
                        throw new FormatException();
                }
                if (!hashes.Add(Transactions[i].Hash))
                    throw new FormatException();
            }
            if (MerkleTree.ComputeRoot(Transactions.Select(p => p.Hash).ToArray()) != MerkleRoot)
                throw new FormatException();
        }

        /// <summary>
        /// 判断两个区块是否相等
        /// </summary>
        /// <param name="other">待比较区块</param>
        /// <returns>若待比较区块为null，直接返回false。否则进行引用和hash值比较</returns>
        public bool Equals(Block other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return Hash.Equals(other.Hash);
        }

        /// <summary>
        /// 判断区块是否等于某个对象
        /// </summary>
        /// <param name="obj">待对比对象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Block);
        }

        /// <summary>
        /// 获取区块hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        /// <summary>
        /// 重新构建梅克尔树
        /// </summary>
        public void RebuildMerkleRoot()
        {
            MerkleRoot = MerkleTree.ComputeRoot(Transactions.Select(p => p.Hash).ToArray());
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>Version</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>PrevHash</term>
        /// <description>上一个区块hash</description>
        /// </item>
        /// <item>
        /// <term>MerkleRoot</term>
        /// <description>梅克尔树</description>
        /// </item>
        /// <item>
        /// <term>Timestamp</term>
        /// <description>时间戳</description>
        /// </item>
        /// <item>
        /// <term>Index</term>
        /// <description>区块高度</description>
        /// </item>
        /// <item>
        /// <term>ConsensusData</term>
        /// <description>共识数据，默认为block nonce</description>
        /// </item>
        /// <item>
        /// <term>NextConsensus</term>
        /// <description>下一个区块共识地址</description>
        /// </item>
        /// <item>
        /// <term>Transactions</term>
        /// <description>交易集合</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Transactions);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["tx"] = Transactions.Select(p => p.ToJson()).ToArray();
            return json;
        }


        /// <summary>
        /// 转成简化版的block
        /// </summary>
        /// <returns></returns>
        public TrimmedBlock Trim()
        {
            return new TrimmedBlock
            {
                Version = Version,
                PrevHash = PrevHash,
                MerkleRoot = MerkleRoot,
                Timestamp = Timestamp,
                Index = Index,
                ConsensusData = ConsensusData,
                NextConsensus = NextConsensus,
                Witness = Witness,
                Hashes = Transactions.Select(p => p.Hash).ToArray()
            };
        }
    }
}
