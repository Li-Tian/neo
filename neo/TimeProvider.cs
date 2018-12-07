using System;

namespace Neo
{
    /// <summary>
    /// ʱ����ʹ����ṩ������
    /// </summary>
    public class TimeProvider
    {
        private static readonly TimeProvider Default = new TimeProvider();
        /// <summary>
        /// TimeProvider ʵ���ľ�̬���á�
        /// </summary>
        public static TimeProvider Current { get; internal set; } = Default;
        /// <summary>
        /// ��ȡUTC��ǰʱ��
        /// </summary>
        public virtual DateTime UtcNow => DateTime.UtcNow;

        internal static void ResetToDefault()
        {
            Current = Default;
        }
    }
}
