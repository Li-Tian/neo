using Neo.VM;
using System;

namespace Neo.SmartContract
{
    // <summary>
    // 自定义事件数据类，用于互操作服务的日志功能等
    // </summary>
    /// <summary>
    /// Custom event class, log function for interoperable services, etc.
    /// </summary>
    public class LogEventArgs : EventArgs
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
        // 消息
        // </summary>
        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; }
        // <summary>
        // LogEventArgs构造函数
        // </summary>
        // <param name="container">脚本容器</param>
        // <param name="script_hash">脚本哈希</param>
        // <param name="message">消息</param>
        /// <summary>
        /// LogEventArgs constructor
        /// </summary>
        /// <param name="container">Script container</param>
        /// <param name="script_hash">Script hash</param>
        /// <param name="message">Message</param>
        public LogEventArgs(IScriptContainer container, UInt160 script_hash, string message)
        {
            this.ScriptContainer = container;
            this.ScriptHash = script_hash;
            this.Message = message;
        }
    }
}
