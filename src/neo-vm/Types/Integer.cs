using System;
using System.Linq;
using System.Numerics;

namespace Neo.VM.Types
{
    // <summary>
    // 定义了虚拟机Integer类型的相关方法
    // </summary>
    /// <summary>
    /// Virtual machine Integer type class
    /// </summary>
    public class Integer : StackItem
    {
        private BigInteger value;
        // <summary>
        // Integer构造函数
        // </summary>
        // <param name="value">BigInteger类型的值</param>
        /// <summary>
        /// Integer constructor
        /// </summary>
        /// <param name="value">BigInteger type value</param>
        public Integer(BigInteger value)
        {
            this.value = value;
        }
        // <summary>
        // 判断当前Integer与指定的堆栈项是否相等
        // </summary>
        // <param name="other">指定的堆栈项</param>
        // <returns>相等则返回true，否则返回false</returns>
        /// <summary>
        /// Determines whether the current Integer is equal to the specified StackItem
        /// </summary>
        /// <param name="other">specified StackItem</param>
        /// <returns>Return true if it is equal, false otherwise</returns>
        public override bool Equals(StackItem other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            if (other is Integer i) return value == i.value;
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
        // 获取对应的BigInteger
        // </summary>
        // <returns>返回当前Integer的值</returns>
        /// <summary>
        /// Get the corresponding BigInteger
        /// </summary>
        /// <returns>Return the value of current Integer</returns>
        public override BigInteger GetBigInteger()
        {
            return value;
        }
        // <summary>
        // 获取对应的布尔值
        // </summary>
        // <returns>如果当前Integer等于BigInteger.Zero则返回false,否则返回true</returns>
        /// <summary>
        /// Get the corresponding Boolean
        /// </summary>
        /// <returns>Return false if the current Integer is equal to BigInteger.Zero, otherwise return true</returns>
        public override bool GetBoolean()
        {
            return value != BigInteger.Zero;
        }
        // <summary>
        // 获取对应的字节数组
        // </summary>
        // <returns>返回Integer的值转换为的字节数组</returns>
        /// <summary>
        /// Get the corresponding byte array
        /// </summary>
        /// <returns>Return a byte array that converts Integer to</returns>
        public override byte[] GetByteArray()
        {
            return value.ToByteArray();
        }
    }
}
