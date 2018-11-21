using Neo.IO.Json;
using Neo.Ledger;
using System;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 挖矿交易，奖励给出块的共识节点，同时，作为每个区块的第一笔交易
    /// </summary>
    public class MinerTransaction : Transaction
    {
        /// <summary>
        /// 交易nonce 随机值
        /// </summary>
        public uint Nonce;

        /// <summary>
        /// 交易网络手续费，默认为0
        /// </summary>
        public override Fixed8 NetworkFee => Fixed8.Zero;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + sizeof(uint);

        /// <summary>
        /// 创建挖矿交易
        /// </summary>
        public MinerTransaction()
            : base(TransactionType.MinerTransaction)
        {
        }

        /// <summary>
        /// 反序列化，读取nonce值
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
            this.Nonce = reader.ReadUInt32();
        }

        /// <summary>
        ///  交易序列化后处理
        /// </summary>
        /// <exception cref="System.FormatException">若挖矿交易的输入不为空，或者资产不为GAS时，则抛出该异常</exception>
        protected override void OnDeserialized()
        {
            base.OnDeserialized();
            if (Inputs.Length != 0)
                throw new FormatException();
            if (Outputs.Any(p => p.AssetId != Blockchain.UtilityToken.Hash))
                throw new FormatException();
        }

        /// <summary>
        /// 序列化出data外的字段
        /// <list type="bullet">
        /// <item>
        /// <term>Nonce</term>
        /// <description>交易nonce值</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Nonce);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["nonce"] = Nonce;
            return json;
        }
    }
}
