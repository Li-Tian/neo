using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Neo.IO.Json
{
    // <summary>
    // 一个代表json对象类型的类。内部由一个键值对集合构成。可用于json字符串与json对象的相互转换
    // </summary>
    /// <summary>
    /// A class that represents the json object type. It consists of a collection of key-value pairs. 
    /// Can be used to convert json strings to json objects
    /// </summary>
    public class JObject
    {
        // <summary>
        // Null值代表了JObject的默认空值。
        // </summary>
        /// <summary>
        /// The Null represents the default null value of JObject。
        /// </summary>
        public static readonly JObject Null = null;

        private Dictionary<string, JObject> properties = new Dictionary<string, JObject>();
        // <summary>
        // 对JObject对象set、get方法的扩展，使外部方法可以便捷的操作properties字典
        // </summary>
        // <param name="name">属性字典的键</param>
        // <returns>属性字典的键所对应的值</returns>
        /// <summary>
        /// Extension of JObject object set and get methods, 
        /// so that external methods can easily manipulate the properties dictionary
        /// </summary>
        /// <param name="name">key</param>
        /// <returns>value</returns>
        public JObject this[string name]
        {
            get
            {
                properties.TryGetValue(name, out JObject value);
                return value;
            }
            set
            {
                properties[name] = value;
            }
        }
        // <summary>
        // 向外部提供一个只读的属性字典
        // </summary>
        /// <summary>
        /// Provide a read-only property dictionary to the outside
        /// </summary>
        public IReadOnlyDictionary<string, JObject> Properties => properties;
        // <summary>
        // 将JObject对象转换成bool类型数据，默认会抛出类型转换异常
        // </summary>
        // <returns>输出JObject对象转换成的bool类型数据，默认无返回</returns>
        // <exception cref="System.InvalidCastException">默认抛出</exception>
        /// <summary>
        /// Convert a JObject object to a bool type data, which will throw a type InvalidCastException exception by default.
        /// </summary>
        /// <returns>a bool type data.No return by default</returns>
        /// <exception cref="System.InvalidCastException">thrown by default</exception>
        public virtual bool AsBoolean()
        {
            throw new InvalidCastException();
        }
        // <summary>
        // 将JObject对象转换成bool类型数据，默认返回布尔值类型参数。
        // 子类如需重写该方法，需要同时重写CanConvertTo和AsBoolean方法
        // </summary>
        // <param name="value">默认布尔值类型参数</param>
        // <returns>返回默认布尔值类型参数</returns>
        /// <summary>
        /// Convert a JObject object to a bool type data.
        /// Subclasses need to override this method, you need to override both CanConvertTo and AsBoolean methods.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>bool type data</returns>
        public bool AsBooleanOrDefault(bool value = false)
        {
            if (!CanConvertTo(typeof(bool)))
                return value;
            return AsBoolean();
        }
        // <summary>
        // 将JObject对象转换成泛型枚举对象，默认会抛出类型转换异常。
        // 具体行为在子类中定义。
        // </summary>
        // <typeparam name="T">泛型枚举对象的类型参数</typeparam>
        // <param name="ignoreCase">忽略大小写（保留）</param>
        // <returns>输出JObject对象转换成泛型枚举对象，默认无返回</returns>
        // <exception cref="System.InvalidCastException">默认抛出</exception>
        /// <summary>
        /// Convert a JObject object to a generic enumeration type data.
        /// Thrown InvalidCastException exception by default.
        /// Specific behavior is defined in a subclass。
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="ignoreCase">a flag marks if ignore case</param>
        /// <returns>a generic enumeration type data</returns>
        /// <exception cref="System.InvalidCastException">Thrown by default</exception>
        public virtual T AsEnum<T>(bool ignoreCase = false)
        {
            throw new InvalidCastException();
        }

        // <summary>
        // 将JObject转换成泛型枚举类型对象，默认返回默认泛型枚举类型参数。
        // 子类如需重写该方法，需要同时重写CanConvertTo和AsBoolean方法
        // </summary>
        // <typeparam name="T">泛型具体类型</typeparam>
        // <param name="value">默认泛型枚举类型参数</param>
        // <param name="ignoreCase">忽略大小写</param>
        // <returns>输出JObject对象转换成泛型枚举对象，默认返回默认泛型枚举类型参数</returns>
        /// <summary>
        /// Convert a JObject object to a generic enumeration type data
        /// Subclasses need to override this method, you need to override both CanConvertTo and AsBoolean methods.
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="value">value</param>
        /// <param name="ignoreCase">a flag marks if ignose case</param>
        /// <returns>a generic enumeration type data</returns>
        public T AsEnumOrDefault<T>(T value = default(T), bool ignoreCase = false)
        {
            if (!CanConvertTo(typeof(T)))
                return value;
            return AsEnum<T>(ignoreCase);
        }
        // <summary>
        // 将JObject对象转换成double类型数据，默认会抛出类型转换异常。
        // 子类需要覆盖此方法，否则抛出异常。
        // </summary>
        // <returns>输出JObject对象转换成double类型数据，默认无返回</returns>
        // <exception cref="System.InvalidCastException">默认抛出</exception>
        /// <summary>
        /// Convert a JObject object to a double type data.Throw InvalidCastException exception by default.
        /// This method should be overwrote in subclass.
        /// </summary>
        /// <returns>a double type data.No return,by default</returns>
        /// <exception cref="System.InvalidCastException">Thrown by default</exception>
        public virtual double AsNumber()
        {
            throw new InvalidCastException();
        }
        // <summary>
        // 将JObject对象转换成double类型数据，默认返回默认double类型参数。
        // 子类同时重写CanConvertTo和AsNumber方法。
        // </summary>
        // <param name="value">默认double类型参数</param>
        // <returns>输出JObject对象转换成的double类型数据，默认返回默认double类型参数</returns>
        /// <summary>
        /// Convert a JObject object to a double type data
        /// Method CanConvertTo and AsString should be overwrote in subclass.
        /// </summary>
        /// <param name="value">a double type data</param>
        /// <returns>a double type data</returns>
        public double AsNumberOrDefault(double value = 0)
        {
            if (!CanConvertTo(typeof(double)))
                return value;
            return AsNumber();
        }
        // <summary>
        // 将JObject对象转换成String类型数据，默认会抛出类型转换异常
        // </summary>
        // <returns>输出JObject对象转换成String类型数据，默认无返回</returns>
        // <exception cref="System.InvalidCastException">默认抛出</exception>
        /// <summary>
        /// Convert a JObject object to a string type data，Throw InvalidCastException exception by default.
        /// </summary>
        /// <returns>a string type data.No return by default</returns>
        /// <exception cref="System.InvalidCastException">Thrown by default</exception>
        public virtual string AsString()
        {
            throw new InvalidCastException();
        }
        // <summary>
        // 将JObject对象转换成string类型数据，需要子类同时重写CanConvertTo和AsString方法，否则返回默认参数值
        // </summary>
        // <param name="value">默认string类型参数</param>
        // <returns>输出JObject对象转换成String类型数据，默认返回默认string类型参数</returns>
        /// <summary>
        /// Convert JObject object to string type data，
        /// Method CanConvertTo and AsString should be overwrote in subclass.
        /// Otherwise,return false.
        /// </summary>
        /// <param name="value">string type data</param>
        /// <returns>string type data</returns>
        public string AsStringOrDefault(string value = null)
        {
            if (!CanConvertTo(typeof(string)))
                return value;
            return AsString();
        }
        // <summary>
        // 判断JObject能否转换成其他类型数据，默认认为不能
        // </summary>
        // <param name="type">指定想要转换的数据类型</param>
        // <returns>判断结果，默认false,禁止与其他类型互转</returns>
        /// <summary>
        /// Determine whether the JObject could be convert to other types of data.
        /// By default, can't
        /// </summary>
        /// <param name="type">specified type</param>
        /// <returns>By default, can't.Forbid convert JObject to other types of data</returns>
        public virtual bool CanConvertTo(Type type)
        {
            return false;
        }
        // <summary>
        // 判断JObject对象内部的properties字典是否有对应的键
        // </summary>
        // <param name="key">键值对的键</param>
        // <returns>判断结果，properties字典中存在对应key，则返回true,否则返回false</returns>
        /// <summary>
        /// Determine whether the properties dictionary inside the JObject object has a specified key
        /// </summary>
        /// <param name="key">specified key</param>
        /// <returns>If dictionary contains specified key,return true.Otherwise,return false</returns>
        public bool ContainsProperty(string key)
        {
            return properties.ContainsKey(key);
        }
        // <summary>
        // 解析文本读取器中的数据，用文本读取器中的数据构建对应的JObject对象，JObject对象内部最大嵌套层数为100
        // </summary>
        // <param name="reader">文本读取器</param>
        // <param name="max_nest">对象内部最大嵌套层数</param>
        // <returns>输出文本读取器中的数据构建的JObject对象</returns>
        /// <summary>
        /// Parsing data in a text reader and use it to build a JObject object. The max nesting level is 100.
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <param name="max_nest">max nesting level</param>
        /// <returns>JObject object</returns>
        public static JObject Parse(TextReader reader, int max_nest = 100)
        {
            if (max_nest < 0) throw new FormatException();
            SkipSpace(reader);
            char firstChar = (char)reader.Peek();
            if (firstChar == '\"' || firstChar == '\'')
            {
                return JString.Parse(reader);
            }
            if (firstChar == '[')
            {
                return JArray.Parse(reader, max_nest);
            }
            if ((firstChar >= '0' && firstChar <= '9') || firstChar == '-')
            {
                return JNumber.Parse(reader);
            }
            if (firstChar == 't' || firstChar == 'f')
            {
                return JBoolean.Parse(reader);
            }
            if (firstChar == 'n')
            {
                return ParseNull(reader);
            }
            if (reader.Read() != '{') throw new FormatException();
            SkipSpace(reader);
            JObject obj = new JObject();
            while (reader.Peek() != '}')
            {
                if (reader.Peek() == ',') reader.Read();
                SkipSpace(reader);
                string name = JString.Parse(reader).Value;
                SkipSpace(reader);
                if (reader.Read() != ':') throw new FormatException();
                JObject value = Parse(reader, max_nest - 1);
                obj.properties.Add(name, value);
                SkipSpace(reader);
            }
            reader.Read();
            return obj;
        }
        // <summary>
        // 解析字符串，用字符串中的数据构建对应的JObject对象，JObject对象内部最大嵌套层数为100
        // </summary>
        // <param name="value">输入的字符串</param>
        // <param name="max_nest">对象内部最大嵌套层数</param>
        // <returns>字符串中的数据构建对应的JObject对象</returns>
        /// <summary>
        /// Parse the string and use it to build a JObject object. The max nesting level is 100.
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="max_nest">max nesting level</param>
        /// <returns>JObject object</returns>
        public static JObject Parse(string value, int max_nest = 100)
        {
            using (StringReader reader = new StringReader(value))
            {
                return Parse(reader, max_nest);
            }
        }

        private static JObject ParseNull(TextReader reader)
        {
            char firstChar = (char)reader.Read();
            if (firstChar == 'n')
            {
                int c2 = reader.Read();
                int c3 = reader.Read();
                int c4 = reader.Read();
                if (c2 == 'u' && c3 == 'l' && c4 == 'l')
                {
                    return null;
                }
            }
            throw new FormatException();
        }
        // <summary>
        // 去除文本读取器中数据内的空格、制表符、换行符
        // </summary>
        // <param name="reader">文本读取器</param>
        /// <summary>
        /// Remove spaces, tabs, newlines from the data in the text reader
        /// </summary>
        /// <param name="reader">TextReader</param>
        protected static void SkipSpace(TextReader reader)
        {
            while (reader.Peek() == ' ' || reader.Peek() == '\t' || reader.Peek() == '\r' || reader.Peek() == '\n')
            {
                reader.Read();
            }
        }
        // <summary>
        // 将JObject对象转化成json字符串数据
        // </summary>
        // <returns>输出JObject对象转化成的json字符串</returns>
        /// <summary>
        /// Convert a JObject object to a Json string
        /// </summary>
        /// <returns>Json string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            foreach (KeyValuePair<string, JObject> pair in properties)
            {
                sb.Append('"');
                sb.Append(pair.Key);
                sb.Append('"');
                sb.Append(':');
                if (pair.Value == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(pair.Value);
                }
                sb.Append(',');
            }
            if (properties.Count == 0)
            {
                sb.Append('}');
            }
            else
            {
                sb[sb.Length - 1] = '}';
            }
            return sb.ToString();
        }
        // <summary>
        // 将枚举类型数据转换成JString对象
        // </summary>
        // <param name="value">枚举类型数据</param>
        /// <summary>
        /// Convert a enumerated type data to a JString object
        /// </summary>
        /// <param name="value">enumerated type data</param>
        public static implicit operator JObject(Enum value)
        {
            return new JString(value.ToString());
        }
        // <summary>
        // 将JObject对象数组数据转换成JArray对象
        // </summary>
        // <param name="value">JObject对象数组数据</param>
        /// <summary>
        /// Convert a JObject array to a JArray object
        /// </summary>
        /// <param name="value">JObject array</param>
        public static implicit operator JObject(JObject[] value)
        {
            return new JArray(value);
        }
        // <summary>
        // 将布尔类型数据转换成JBoolean对象
        // </summary>
        // <param name="value">布尔类型数据</param>
        /// <summary>
        /// Convert a bool type data to a JBoolean object
        /// </summary>
        /// <param name="value">bool type data</param>
        public static implicit operator JObject(bool value)
        {
            return new JBoolean(value);
        }
        // <summary>
        // 将double类型数据转换成JNumber对象
        // </summary>
        // <param name="value">double类型数据</param>
        /// <summary>
        /// Convert a double type data to a JNumber object
        /// </summary>
        /// <param name="value">double type data</param>
        public static implicit operator JObject(double value)
        {
            return new JNumber(value);
        }
        // <summary>
        // 将String类型数据转换成JString对象
        // </summary>
        // <param name="value">string类型数据</param>
        /// <summary>
        /// Convert a string type data to a JString object 
        /// </summary>
        /// <param name="value">string type data</param>
        public static implicit operator JObject(string value)
        {
            return value == null ? null : new JString(value);
        }
    }
}
