namespace Neo.VM
{
    /// <summary>
    /// 脚本表接口
    /// </summary>
    public interface IScriptTable
    {
        /// <summary>
        /// 根据脚本哈希获取对应的脚本
        /// </summary>
        /// <param name="script_hash">脚本哈希</param>
        /// <returns>对应的脚本</returns>
        byte[] GetScript(byte[] script_hash);
    }
}
