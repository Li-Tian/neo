using System;

namespace Neo.VM.Types
{
    // <summary>
    // 定义了虚拟机InteropInterface（互操作服务接口）类型的相关方法
    // </summary>
    /// <summary>
    /// Virtual machine InteropInterface type class
    /// </summary>
    public abstract class InteropInterface : StackItem
    {
        // <summary>
        // 获取字节数组
        // </summary>
        // <returns>字节数组</returns>
        // <exception cref="System.NotSupportedException">默认抛出</exception>
        /// <summary>
        /// Get the corresponding byte array
        /// </summary>
        /// <returns>Throw exception</returns>
        public override byte[] GetByteArray()
        {
            throw new NotSupportedException();
        }
        // <summary>
        // 转换成指定的类型
        // </summary>
        // <typeparam name="T">指定的类型</typeparam>
        // <returns>转换成指定类型的对象</returns>
        /// <summary>
        /// Convert to the specified type
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <returns>Object of the specified type</returns>
        public abstract T GetInterface<T>() where T : class;
    }
    // <summary>
    // 定义了虚拟机InteropInterface（互操作服务接口）泛型类型的相关方法
    // </summary>
    // <typeparam name="T">泛型，必须是引用类型</typeparam>
    /// <summary>
    /// Defines the associated method for the generic type of the virtual machine InteropInterface
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public class InteropInterface<T> : InteropInterface
        where T : class
    {
        private T _object;
        // <summary>
        // InteropInterface构造函数
        // </summary>
        // <param name="value">泛型</param>
        /// <summary>
        /// InteropInterface constructor
        /// </summary>
        /// <param name="value">T</param>
        public InteropInterface(T value)
        {
            this._object = value;
        }
        // <summary>
        // 判断当前InteropInterface与指定的堆栈项是否相等
        // </summary>
        // <param name="other">指定的堆栈项</param>
        // <returns>相等则返回true，否则返回false</returns>
        /// <summary>
        /// Determines whether the current InteropInterface is equal to the specified StackItem
        /// </summary>
        /// <param name="other">specified StackItem</param>
        /// <returns>Return true if it is equal, false otherwise</returns>
        public override bool Equals(StackItem other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!(other is InteropInterface<T> i)) return false;
            return _object.Equals(i._object);
        }
        // <summary>
        // 获取对应的布尔值
        // </summary>
        // <returns>_object不为空则返回true,否则返回false</returns>
        /// <summary>
        /// Get the corresponding Boolean value
        /// </summary>
        /// <returns>Return true if the _object is not null, false otherwise.</returns>
        public override bool GetBoolean()
        {
            return _object != null;
        }
        // <summary>
        // 转换成为指定的数据类型接口
        // </summary>
        // <typeparam name="I">指定的数据类型接口</typeparam>
        // <returns>指定的数据类型接口的实例</returns>
        /// <summary>
        /// Convert to the specified data type interface
        /// </summary>
        /// <typeparam name="I">I</typeparam>
        /// <returns>An instance of the specified data type interface</returns>
        public override I GetInterface<I>()
        {
            return _object as I;
        }

        // <summary>
        // 隐式类型转换
        // </summary>
        // <param name="interface">泛型</param>
        /// <summary>
        /// Implicit type conversion
        /// </summary>
        /// <param name="interface">T</param>
        public static implicit operator T(InteropInterface<T> @interface)
        {
            return @interface._object;
        }
    }
}
