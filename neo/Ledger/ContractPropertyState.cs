using System;

namespace Neo.Ledger
{
    // <summary>
    // 智能合约属性状态
    // </summary>
    /// <summary>
    /// state of contract property
    /// </summary>
    [Flags]
    public enum ContractPropertyState : byte
    {
        // <summary>
        // 合约不包含属性
        // </summary>
        /// <summary>
        /// This contract do not have property
        /// </summary>
        NoProperty = 0,

        // <summary>
        // 包含存储区
        // </summary>
        /// <summary>
        /// This contract have storage
        /// </summary>
        HasStorage = 1 << 0,

        // <summary>
        // 动态调用
        // </summary>
        /// <summary>
        /// This contract can dynamically invoke other contract
        /// </summary>
        HasDynamicInvoke = 1 << 1,

        // <summary>
        // 可收款(保留功能)
        // </summary>
        /// <summary>
        /// Is payable(Developing)
        /// </summary>
        Payable = 1 << 2
    }
}
