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
        /// none
        /// </summary>
        None = 0,

        /// <summary>
        /// 产量
        /// </summary>
        Constant = 0x01
    }
}
