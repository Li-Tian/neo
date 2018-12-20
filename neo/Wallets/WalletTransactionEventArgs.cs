using Neo.Network.P2P.Payloads;
using System;

namespace Neo.Wallets
{
    // <summary>
    // 钱包交易的事件参数
    // </summary>
    /// <summary>
    /// wallet transaction event arguments
    /// </summary>
    public class WalletTransactionEventArgs : EventArgs
    {
        // <summary>
        // 交易
        // </summary>
        /// <summary>
        /// transaction
        /// </summary>
        public Transaction Transaction;

        // <summary>
        // 相关的账户
        // </summary>
        /// <summary>
        /// related accounts
        /// </summary>
        public UInt160[] RelatedAccounts;

        // <summary>
        // 区块高度
        // </summary>
        /// <summary>
        /// block height
        /// </summary>
        public uint? Height;

        // <summary>
        // 时间戳
        // </summary>
        /// <summary>
        /// time stamp
        /// </summary>
        public uint Time;
    }
}
