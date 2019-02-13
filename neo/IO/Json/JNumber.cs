using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Neo.IO.Json
{
    // <summary>
    // JObject类的子类，重写了JObject中的一部分方法，用于与json字符串中数值的相互转换
    // </summary>
    /// <summary>
    /// A subclass of the JObject class that overrides some of the methods in JObject .
    /// It is used to convert to and from number data in json strings
    /// </summary>
    public class JNumber : JObject
    {
        // <summary>
        // Value用于存储double类型数值
        // </summary>
        /// <summary>
        /// It is used to store double type data
        /// </summary>
        public double Value { get; private set; }
        // <summary>
        // JNumber类的构造函数，用于构建JNumber对象
        // </summary>
        // <param name="value">Value的初始值，默认为0</param>
        /// <summary>
        /// constructor，build a JNumber object
        /// </summary>
        /// <param name="value">init value of Value，default is 0</param>
        public JNumber(double value = 0)
        {
            this.Value = value;
        }
        // <summary>
        // 重写了父类的相关方法，将JNumber对象转换成bool类型数据
        // </summary>
        // <returns>JNumber对象内部存储的Value的值为0，则输出false,否则输出true</returns>
        /// <summary>
        /// overrides the method in JObject，the purpose is to convert the JBoolean object into bool type data
        /// </summary>
        /// <returns>If the value of Value is 0,return false.Otherwise,return true</returns>
        public override bool AsBoolean()
        {
            if (Value == 0)
                return false;
            return true;
        }
        // <summary>
        // 重写了父类的相关方法，将JNumber对象转换成泛型枚举类型数据，
        // 可指定的类型有byte、int、long、sbyte、short、uint、ulong、ushort
        // </summary>
        // <typeparam name="T">泛型枚举类型的类型</typeparam>
        // <param name="ignoreCase">保留</param>
        // <returns>输出JNumber对象转换成泛型枚举类型数据</returns>
        // <exception cref="System.InvalidCastException">
        // 泛型枚举类型的类型为byte、int、long、sbyte、short、uint、ulong、ushort以外的类型
        // </exception>
        /// <summary>
        /// overrides the method in JObject，convert JNumber object to generic enumerated type data，
        /// specified type is  within the range of byte、int、long、sbyte、short、uint、ulong、ushort
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="ignoreCase">A flag marks if ignore case</param>
        /// <returns>specified type object</returns>
        /// <exception cref="System.InvalidCastException">
        /// specified type is not within the range of byte、int、long、sbyte、short、uint、ulong、ushort
        /// </exception>
        public override T AsEnum<T>(bool ignoreCase = false)
        {
            Type t = typeof(T);
            TypeInfo ti = t.GetTypeInfo();
            if (!ti.IsEnum)
                throw new InvalidCastException();
            if (ti.GetEnumUnderlyingType() == typeof(byte))
                return (T)Enum.ToObject(t, (byte)Value);
            if (ti.GetEnumUnderlyingType() == typeof(int))
                return (T)Enum.ToObject(t, (int)Value);
            if (ti.GetEnumUnderlyingType() == typeof(long))
                return (T)Enum.ToObject(t, (long)Value);
            if (ti.GetEnumUnderlyingType() == typeof(sbyte))
                return (T)Enum.ToObject(t, (sbyte)Value);
            if (ti.GetEnumUnderlyingType() == typeof(short))
                return (T)Enum.ToObject(t, (short)Value);
            if (ti.GetEnumUnderlyingType() == typeof(uint))
                return (T)Enum.ToObject(t, (uint)Value);
            if (ti.GetEnumUnderlyingType() == typeof(ulong))
                return (T)Enum.ToObject(t, (ulong)Value);
            if (ti.GetEnumUnderlyingType() == typeof(ushort))
                return (T)Enum.ToObject(t, (ushort)Value);
            throw new InvalidCastException();
        }
        // <summary>
        // 重写了父类的相关方法，将JNumber对象转换成double类型数据
        // </summary>
        // <returns>输出JNumber对象内Value的值</returns>
        /// <summary>
        /// overrides the method in JObject，convert JNumber object to double type data
        /// </summary>
        /// <returns>return value of Value</returns>
        public override double AsNumber()
        {
            return Value;
        }
        // <summary>
        // 重写了父类的相关方法，将JNumber对象转换成string类型数据
        // </summary>
        // <returns>输出JNumber对象内Value的值对应的字符串</returns>
        /// <summary>
        /// overrides the method in JObject，convert JNumber object to string type data
        /// </summary>
        /// <returns>the string corresponding to the value of Value</returns>
        public override string AsString()
        {
            return Value.ToString();
        }
        // <summary>
        // 重写了父类的相关方法，判断JBoolean对象能否转换成其他类型数据，
        // 允许JNumber对象与bool、double和string类型数据的互转
        // </summary>
        // <param name="type">指定的数据类型</param>
        // <returns>指定的数据能转换则返回true，否则返回false</returns>
        /// <summary>
        /// overrides the method in JObject，
        /// determine if a JBoolean object could be converted to a string type,bool type or double type data
        /// </summary>
        /// <param name="type">specified type</param>
        /// <returns>If it could be converted,otherwise，return false</returns>
        public override bool CanConvertTo(Type type)
        {
            if (type == typeof(bool))
                return true;
            if (type == typeof(double))
                return true;
            if (type == typeof(string))
                return true;
            TypeInfo ti = type.GetTypeInfo();
            if (ti.IsEnum && Enum.IsDefined(type, Convert.ChangeType(Value, ti.GetEnumUnderlyingType())))
                return true;
            return false;
        }
        // <summary>
        // 解析文本读取器中的数据，用文本读取器中的数据构建对应的JNumber对象
        // </summary>
        // <param name="reader">文本读取器</param>
        // <returns>输出由文本读取器中的数据构建的JNumber对象</returns>
        /// <summary>
        /// Parse the data in the text reader and use it to build a JNumber object
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <returns>JNumber object</returns>
        internal static JNumber Parse(TextReader reader)
        {
            SkipSpace(reader);
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                char c = (char)reader.Peek();
                if (c >= '0' && c <= '9' || c == '.' || c == '-')
                {
                    sb.Append(c);
                    reader.Read();
                }
                else
                {
                    break;
                }
            }
            return new JNumber(double.Parse(sb.ToString()));
        }
        // <summary>
        // JNumber对象转换成String类型数据
        // </summary>
        // <returns>输出JNumber对象中Value的值对应的字符串</returns>
        /// <summary>
        /// Convert a JNumber object to a string type data
        /// </summary>
        /// <returns>the string corresponding to the value of Value</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
        // <summary>
        // JNumber对象转换成DateTime类型的时间戳
        // </summary>
        // <returns>输出JNumber对象转换的DateTime类型时间戳</returns>
        // <exception cref="System.InvalidCastException">JNumber对象内Value的值小于0或大于ulong类型所能标识的最大值</exception>
        /// <summary>
        /// Convert a JNumber object to a DateTime time stamp
        /// </summary>
        /// <returns>a DateTime time stamp</returns>
        /// <exception cref="System.InvalidCastException">If value of Value less than 0 or larger than the max value of ulong type data</exception>
        public DateTime ToTimestamp()
        {
            if (Value < 0 || Value > ulong.MaxValue)
                throw new InvalidCastException();
            return ((ulong)Value).ToDateTime();
        }
    }
}
