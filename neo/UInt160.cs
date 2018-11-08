using System;
using System.Globalization;
using System.Linq;

namespace Neo
{
    // <summary>
    // This class stores a 160 bit unsigned int, represented as a 20-byte little-endian byte array
    // </summary>
    /// <summary>
    /// UInt160继承了UIntBase类,使用一个20字节的little-endian字节的数组来存储一个160位的无符号整数
    /// </summary>
    public class UInt160 : UIntBase, IComparable<UInt160>, IEquatable<UInt160>
    {
        /// <summary>
        /// 作为一个值为0的 UInt160对象
        /// </summary>
        public static readonly UInt160 Zero = new UInt160();

        // <summary>
        // The empty constructor stores a null byte array
        // </summary>
        /// <summary>
        /// 构造一个byte array为空的对象
        /// </summary>
        public UInt160()
            : this(null)
        {
        }

        // <summary>
        // The byte[] constructor invokes base class UIntBase constructor for 20 bytes
        // </summary>
        /// <summary>
        /// 调用UIntBase的构造器来构建一个20字节的无符号整数.
        /// </summary>
        /// <param name="value">传入的value值</param>
        public UInt160(byte[] value)
            : base(20, value)
        {
        }

        // <summary>
        // Method CompareTo returns 1 if this UInt160 is bigger than other UInt160; -1 if it's smaller; 0 if it's equals
        // Example: assume this is 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4, this.CompareTo(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) returns 1
        // </summary>
        /// <summary>
        /// 与另一个UInt160对象做比较
        /// </summary>
        /// <param name="other">另一个被比较的UInt160对象</param>
        /// <returns>如果这个UInt160比另一个对象大则返回1, 如果比另外一个对象小则返回-1, 如果相等则返回0</returns>
        public int CompareTo(UInt160 other)
        {
            byte[] x = ToArray();
            byte[] y = other.ToArray();
            for (int i = x.Length - 1; i >= 0; i--)
            {
                if (x[i] > y[i])
                    return 1;
                if (x[i] < y[i])
                    return -1;
            }
            return 0;
        }

        // <summary>
        // Method Equals returns true if objects are equal, false otherwise
        // </summary>
        /// <summary>
        /// 调用UIntBase的Equals方法来比较
        /// </summary>
        /// <param name="other">另一个被比较的UInt160对象</param>
        /// <returns>如果两个对象相等返回<c>true</c>, 否则返回<c>false</c></returns>
        bool IEquatable<UInt160>.Equals(UInt160 other)
        {
            return Equals(other);
        }

        // <summary>
        // Method Parse receives a big-endian hex string and stores as a UInt160 little-endian 20-bytes array
        // Example: Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01") should create UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        // </summary>
        /// <summary>
        /// 这个解析方法读取一个16进制表示的big-endian字符串然后转化为一个20字节的little-endian的UInt160对象
        /// </summary>
        /// <exception cref="ArgumentNullException">如果被解析字符串为null</exception>
        /// <exception cref="FormatException">如果被解析的字符串长度不符合格式</exception>
        /// <param name="value">被解析的字符串</param>
        /// <returns>转化后的UInt160对象</returns>
        public static new UInt160 Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException();
            if (value.StartsWith("0x"))
                value = value.Substring(2);
            if (value.Length != 40)
                throw new FormatException();
            return new UInt160(value.HexToBytes().Reverse().ToArray());
        }

        // <summary>
        // Method TryParse tries to parse a big-endian hex string and store it as a UInt160 little-endian 20-bytes array
        // Example: TryParse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01", result) should create result UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        // </summary>
        /// <summary>
        /// 解析一个big-endian所表示的16进制字符串, 将这个字符串转化为一个UInt160类型,20个字节的little-endian的无符号整数
        /// </summary>
        /// <param name="s">被转化的字符串</param>
        /// <param name="result">用来保存结果的一个UInt160的对象</param>
        /// <returns>如果解析成功返回true, 否则返回false</returns>
        public static bool TryParse(string s, out UInt160 result)
        {
            if (s == null)
            {
                result = null;
                return false;
            }
            if (s.StartsWith("0x"))
                s = s.Substring(2);
            if (s.Length != 40)
            {
                result = null;
                return false;
            }
            byte[] data = new byte[20];
            for (int i = 0; i < 20; i++)
                if (!byte.TryParse(s.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier, null, out data[i]))
                {
                    result = null;
                    return false;
                }
            result = new UInt160(data.Reverse().ToArray());
            return true;
        }

        // <summary>
        // Operator &gt; returns true if left UInt160 is bigger than right UInt160
        // Example: UInt160(01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) &gt; UInt160 (001f00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        // </summary>
        /// <summary>
        /// <c>&gt;</c>操作符,比较第一个Uint160对象是否大于第二个Uint160对象
        /// </summary>
        /// <param name="left">第一个Uint160对象</param>
        /// <param name="right">第二个Uint160对象</param>
        /// <returns>如果第一个Uint160对象大于第二个Uint160对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator >(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) > 0;
        }

        // <summary>
        // Operator &gt;= returns true if left UInt160 is bigger or equals to right UInt160
        // Example: UInt160(01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) &gt;= UInt160 (01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        // </summary>
        /// <summary>
        /// <c>&gt;=</c>操作符,比较第一个Uint160对象是否大于等于第二个Uint160对象
        /// </summary>
        /// <param name="left">第一个Uint160对象</param>
        /// <param name="right">第二个Uint160对象</param>
        /// <returns>如果第一个Uint160对象大于等于第二个Uint160对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator >=(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) >= 0;
        }

        // <summary>
        // Operator &lt; returns true if left UInt160 is less than right UInt160
        // Example: UInt160(001f00ff00ff00ff00ff00ff00ff00ff00ff00a3) &lt; UInt160 (01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        // </summary>
        /// <summary>
        /// <c>&lt;</c>操作符,比较第一个Uint160对象是否小于第二个Uint160对象
        /// </summary>
        /// <param name="left">第一个Uint160对象</param>
        /// <param name="right">第二个Uint160对象</param>
        /// <returns>如果第一个Uint160对象小于第二个Uint160对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator <(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) < 0;
        }

        // <summary>
        // Operator &lt;= returns true if left UInt160 is less or equals to right UInt160
        // Example: UInt160(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) &lt;= UInt160 (02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        // </summary>
        /// <summary>
        /// <c>&lt;=</c>操作符,比较第一个Uint160对象是否小于等于第二个Uint160对象
        /// </summary>
        /// <param name="left">第一个Uint160对象</param>
        /// <param name="right">第二个Uint160对象</param>
        /// <returns>如果第一个Uint160对象小于等于第二个Uint160对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator <=(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) <= 0;
        }
    }
}
