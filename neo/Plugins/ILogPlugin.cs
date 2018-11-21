namespace Neo.Plugins
{
    /// <summary>
    /// 日志插件
    /// </summary>
    public interface ILogPlugin
    {
        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="source">日志源</param>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志内容</param>
        void Log(string source, LogLevel level, string message);
    }
}
