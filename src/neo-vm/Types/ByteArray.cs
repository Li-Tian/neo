using System;
using System.Linq;

namespace Neo.VM.Types
{
    /// <summary>
    /// 定义了虚拟机ByteArray类型的相关方法
    /// </summary>
    public class ByteArray : StackItem
    {
        private byte[] value;
        /// <summary>
        /// ByteArray构造函数
        /// </summary>
        /// <param name="value">字节数组</param>
        public ByteArray(byte[] value)
        {
            this.value = value;
        }
        /// <summary>
        /// 判断当前ByteArray与指定的堆栈项是否相等
        /// </summary>
        /// <param name="other">指定的堆栈项</param>
        /// <returns>相等则返回true，否则返回false</returns>
        public override bool Equals(StackItem other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            byte[] bytes_other;
            try
            {
                bytes_other = other.GetByteArray();
            }
            catch (NotSupportedException)
            {
                return false;
            }
            return value.SequenceEqual(bytes_other);
        }
        /// <summary>
        /// 获取对应的字节数组
        /// </summary>
        /// <returns>返回ByteArray的值</returns>
        public override byte[] GetByteArray()
        {
            return value;
        }
    }
}
