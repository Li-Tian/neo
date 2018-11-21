namespace Neo.Plugins
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel : byte
    {
        /// <summary>
        /// 致命
        /// </summary>
        Fatal,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 警告
        /// </summary>
        Warning,

        /// <summary>
        /// 信息
        /// </summary>
        Info,

        /// <summary>
        /// 调试
        /// </summary>
        Debug
    }
}
