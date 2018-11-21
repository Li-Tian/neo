using Neo.SmartContract;
using Neo.VM;

namespace Neo.Ledger
{
    /// <summary>
    /// NVM执行结果
    /// </summary>
    public class ApplicationExecutionResult
    {
        /// <summary>
        /// 触发器
        /// </summary>
        public TriggerType Trigger { get; internal set; }

        /// <summary>
        /// 执行的合约脚本hash
        /// </summary>
        public UInt160 ScriptHash { get; internal set; }

        /// <summary>
        /// VM状态
        /// </summary>
        public VMState VMState { get; internal set; }

        /// <summary>
        /// Gas消耗
        /// </summary>
        public Fixed8 GasConsumed { get; internal set; }

        /// <summary>
        /// 栈顶数据
        /// </summary>
        public StackItem[] Stack { get; internal set; }

        /// <summary>
        /// 执行过程中触发的事件
        /// </summary>
        public NotifyEventArgs[] Notifications { get; internal set; }
    }
}
