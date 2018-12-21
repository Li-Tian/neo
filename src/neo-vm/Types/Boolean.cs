using System;
using System.Linq;
using System.Numerics;

namespace Neo.VM.Types
{
    // <summary>
    // 定义了虚拟机Boolean类型的相关方法
    // </summary>
    /// <summary>
    /// Virtual machine Boolean type class
    /// </summary>
    public class Boolean : StackItem
    {
        private static readonly byte[] TRUE = { 1 };
        private static readonly byte[] FALSE = new byte[0];

        private bool value;
        // <summary>
        // 构造函数
        // </summary>
        // <param name="value">一个bool类型的值</param>
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">a bool value</param>
        public Boolean(bool value)
        {
            this.value = value;
        }
        // <summary>
        // 判断当前Boolean是否与指定的堆栈项相等
        // </summary>
        // <param name="other">指定的堆栈项</param>
        // <returns>相等则返回true，否则返回false</returns>
        // <exception cref="System.NotSupportedException">调用StackItem对象GetByteArray出错时抛出</exception>
        /// <summary>
        /// Determine whether the current Boolean is equal to the specified StackItem
        /// </summary>
        /// <param name="other">specified StackItem</param>
        /// <returns>Return true if it is equal, false otherwise</returns>
        public override bool Equals(StackItem other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            if (other is Boolean b) return value == b.value;
            byte[] bytes_other;
            try
            {
                bytes_other = other.GetByteArray();
            }
            catch (NotSupportedException)
            {
                return false;
            }
            return GetByteArray().SequenceEqual(bytes_other);
        }
        // <summary>
        // 获取对应的BigInteger，true为BigInteger.One，false为BigInteger.Zero
        // </summary>
        // <returns>转换后的值。True:1, false:0</returns>
        /// <summary>
        /// Get the corresponding BigInteger, true is BigInteger.One, false is BigInteger.Zero
        /// </summary>
        /// <returns>The converted value. True: 1, false: 0</returns>
        public override BigInteger GetBigInteger()
        {
            return value ? BigInteger.One : BigInteger.Zero;
        }
        // <summary>
        // 获取对应的Boolean值
        // </summary>
        // <returns>Boolean对应的值</returns>
        /// <summary>
        /// Get the corresponding Boolean value
        /// </summary>
        /// <returns>corresponding Boolean value</returns>
        public override bool GetBoolean()
        {
            return value;
        }
        // <summary>
        // 获取Boolean对应的字节数组，TRUE或者FALSE
        // </summary>
        // <returns>对应的字节数组</returns>
        /// <summary>
        /// Get the corresponding byte array,TRUE or FALSE
        /// </summary>
        /// <returns>corresponding byte array</returns>
        public override byte[] GetByteArray()
        {
            return value ? TRUE : FALSE;
        }
    }
}
