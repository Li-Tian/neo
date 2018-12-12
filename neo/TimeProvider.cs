using System;

namespace Neo
{
    /// <summary>
    /// 时间访问功能提供工具类
    /// </summary>
    public class TimeProvider
    {
        private static readonly TimeProvider Default = new TimeProvider();
        /// <summary>
        /// TimeProvider 实例的静态引用。
        /// </summary>
        public static TimeProvider Current { get; internal set; } = Default;
        /// <summary>
        /// 获取UTC当前时间
        /// </summary>
        public virtual DateTime UtcNow => DateTime.UtcNow;

        internal static void ResetToDefault()
        {
            Current = Default;
        }
    }
}
