using Neo.IO;
using Neo.IO.Json;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 执行交易，执行脚本或智能合约
    /// </summary>
    public class InvocationTransaction : Transaction
    {
        /// <summary>
        /// 脚本
        /// </summary>
        public byte[] Script;

        /// <summary>
        /// GAS消耗
        /// </summary>
        public Fixed8 Gas;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Script.GetVarSize();

        /// <summary>
        /// 交易手续费 = NVM执行的GAS消耗
        /// </summary>
        public override Fixed8 SystemFee => Gas;

        /// <summary>
        /// 创建执行交易
        /// </summary>
        public InvocationTransaction()
            : base(TransactionType.InvocationTransaction)
        {
        }

        /// <summary>
        /// 反序列化数据，除了data数据外。
        /// </summary>
        /// <param name="reader">二进制读取流</param>
        /// <exception cref="System.FormatException">若交易版本大于1，则抛出该异常</exception>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version > 1) throw new FormatException();
            Script = reader.ReadVarBytes(65536);
            if (Script.Length == 0) throw new FormatException();
            if (Version >= 1)
            {
                Gas = reader.ReadSerializable<Fixed8>();
                if (Gas < Fixed8.Zero) throw new FormatException();
            }
            else
            {
                Gas = Fixed8.Zero;
            }
        }

        /// <summary>
        /// 获取GAS消耗
        /// </summary>
        /// <param name="consumed">实际消耗的gas</param>
        /// <returns>GAS消息等于实际消耗的GAS减去免费的10GAS；若gas消耗小于等于0，则返回0；最后对gas消耗取上整数</returns>
        public static Fixed8 GetGas(Fixed8 consumed)
        {
            Fixed8 gas = consumed - Fixed8.FromDecimal(10);
            if (gas <= Fixed8.Zero) return Fixed8.Zero;
            return gas.Ceiling();
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>Script</term>
        /// <description>待执行脚本</description>
        /// </item>
        /// <item>
        /// <term>Gas</term>
        /// <description>若交易版本号大于等于1，则序列化Gas字段</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.WriteVarBytes(Script);
            if (Version >= 1)
                writer.Write(Gas);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["script"] = Script.ToHexString();
            json["gas"] = Gas.ToString();
            return json;
        }

        /// <summary>
        /// 校验交易
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <param name="mempool">内存池交易</param>
        /// <returns>若消耗的GAS不能整除10^8, 则返回false</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            if (Gas.GetData() % 100000000 != 0) return false;
            return base.Verify(snapshot, mempool);
        }
    }
}
