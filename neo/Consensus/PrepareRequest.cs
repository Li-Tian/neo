using Neo.IO;
using Neo.Network.P2P.Payloads;
using System;
using System.IO;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    ///  PrepareRequest消息.
    /// </summary>
    internal class PrepareRequest : ConsensusMessage
    {
        /// <summary>
        /// Block nonce, 随机值
        /// </summary>
        public ulong Nonce;

        /// <summary>
        /// 下一轮共识节点的多方签名脚本hash
        /// </summary>        
        public UInt160 NextConsensus;

        /// <summary>
        /// 提案block的交易hash列表
        /// </summary>   
        public UInt256[] TransactionHashes;

        /// <summary>
        /// 挖矿交易，议长奖励交易
        /// </summary>   
        public MinerTransaction MinerTransaction;

        /// <summary>
        /// 提案block的签名
        /// </summary>   
        public byte[] Signature;

        /// <summary>
        /// 消息大小：
        /// <code>base.Size + sizeof(ulong) + NextConsensus.Size + TransactionHashes.GetVarSize() + MinerTransaction.Size + Signature.Length</code>
        /// </summary>
        public override int Size => base.Size + sizeof(ulong) + NextConsensus.Size + TransactionHashes.GetVarSize() + MinerTransaction.Size + Signature.Length;

        /// <summary>
        /// 构建PrepareRequest消息
        /// </summary>
        public PrepareRequest()
            : base(ConsensusMessageType.PrepareRequest)
        {
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制读取流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Nonce = reader.ReadUInt64();
            NextConsensus = reader.ReadSerializable<UInt160>();
            TransactionHashes = reader.ReadSerializableArray<UInt256>();
            if (TransactionHashes.Distinct().Count() != TransactionHashes.Length)
                throw new FormatException();
            MinerTransaction = reader.ReadSerializable<MinerTransaction>();
            if (MinerTransaction.Hash != TransactionHashes[0])
                throw new FormatException();
            Signature = reader.ReadBytes(64);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <list type="bullet">
        /// <item>
        /// <term>Type</term>
        /// <description>消息类型</description>
        /// </item>
        /// <item>
        /// <term>ViewNumber</term>
        /// <description>当前视图编号</description>
        /// </item>
        /// <item>
        /// <term>Nonce</term>
        /// <description>Block nonce</description>
        /// </item>
        /// <term>NextConsensus</term>
        /// <description>下一轮共识节点的多方签名脚本hash</description>
        /// </item>
        /// <term>TransactionHashes</term>
        /// <description>提案block的交易hash列表</description>
        /// </item>
        /// <term>Signature</term>
        /// <description>对提案block的签名</description>
        /// </item>
        /// </list>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Nonce);
            writer.Write(NextConsensus);
            writer.Write(TransactionHashes);
            writer.Write(MinerTransaction);
            writer.Write(Signature);
        }
    }
}
