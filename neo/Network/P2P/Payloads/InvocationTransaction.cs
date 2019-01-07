using Neo.IO;
using Neo.IO.Json;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 执行交易，执行脚本或智能合约。包括部署和执行智能合约
    // </summary>
    /// <summary>
    /// Invocation of the transaction, which execute the scripts and contracts, including deploying and executing smart contract
    /// </summary>
    public class InvocationTransaction : Transaction
    {
        // <summary>
        // 脚本
        // </summary>
        /// <summary>
        /// The hash script 
        /// </summary>
        public byte[] Script;

        // <summary>
        // GAS消耗
        // </summary>
        /// <summary>
        /// Gas consumption
        /// </summary>
        public Fixed8 Gas;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage
        /// </summary>
        public override int Size => base.Size + Script.GetVarSize();

        // <summary>
        // 交易手续费 = NVM执行的GAS消耗
        // </summary>
        /// <summary>
        /// The system fee = Gas consumption of NVM
        /// </summary>
        public override Fixed8 SystemFee => Gas;

        // <summary>
        // 构造函数：创建执行交易
        // </summary>
        /// <summary>
        /// The construct function: creating the InvocationTransaction
        /// </summary>
        public InvocationTransaction()
            : base(TransactionType.InvocationTransaction)
        {
        }



        // <summary>
        // 反序列化数据，除了data数据外。
        // </summary>
        // <param name="reader">二进制读取流</param>
        // <exception cref="System.FormatException">
        // 1. 若交易版本大于1，则抛出该异常<br/>
        // 2. 反序列化的脚本数组长度为0.<br/>
        // 3. 指定的执行智能合约的GAS额度小于0.<br/>
        // </exception>
        // <remarks>
        // Version为0时，不指定GAS。默认为0<br/>
        // Version为1时，需要指定GAS<br/>
        // </remarks>

        /// <summary>
        /// Deserialization function which exclude the data
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        /// <exception cref="System.FormatException">
        /// 1. If the transaction version is larger than 1 then throw this exception<br/>
        /// 2. The transction script's length is equal to 0<br/>
        /// 3. The gas consumption of smart contract invocation is smaller than 0. 
        /// </exception>
        /// <remarks>
        /// When the version is 0， do not need gas. The default value is 0<br/>
        /// When the version is 1, need set the gas <br/>
        /// </remarks>
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

        // <summary>
        // 获取GAS消耗
        // </summary>
        // <param name="consumed">实际消耗的gas</param>
        // <returns>
        // GAS消息等于实际消耗的GAS减去免费的10GAS；
        // 若gas消耗小于等于0，则返回0；
        // 最后对gas消耗取上整数
        // </returns>


        /// <summary>
        /// Get the gas consumption
        /// </summary>
        /// <param name="consumed">The real consumpt gas</param>
        /// <returns>
        /// The Gas consumption is equal to the comsumed gas minus the free 10 Gas.
        /// If the gas consumption is small than 0, then return 0
        /// Finally use the ceil of the gas
        /// </returns>
        public static Fixed8 GetGas(Fixed8 consumed)
        {
            Fixed8 gas = consumed - Fixed8.FromDecimal(10);
            if (gas <= Fixed8.Zero) return Fixed8.Zero;
            return gas.Ceiling();
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>Script</term>
        // <description>待执行脚本</description>
        // </item>
        // <item>
        // <term>Gas</term>
        // <description>若交易版本号大于等于1，则序列化Gas字段</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>

        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>Script</term>
        /// <description>The script waiting to be executed</description>
        /// </item>
        /// <item>
        /// <term>Gas</term>
        /// <description>If the version of transaction is larger than 1, then serialize this data</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.WriteVarBytes(Script);
            if (Version >= 1)
                writer.Write(Gas);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>

        /// <summary>
        /// Transfer to json object
        /// </summary>
        /// <returns>Json object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["script"] = Script.ToHexString();
            json["gas"] = Gas.ToString();
            return json;
        }

        // <summary>
        // 校验交易
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <param name="mempool">内存池交易</param>
        // <returns>
        // 1. 若消耗的GAS不能整除10^8, 则返回false.（即，GAS必须是整数单位形式的Fixed8，即不能包含有小数的GAS） <br/>
        // 2. 进行交易的基本验证，若验证失败，则返回false  <br/>
        // </returns>

        /// <summary>
        /// Verify the transaction
        /// </summary>
        /// <param name="snapshot">Snapshot of database</param>
        /// <param name="mempool">Memory pool of transactions</param>
        /// <returns>
        /// 1. If the consuming gas can not be divide by 10^8 ,return false.(e.g. The gas must be the integer format of Fixed8, which means there is not decimal Gas.)<br/>
        /// 2. The basic verification of the transaction invocation. If not verified, return false.
        /// </returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            if (Gas.GetData() % 100000000 != 0) return false;
            return base.Verify(snapshot, mempool);
        }
    }
}
