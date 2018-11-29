using System;

namespace Neo.VM
{
    /// <summary>
    /// 虚拟机状态，一共有四种
    /// </summary>
    [Flags]
    public enum VMState : byte
    {
        /// <summary>
        /// 正常状态
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 停止状态，当调用栈为空，即所有脚本执行完毕后，会将虚拟机状态置为HALT
        /// </summary>
        HALT = 1 << 0,
        /// <summary>
        /// 错误状态，当指令操作出错时会将虚拟机状态置为FAULT
        /// </summary>
        FAULT = 1 << 1,
        /// <summary>
        /// 中断状态，一般用于智能合约的调试过程中
        /// </summary>
        BREAK = 1 << 2,
    }
}
