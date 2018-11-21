using Neo.Ledger;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 区块头
    /// </summary>
    public class Header : BlockBase, IEquatable<Header>
    {

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + 1;

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            if (reader.ReadByte() != 0) throw new FormatException();
        }

        /// <summary>
        /// 比较区块头
        /// </summary>
        /// <param name="other">待比较区块头</param>
        /// <returns>若待比较区块头为null，返回false</returns>
        public bool Equals(Header other)
        {
            if (other is null) return false;
            if (ReferenceEquals(other, this)) return true;
            return Hash.Equals(other.Hash);
        }

        /// <summary>
        /// 比较区块头是否等于某对象
        /// </summary>
        /// <param name="obj">待比较对象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Header);
        }

        /// <summary>
        /// 获取hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        /// <summary>
        /// 序列化，尾部写入固定值0
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
        /// <term>0</term>
        /// <description>固定值0</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write((byte)0);
        }

        /// <summary>
        /// 转成简化版block
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
                Hashes = new UInt256[0]
            };
        }
    }
}
