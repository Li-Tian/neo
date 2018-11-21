using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;

namespace Neo.IO.Json
{
    /// <summary>
    /// JObject类的子类，重写了JObject中的一部分方法，用于与json字符串中string类型数据的相互转换
    /// </summary>
    public class JString : JObject
    {
        /// <summary>
        /// Value用于存储string类型数据
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// JString类的构造函数，用于构建JString对象
        /// </summary>
        /// <param name="value">Value的初始值</param>
        /// <exception cref="System.ArgumentNullException">参数value为空</exception>
        public JString(string value)
        {
            this.Value = value ?? throw new ArgumentNullException();
        }
        /// <summary>
        /// 重写了父类的相关方法，将JString对象转换成bool类型数据
        /// </summary>
        /// <returns>JString对象内部存储的Value的值（小写）为"0"、"f"、"false"、"n"、"no"、"off"时，
        /// 则输出false,否则输出true</returns>
        public override bool AsBoolean()
        {
            switch (Value.ToLower())
            {
                case "0":
                case "f":
                case "false":
                case "n":
                case "no":
                case "off":
                    return false;
                default:
                    return true;
            }
        }
        /// <summary>
        /// 将JString对象转成泛型枚举对象
        /// </summary>
        /// <typeparam name="T">泛型枚举对象的数据类型</typeparam>
        /// <param name="ignoreCase">保留</param>
        /// <returns>输出JString对象转成的泛型枚举对象</returns>
        /// <exception cref="System.InvalidCastException">转换失败时抛出</exception>
        public override T AsEnum<T>(bool ignoreCase = false)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), Value, ignoreCase);
            }
            catch
            {
                throw new InvalidCastException();
            }
        }
        /// <summary>
        /// 将JString对象转成double类型数据
        /// </summary>
        /// <returns>输出JString对象中Value值转成的double类型数据</returns>
        /// <exception cref="System.InvalidCastException">JString对象中Value值无法转成double类型数据</exception>
        public override double AsNumber()
        {
            try
            {
                return double.Parse(Value);
            }
            catch
            {
                throw new InvalidCastException();
            }
        }
        /// <summary>
        /// 将JString对象转string类型数据
        /// </summary>
        /// <returns>输出JString对象中Value值</returns>
        public override string AsString()
        {
            return Value;
        }
        /// <summary>
        /// 重写了父类的相关方法，判断JString对象能否转换成指定类型数据，允许JString对象与bool、枚举、double和string类型数据的互转
        /// </summary>
        /// <param name="type">指定数据类型</param>
        /// <returns>指定数据类型为bool、枚举、double和string时返回true,否则返回false</returns>
        public override bool CanConvertTo(Type type)
        {
            if (type == typeof(bool))
                return true;
            if (type.GetTypeInfo().IsEnum && Enum.IsDefined(type, Value))
                return true;
            if (type == typeof(double))
                return true;
            if (type == typeof(string))
                return true;
            return false;
        }
        /// <summary>
        /// 解析文本读取器中的数据，用文本读取器中的数据构建对应的JString对象
        /// </summary>
        /// <param name="reader">文本读取器</param>
        /// <returns>输出由文本读取器中的数据构建的JString对象</returns>
        /// <exception cref="System.FormatException">文本读取器中的数据去除换行符、空格符、制表符后，
        /// 不以双引号和单引号开始或单字节带符号位</exception>
        internal static JString Parse(TextReader reader)
        {
            SkipSpace(reader);
            char[] buffer = new char[4];
            char firstChar = (char)reader.Read();
            if (firstChar != '\"' && firstChar != '\'') throw new FormatException();
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                char c = (char)reader.Read();
                if (c == 65535) throw new FormatException();
                if (c == firstChar) break;
                if (c == '\\')
                {
                    c = (char)reader.Read();
                    if (c == 'u')
                    {
                        reader.Read(buffer, 0, 4);
                        c = (char)short.Parse(new string(buffer), NumberStyles.HexNumber);
                    }
                }
                sb.Append(c);
            }
            return new JString(sb.ToString());
        }
        /// <summary>
        /// JString转换成String类型数据
        /// </summary>
        /// <returns>输出JString对象中Value的值对应的字符串</returns>
        public override string ToString()
        {
            return $"\"{JavaScriptEncoder.Default.Encode(Value)}\"";
        }
    }
}
