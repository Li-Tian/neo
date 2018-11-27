using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Array = Neo.VM.Types.Array;
using Boolean = Neo.VM.Types.Boolean;

namespace Neo.VM
{
    /// <summary>
    /// 堆栈项抽象类，定义了堆栈项的一些抽象方法和隐式转换方法
    /// </summary>
    public abstract class StackItem : IEquatable<StackItem>
    {
        /// <summary>
        /// 抽象函数，判断两个堆栈项是否相等
        /// </summary>
        /// <param name="other">另一个堆栈项</param>
        /// <returns>相等返回true,不相等返回false</returns>
        public abstract bool Equals(StackItem other);
        /// <summary>
        /// 判断指定的对象是否等于当前对象
        /// </summary>
        /// <param name="obj">指定对象</param>
        /// <returns>相等返回true,不相等返回false</returns>
        public sealed override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj is StackItem other)
                return Equals(other);
            return false;
        }
        /// <summary>
        /// 将一个Class对象转换为堆栈项
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="value">需要转换的对象</param>
        /// <returns>一个互操作接口类型的堆栈项</returns>
        public static StackItem FromInterface<T>(T value)
            where T : class
        {
            return new InteropInterface<T>(value);
        }
        /// <summary>
        /// 从堆栈项获取一个任意大的带符号整数
        /// </summary>
        /// <returns>对应的任意大的带符号整数</returns>
        public virtual BigInteger GetBigInteger()
        {
            return new BigInteger(GetByteArray());
        }
        /// <summary>
        /// 从堆栈项获取布尔值
        /// </summary>
        /// <returns>布尔值</returns>
        public virtual bool GetBoolean()
        {
            return GetByteArray().Any(p => p != 0);
        }
        /// <summary>
        /// 从堆栈项获取字节数组，抽象方法
        /// </summary>
        /// <returns>字节数组</returns>
        public abstract byte[] GetByteArray();
        /// <summary>
        /// 获取HashCode
        /// </summary>
        /// <returns>计算得到的HAshCode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (byte element in GetByteArray())
                    hash = hash * 31 + element;
                return hash;
            }
        }
        /// <summary>
        /// 从堆栈项获取字符串
        /// </summary>
        /// <returns>得到的字符串</returns>
        public virtual string GetString()
        {
            return Encoding.UTF8.GetString(GetByteArray());
        }
        /// <summary>
        /// 隐式类型转换，将int类型转换为StackItem类型
        /// </summary>
        /// <param name="value">int类型值</param>
        public static implicit operator StackItem(int value)
        {
            return (BigInteger)value;
        }
        /// <summary>
        /// 隐式类型转换，将uint类型转换为StackItem类型
        /// </summary>
        /// <param name="value">uint类型值</param>
        public static implicit operator StackItem(uint value)
        {
            return (BigInteger)value;
        }
        /// <summary>
        /// 隐式类型转换，将long类型转换为StackItem类型
        /// </summary>
        /// <param name="value">long类型值</param>
        public static implicit operator StackItem(long value)
        {
            return (BigInteger)value;
        }
        /// <summary>
        /// 隐式类型转换，将ulong类型转换为StackItem类型
        /// </summary>
        /// <param name="value">ulong类型值</param>
        public static implicit operator StackItem(ulong value)
        {
            return (BigInteger)value;
        }
        /// <summary>
        /// 隐式类型转换，将BigInteger类型转换为StackItem类型
        /// </summary>
        /// <param name="value">BigInteger类型值</param>
        public static implicit operator StackItem(BigInteger value)
        {
            return new Integer(value);
        }
        /// <summary>
        /// 隐式类型转换，将bool类型转换为StackItem类型
        /// </summary>
        /// <param name="value">bool类型值</param>
        public static implicit operator StackItem(bool value)
        {
            return new Boolean(value);
        }
        /// <summary>
        /// 隐式类型转换，将byte[]类型转换为StackItem类型
        /// </summary>
        /// <param name="value">byte[]类型值</param>
        public static implicit operator StackItem(byte[] value)
        {
            return new ByteArray(value);
        }
        /// <summary>
        /// 隐式类型转换，将string类型转换为StackItem类型
        /// </summary>
        /// <param name="value">string类型值</param>
        public static implicit operator StackItem(string value)
        {
            return new ByteArray(Encoding.UTF8.GetBytes(value));
        }
        /// <summary>
        /// 隐式类型转换，将StackItem[]类型转换为StackItem类型
        /// </summary>
        /// <param name="value">StackItem[]类型值</param>
        public static implicit operator StackItem(StackItem[] value)
        {
            return new Array(value);
        }
        /// <summary>
        /// 隐式类型转换，将List<StackItem>类型转换为StackItem类型
        /// </summary>
        /// <param name="value">List<StackItem>类型值</param>
        public static implicit operator StackItem(List<StackItem> value)
        {
            return new Array(value);
        }
    }
}
