﻿namespace Neo.VM
{
    /// <summary>
    /// 互操作服务接口
    /// </summary>
    public interface IInteropService
    {
        /// <summary>
        /// 调用互操作服务
        /// </summary>
        /// <param name="method">方法名称</param>
        /// <param name="engine">执行引擎</param>
        /// <returns>调用成功返回true,否则返回false</returns>
        bool Invoke(byte[] method, ExecutionEngine engine);
    }
}
