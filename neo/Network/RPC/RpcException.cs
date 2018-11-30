using System;

namespace Neo.Network.RPC
{
    /// <summary>
    /// RPC 服务器的Exception类
    /// </summary>
    public class RpcException : Exception
    {   
        /// <summary>
        /// RPC异常, 当RpcServer遇到错误时抛出
        /// </summary>
        /// <param name="code">RPC的错误代码</param>
        /// <param name="message">错误信息</param>
        public RpcException(int code, string message) : base(message)
        {
            HResult = code;
        }
    }
}
