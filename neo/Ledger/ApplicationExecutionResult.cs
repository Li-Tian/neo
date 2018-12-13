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
        /// 触发器类型
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
        /// 栈数据
        /// </summary>
        public StackItem[] Stack { get; internal set; }

        /// <summary>
        /// 智能合约执行过程中，通过互操作服务 Runtime_Notify 向客户端发出的通知事件的列表
        /// </summary>
        public NotifyEventArgs[] Notifications { get; internal set; }
    }
}
