namespace Neo.VM
{
    // <summary>
    // 脚本表接口
    // </summary>
    /// <summary>
    /// ScriptTable interface
    /// </summary>
    public interface IScriptTable
    {
        // <summary>
        // 根据脚本哈希获取对应的脚本
        // </summary>
        // <param name="script_hash">脚本哈希</param>
        // <returns>对应的脚本</returns>
        /// <summary>
        /// Get the corresponding script according to the script hash
        /// </summary>
        /// <param name="script_hash">script hash</param>
        /// <returns>Corresponding script</returns>
        byte[] GetScript(byte[] script_hash);
    }
}
