﻿using Neo.IO;
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
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.AssetId = reader.ReadSerializable<UInt256>();
            this.Value = reader.ReadSerializable<Fixed8>();
            if (Value <= Fixed8.Zero) throw new FormatException();
            this.ScriptHash = reader.ReadSerializable<UInt160>();
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">二进制输出</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(AssetId);
            writer.Write(Value);
            writer.Write(ScriptHash);
        }

        /// <summary>
        /// 转成json数据
        /// </summary>
        /// <param name="index">此UTXO在交易的output列表中的index。从0开始。</param>
        /// <returns>json对象</returns>
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
