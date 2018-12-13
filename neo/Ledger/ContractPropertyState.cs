using System;

namespace Neo.Ledger
{
    /// <summary>
    /// 智能合约属性状态
    /// </summary>
    [Flags]
    public enum ContractPropertyState : byte
    {
        /// <summary>
        /// 合约不包含属性
        /// </summary>
        NoProperty = 0,

        /// <summary>
        /// 包含存储区
        /// </summary>
        HasStorage = 1 << 0,

        /// <summary>
        /// 动态调用
        /// </summary>
        HasDynamicInvoke = 1 << 1,

        /// <summary>
        /// 可支付(保留功能)
        /// </summary>
        Payable = 1 << 2
    }
}
