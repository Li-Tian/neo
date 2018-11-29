using Neo.IO;
using Neo.IO.Json;
using Neo.Wallets;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 交易输出
    /// </summary>
    public class TransactionOutput : ISerializable
    {
        /// <summary>
        /// 资产Id
        /// </summary>
        public UInt256 AssetId;

        /// <summary>
        /// 转账金额
        /// </summary>
        public Fixed8 Value;

        /// <summary>
        /// 收款人地址脚本hash
        /// </summary>
        public UInt160 ScriptHash;

        /// <summary>
        /// 存储大小
        /// </summary>
        public int Size => AssetId.Size + Value.Size + ScriptHash.Size;

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.AssetId = reader.ReadSerializable<UInt256>();
            this.Value = reader.ReadSerializable<Fixed8>();
            if (Value <= Fixed8.Zero) throw new FormatException();
            this.ScriptHash = reader.ReadSerializable<UInt160>();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(AssetId);
            writer.Write(Value);
            writer.Write(ScriptHash);
        }

        /// <summary>
        /// 转成json数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JObject ToJson(ushort index)
        {
            JObject json = new JObject();
            json["n"] = index;
            json["asset"] = AssetId.ToString();
            json["value"] = Value.ToString();
            json["address"] = ScriptHash.ToAddress();
            return json;
        }
    }
}
