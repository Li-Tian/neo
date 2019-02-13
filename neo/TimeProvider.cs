using System;

namespace Neo
{
    // <summary>
    // 时间访问功能提供工具类
    // </summary>
    /// <summary>
	/// A helper class that provides accessing current time.
    /// </summary>
    public class TimeProvider
    {
        private static readonly TimeProvider Default = new TimeProvider();
        // <summary>
        // TimeProvider 实例的静态引用。
        // </summary>
        /// <summary>
        /// A static reference to an instance of TimeProvider.
        /// </summary>
        public static TimeProvider Current { get; internal set; } = Default;
        // <summary>
        // 获取UTC当前时间
        // </summary>
        /// <summary>
        /// Get UTC current time.
        /// </summary>
        public virtual DateTime UtcNow => DateTime.UtcNow;

        internal static void ResetToDefault()
        {
            Current = Default;
        }
    }
}
