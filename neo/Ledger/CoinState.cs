using System;

namespace Neo.Ledger
{
    /// <summary>
    /// 一个代表了当前NEO状态的enum类
    /// </summary>
    [Flags]
    public enum CoinState : byte
    {
        /// <summary>
        /// 未确认的
        /// </summary>
        Unconfirmed = 0,

        /// <summary>
        /// 已确认的
        /// </summary>
        Confirmed = 1 << 0,

        /// <summary>
        /// 已经被支付给他人的
        /// </summary>
        Spent = 1 << 1,
        //Vote = 1 << 2,

        /// <summary>
        /// 已经被Claimed
        /// </summary>
        Claimed = 1 << 3,
        //Locked = 1 << 4,

        /// <summary>
        /// 锁仓中的
        /// </summary>
        Frozen = 1 << 5,
        //WatchOnly = 1 << 6,
    }
}
