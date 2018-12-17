using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 最普通交易（不是发布智能合约）
    /// </summary>
    public class ContractTransaction : Transaction
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ContractTransaction()
            : base(TransactionType.ContractTransaction)
        {
        }

        /// <summary>
        /// 反序列数据，未读取任何数据。只验证交易版本号为0
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        /// <exception cref="System.FormatException">若交易版本号不是0，抛出该异常</exception>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
        }
    }
}
