using System;
using System.IO;

namespace Neo.IO.Wrappers
{
    /// <summary>
    /// 可序列化对象的封装类，实现Equal等扩展功能
    /// 这是个抽象类
    /// </summary>
    /// <typeparam name="T">指定的数据类型</typeparam>
    public abstract class SerializableWrapper<T> : IEquatable<T>, IEquatable<SerializableWrapper<T>>, ISerializable
        where T : struct, IEquatable<T>
    {
        /// <summary>
        /// 封装的对象
        /// </summary>
        protected T value;
        /// <summary>
        /// 大小
        /// </summary>
        public abstract int Size { get; }
        /// <summary>
        /// 从2进制读取器反序列化出相应对象
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        public abstract void Deserialize(BinaryReader reader);
        /// <summary>
        /// 当前内部持有的对象是否等于另一个对象
        /// </summary>
        /// <param name="other">待比较的对象</param>
        /// <returns>比较结果，相等返回true,否则返回false</returns>
        public bool Equals(T other)
        {
            return value.Equals(other);
        }
        /// <summary>
        /// 判断当前SerializableWrapper对象是否与另一个SerializableWrapper对象相等
        /// </summary>
        /// <param name="other">待比较的SerializableWrapper对象</param>
        /// <returns>比较结果，相等返回true,否则返回false</returns>
        public bool Equals(SerializableWrapper<T> other)
        {
            return value.Equals(other.value);
        }
        /// <summary>
        /// 序列化方法
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        public abstract void Serialize(BinaryWriter writer);
    }
}
