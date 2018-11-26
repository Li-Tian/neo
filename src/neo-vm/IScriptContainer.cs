namespace Neo.VM
{
    /// <summary>
    /// 脚本容器接口
    /// </summary>
    public interface IScriptContainer
    {
        /// <summary>
        /// 获取脚本容器消息
        /// </summary>
        /// <returns>消息信息</returns>
        byte[] GetMessage();
    }
}
