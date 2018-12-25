using System;
using System.IO;

namespace Neo.IO.Json
{
    // <summary>
    // JObject类的子类，重写了JObject中的一部分方法，用于与json字符串中布尔类型数据的相互转换
    // </summary>
    /// <summary>
    /// A subclass of the JObject class that overrides some of the methods in JObject .
    /// It is used to convert to and from Boolean data in json strings
    /// </summary>
    public class JBoolean : JObject
    {
        // <summary>
        // Value用于存储布尔类型数据
        // </summary>
        /// <summary>
        /// It is used to store boolean type data
        /// </summary>
        public bool Value { get; private set; }
        // <summary>
        // JBoolean类的构造函数，用于构建JBoolean对象
        // </summary>
        // <param name="value">Value的初始值，默认为false</param>
        /// <summary>
        /// Constructor ，build a JBoolean object
        /// </summary>
        /// <param name="value">init value of Value，default is false</param>
        public JBoolean(bool value = false)
        {
            this.Value = value;
        }
        // <summary>
        // 重写了父类的相关方法，目的是将JBoolean对象转换成bool类型数据
        // </summary>
        // <returns>输出JBoolean对象内部存储的Value的值</returns>
        /// <summary>
        /// overrides the methods in JObject，the purpose is to convert the JBoolean object into bool type data
        /// </summary>
        /// <returns>Value</returns>
        public override bool AsBoolean()
        {
            return Value;
        }
        // <summary>
        // 重写了父类的相关方法，目的是将JBoolean对象转换成String类型数据
        // </summary>
        // <returns>输出JBoolean对象内部存储的Value的值转成的字符串（小写）</returns>
        /// <summary>
        /// overrides the methods in JObject，the purpose is to convert the JBoolean object into string type data
        /// </summary>
        /// <returns>string(lowercase)</returns>
        public override string AsString()
        {
            return Value.ToString().ToLower();
        }
        // <summary>
        // 重写了父类的相关方法，判断JBoolean对象能否转换成其他类型，允许JBoolean对象与bool和string类型数据互转
        // </summary>
        // <param name="type">指定数据类型</param>
        // <returns>指定数据类型为bool和string，则返回true，否则返回false</returns>
        /// <summary>
        /// overrides the methods in JObject.
        /// Determine if a JBoolean object can be converted to bool type data or string type data，
        /// </summary>
        /// <param name="type">specified type</param>
        /// <returns>If specified type is bool or string，return true.Otherwise,return false</returns>
        public override bool CanConvertTo(Type type)
        {
            if (type == typeof(bool))
                return true;
            if (type == typeof(string))
                return true;
            return false;
        }
        // <summary>
        // 解析文本读取器中的数据，用文本读取器中的数据构建对应的JBoolean对象
        // </summary>
        // <param name="reader">文本读取器</param>
        // <returns>输出由文本读取器中的数据构建的JBoolean对象</returns>
        // <exception cref="System.FormatException">文本读取器中的数据去除空格、换行符、制表符后
        // 不为"true"或者"false"</exception>
        /// <summary>
        /// Parse the data in the text reader and use it to build a JBoolean object
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <returns>a JBoolean object</returns>
        /// <exception cref="System.FormatException">
        /// Data in the text reader removes spaces, line breaks, and tabs does not equals "true" or "false"
        /// </exception>
        internal static JBoolean Parse(TextReader reader)
        {
            SkipSpace(reader);
            char firstChar = (char)reader.Read();
            if (firstChar == 't')
            {
                int c2 = reader.Read();
                int c3 = reader.Read();
                int c4 = reader.Read();
                if (c2 == 'r' && c3 == 'u' && c4 == 'e')
                {
                    return new JBoolean(true);
                }
            }
            else if (firstChar == 'f')
            {
                int c2 = reader.Read();
                int c3 = reader.Read();
                int c4 = reader.Read();
                int c5 = reader.Read();
                if (c2 == 'a' && c3 == 'l' && c4 == 's' && c5 == 'e')
                {
                    return new JBoolean(false);
                }
            }
            throw new FormatException();
        }
        // <summary>
        // JBoolean对象转换成String类型数据
        // </summary>
        // <returns>输出JBoolean对象中Value的值对应的小写字符串</returns>
        /// <summary>
        /// Convert a JBoolean object to a String type data
        /// </summary>
        /// <returns>string(lower case)</returns>
        public override string ToString()
        {
            return Value.ToString().ToLower();
        }
    }
}
