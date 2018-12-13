using System;

namespace Neo.IO.Caching
{
    /// <summary>
    /// 反射缓存特性类
    /// </summary>
    public class ReflectionCacheAttribute : Attribute
    {
        // <summary>
        // Type
        // </summary>
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; private set; }

        // <summary>
        // Constructor
        // </summary>
        // <param name="type">Type</param>
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="type">类型</param>
        public ReflectionCacheAttribute(Type type)
        {
            Type = type;
        }
    }
}