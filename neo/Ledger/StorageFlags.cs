using System;

namespace Neo.Ledger
{
    // <summary>
    // 存储标记
    // </summary>
    /// <summary>
    /// The flag for storage
    /// </summary>
    [Flags]
    public enum StorageFlags : byte
    {
        // <summary>
        // 无特殊标记
        // </summary>
        /// <summary>
        /// No special flag
        /// </summary>
        None = 0,

        // <summary>
        // 常量（一次写入不可修改）
        // </summary>
        /// <summary>
        /// Constant flag
        /// </summary>
        Constant = 0x01
    }
}
