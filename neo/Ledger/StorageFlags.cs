using System;

namespace Neo.Ledger
{
    /// <summary>
    /// 存储标记
    /// </summary>
    [Flags]
    public enum StorageFlags : byte
    {
        /// <summary>
        /// 无特殊标记
        /// </summary>
        None = 0,

        /// <summary>
        /// 常量（一次写入不可修改）
        /// </summary>
        Constant = 0x01
    }
}
