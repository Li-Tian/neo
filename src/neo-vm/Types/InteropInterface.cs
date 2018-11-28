using System;

namespace Neo.VM.Types
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class InteropInterface : StackItem
    {
        public override byte[] GetByteArray()
        {
            throw new NotSupportedException();
        }

        public abstract T GetInterface<T>() where T : class;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">泛型，必须是引用类型</typeparam>
    public class InteropInterface<T> : InteropInterface
        where T : class
    {
        private T _object;
        /// <summary>
        /// InteropInterface构造函数
        /// </summary>
        /// <param name="value">泛型</param>
        public InteropInterface(T value)
        {
            this._object = value;
        }
        /// <summary>
        /// 判断当前InteropInterface与指定的堆栈项是否相等
        /// </summary>
        /// <param name="other">指定的堆栈项</param>
        /// <returns>相等则返回true，否则返回false</returns>
        public override bool Equals(StackItem other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!(other is InteropInterface<T> i)) return false;
            return _object.Equals(i._object);
        }
        /// <summary>
        /// 获取对应的布尔值
        /// </summary>
        /// <returns>_object不为空则返回true,否则返回false</returns>
        public override bool GetBoolean()
        {
            return _object != null;
        }

        public override I GetInterface<I>()
        {
            return _object as I;
        }

        /// <summary>
        /// 隐式类型转换
        /// </summary>
        /// <param name="interface">泛型</param>
        public static implicit operator T(InteropInterface<T> @interface)
        {
            return @interface._object;
        }
    }
}
