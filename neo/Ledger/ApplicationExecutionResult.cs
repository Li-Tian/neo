using Neo.SmartContract;
using Neo.VM;

namespace Neo.Ledger
{
    // <summary>
    // NVM执行结果
    // </summary>
    /// <summary>
    /// A class descripts the NVM execution results
    /// </summary>
    public class ApplicationExecutionResult
    {
        // <summary>
        // 触发器类型
        // </summary>
        /// <summary>
        /// The type of trigger
        /// </summary>
        public TriggerType Trigger { get; internal set; }

        // <summary>
        // 执行的合约脚本hash
        // </summary>
        /// <summary>
        /// The hash of contract which the NVM executed 
        /// </summary>
        public UInt160 ScriptHash { get; internal set; }

        // <summary>
        // VM状态
        // </summary>
        /// <summary>
        /// The state of VM
        /// </summary>
        public VMState VMState { get; internal set; }

        // <summary>
        // Gas消耗
        // </summary>
        /// <summary>
        /// The Gas consumed for this execution
        /// </summary>
        public Fixed8 GasConsumed { get; internal set; }

        // <summary>
        // 栈数据
        // </summary>
        /// <summary>
        /// The data of stack
        /// </summary>
        public StackItem[] Stack { get; internal set; }

        // <summary>
        // 智能合约执行过程中，通过互操作服务 Runtime_Notify 向客户端发出的通知事件的列表
        // </summary>
        /// <summary>
        /// During the execution of smart contract, the list of notifications which sent from client throught the Runtime_Notify Interoperability service
        /// </summary>
        public NotifyEventArgs[] Notifications { get; internal set; }
    }
}
