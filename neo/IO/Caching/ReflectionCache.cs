using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neo.IO.Caching
{
    // <summary>
    // 反射缓存类
    // </summary>
    // <typeparam name="T">泛型</typeparam>
    /// <summary>
    /// ReflectionCache class 
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public class ReflectionCache<T> : Dictionary<T, Type>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        // <summary>
        // 构造器
        // </summary>
        public ReflectionCache() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <typeparam name="EnumType">Enum type</typeparam>
        /// <exception cref="System.ArgumentException">Thrown when EnumType is not an enumerated type</exception>
        /// <exception cref="System.FormatException">Thrown if each attribute in EnumType is unavailable</exception>
        // <summary>
        // 从一个枚举类型创建实例
        // </summary>
        // <typeparam name="EnumType">枚举类型</typeparam>
        // <returns>反射缓存</returns>
        // <exception cref="System.ArgumentException">EnumType不是枚举类型时抛出</exception>
        // <exception cref="System.FormatException">EnumType中每个属性的注解无法获取时抛出</exception>
        public static ReflectionCache<T> CreateFromEnum<EnumType>() where EnumType : struct, IConvertible
        {
            Type enumType = typeof(EnumType);

            if (!enumType.GetTypeInfo().IsEnum)
                throw new ArgumentException("K must be an enumerated type");

            // Cache all types
            ReflectionCache<T> r = new ReflectionCache<T>();

            foreach (object t in Enum.GetValues(enumType))
            {
                // Get enumn member
                MemberInfo[] memInfo = enumType.GetMember(t.ToString());
                if (memInfo == null || memInfo.Length != 1)
                    throw (new FormatException());

                // Get attribute
                ReflectionCacheAttribute attribute = memInfo[0].GetCustomAttributes(typeof(ReflectionCacheAttribute), false)
                    .Cast<ReflectionCacheAttribute>()
                    .FirstOrDefault();

                if (attribute == null)
                    throw (new FormatException());

                // Append to cache
                r.Add((T)t, attribute.Type);
            }
            return r;
        }
        /// <summary>
        /// Create object from key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="def">Default value</param>
        // <summary>
        // 根据键创建对象。如果创建失败则返回默认对象。
        // </summary>
        // <param name="key">键</param>
        // <param name="def">默认值</param>
        // <returns>返回新创建的对象或者默认对象。</returns>
        public object CreateInstance(T key, object def = null)
        {
            Type tp;

            // Get Type from cache
            if (TryGetValue(key, out tp)) return Activator.CreateInstance(tp);

            // return null
            return def;
        }
        /// <summary>
        /// Create object from key
        /// </summary>
        /// <typeparam name="K">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="def">Default value</param>
        // <summary>
        // 根据键创建对象。创建失败则返回默认值。
        // </summary>
        // <typeparam name="K">类型</typeparam>
        // <param name="key">键</param>
        // <param name="def">默认值</param>
        // <returns>新创建的对象或者默认值</returns>
        public K CreateInstance<K>(T key, K def = default(K))
        {
            Type tp;

            // Get Type from cache
            if (TryGetValue(key, out tp)) return (K)Activator.CreateInstance(tp);

            // return null
            return def;
        }
    }
}