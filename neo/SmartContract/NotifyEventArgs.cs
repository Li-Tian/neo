using Neo.VM;
using System;

namespace Neo.SmartContract
{
    // <summary>
    // 自定义事件数据类，用于互操作服务的通知功能等
    // </summary>
    /// <summary>
    /// Custom data class, notification function for interoperable services, etc.
    /// </summary>
    public class NotifyEventArgs : EventArgs
    {
        // <summary>
        // 脚本容器
        // </summary>
        /// <summary>
        /// Script container
        /// </summary>
        public IScriptContainer ScriptContainer { get; }
        // <summary>
        // 脚本哈希
        // </summary>
        /// <summary>
        /// Script hash
        /// </summary>
        public UInt160 ScriptHash { get; }
        // <summary>
        // 虚拟机栈状态
        // </summary>
        /// <summary>
        /// Virtual machine stack status
        /// </summary>
        public StackItem State { get; }
        // <summary>
        // NotifyEventArgs构造函数
        // </summary>
        // <param name="container">脚本容器</param>
        // <param name="script_hash">脚本哈希</param>
        // <param name="state">虚拟机栈状态</param>
        /// <summary>
        /// NotifyEventArgs constructor
        /// </summary>
        /// <param name="container">Script container</param>
        /// <param name="script_hash">Script hash</param>
        /// <param name="state">Virtual machine stack status</param>
        public NotifyEventArgs(IScriptContainer container, UInt160 script_hash, StackItem state)
        {
            this.ScriptContainer = container;
            this.ScriptHash = script_hash;
            this.State = state;
        }
    }
}
