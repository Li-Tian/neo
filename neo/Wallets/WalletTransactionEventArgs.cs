using Neo.Network.P2P.Payloads;
using System;

namespace Neo.Wallets
{
    /// <summary>
    /// 钱包交易的事件参数
    /// </summary>
    public class WalletTransactionEventArgs : EventArgs
    {
        /// <summary>
        /// 交易
        /// </summary>
        public Transaction Transaction;

        /// <summary>
        /// 相关的账户
        /// </summary>
        public UInt160[] RelatedAccounts;

        /// <summary>
        /// 区块高度
        /// </summary>
        public uint? Height;

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Time;
    }
}
