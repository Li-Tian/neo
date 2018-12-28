namespace Neo.Plugins
{
    // <summary>
    // 日志插件
    // </summary>
    /// <summary>
    /// Log plugin
    /// </summary>
    public interface ILogPlugin
    {
        // <summary>
        // 日志输出
        // </summary>
        // <param name="source">日志源</param>
        // <param name="level">日志级别</param>
        // <param name="message">日志内容</param>
        /// <summary>
        /// Log output
        /// </summary>
        /// <param name="source">Log source</param>
        /// <param name="level">Log level</param>
        /// <param name="message">Log message</param>
        void Log(string source, LogLevel level, string message);
    }
}
