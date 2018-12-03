using System;

namespace Neo.Network.RPC
{
    /// <summary>
    /// RPC服务的Exception类
    /// </summary>
    public class RpcException : Exception
    {   
        /// <summary>
        /// RPC服务异常, 当Rpc服务执行遇到错误时抛出
        /// </summary>
        /// <param name="code">RPC服务的错误代码</param>
        /// <param name="message">错误信息</param>
        public RpcException(int code, string message) : base(message)
        {
            HResult = code;
        }
    }
}
