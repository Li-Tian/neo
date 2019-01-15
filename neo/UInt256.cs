using System;
using System.Globalization;
using System.Linq;

namespace Neo
{

    // <summary>
    // UInt256继承了UIntBase类,使用一个32字节的little-endian字节的数组来存储一个256位的无符号整数
    // </summary>
    /// <summary>
    /// This class stores a 256 bit unsigned int, represented as a 32-byte little-endian byte array
    /// </summary>
    public class UInt256 : UIntBase, IComparable<UInt256>, IEquatable<UInt256>
    {
        // <summary>
        // 作为一个值为0的UInt256 对象
        // </summary>
        /// <summary>
        /// a UInt256 object with a value of 0
        /// </summary>
        public static readonly UInt256 Zero = new UInt256();



        // <summary>
        // 构造一个对象。表示0.
        // </summary>
        /// <summary>
        /// The empty constructor stores a null byte array. Represents 0.
        /// </summary>
        public UInt256()
            : this(null)
        {
        }


        // <summary>
        // 调用UIntBase的构造器来构建一个32字节的无符号整数.
        // </summary>
        // <param name="value">传入的value值</param>
        /// <summary>
        /// The byte[] constructor invokes base class UIntBase constructor for 32 bytes
        /// </summary>
        /// <param name="value">value</param>
        public UInt256(byte[] value)
            : base(32, value)
        {
        }

        // <summary>
        // 与另一个UInt256对象做比较
        // </summary>
        // <param name="other">另一个被比较的UInt256对象</param>
        // <returns>如果这个UInt256比另一个对象大则返回1, 如果比另外一个对象小则返回-1, 如果相等则返回0</returns>
        /// <summary>
        /// Method CompareTo returns 1 if this UInt256 is bigger than other UInt256; -1 if it's smaller; 0 if it's equals
        /// Example: assume this is 01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4, this.CompareTo(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) returns 1
        /// </summary>
        /// <param name="other">another UInt256 object</param>
        /// <returns>returns 1 if this UInt256 is bigger than other UInt256; -1 if it's smaller; 0 if it's equals</returns>
        public int CompareTo(UInt256 other)
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
        // 调用UIntBase的Equals方法来比较
        // </summary>
        // <param name="other">另一个被比较的UInt256对象</param>
        // <returns>如果两个对象相等返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Method Equals returns true if objects are equal, false otherwise
        /// </summary>
        /// <param name="other">another UInt256 object</param>
        /// <returns>returns true if objects are equal, false otherwise</returns>
        bool IEquatable<UInt256>.Equals(UInt256 other)
        {
            return Equals(other);
        }

        // <summary>
        // 这个解析方法读取一个16进制表示的big-endian字符串然后转化为一个32字节的little-endian的UInt256对象
        // </summary>
        // <exception cref="ArgumentNullException">如果被解析字符串为null</exception>
        // <exception cref="FormatException">如果被解析的字符串长度不符合格式</exception>
        // <param name="s">被解析的字符串</param>
        // <returns>转化后的UInt256对象</returns>
        /// <summary>
        /// Method Parse receives a big-endian hex string and stores as a UInt256 little-endian 32-bytes array
        /// Example: Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff01") should create UInt256 01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        /// </summary>
        /// <exception cref="ArgumentNullException">if hex string is null</exception>
        /// <exception cref="FormatException">if the length of hex string is not 64</exception>
        /// <param name="s">a big-endian hex string</param>
        /// <returns>a UInt256 object</returns>
        public static new UInt256 Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException();
            if (s.StartsWith("0x"))
                s = s.Substring(2);
            if (s.Length != 64)
                throw new FormatException();
            return new UInt256(s.HexToBytes().Reverse().ToArray());
        }


        // <summary>
        // 解析一个big-endian所表示的16进制字符串, 将这个字符串转化为一个UInt256类型,
        // 20个字节的little-endian的无符号整数
        // </summary>
        // <param name="s">被转化的字符串</param>
        // <param name="result">用来保存结果的一个UInt256的对象</param>
        // <returns>如果解析成功返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Method TryParse tries to parse a big-endian hex string and store it as a UInt256 little-endian 32-bytes array
        /// Example: TryParse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff01", result) should create result UInt256 01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        /// </summary>
        /// <param name="s">a big-endian hex string</param>
        /// <param name="result">a UInt256 object used to save the result</param>
        /// <returns>if parse successfully,return true.otherwise,return false</returns>
        public static bool TryParse(string s, out UInt256 result)
        {
            if (s == null)
            {
                result = null;
                return false;
            }
            if (s.StartsWith("0x"))
                s = s.Substring(2);
            if (s.Length != 64)
            {
                result = null;
                return false;
            }
            byte[] data = new byte[32];
            for (int i = 0; i < 32; i++)
                if (!byte.TryParse(s.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier, null, out data[i]))
                {
                    result = null;
                    return false;
                }
            result = new UInt256(data.Reverse().ToArray());
            return true;
        }


        // <summary>
        // <c>&gt;</c>操作符,比较第一个UInt256对象是否大于第二个UInt256对象
        // </summary>
        // <param name="left">第一个UInt256对象</param>
        // <param name="right">第二个UInt256对象</param>
        // <returns>如果第一个UInt256对象大于第二个UInt256对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// Operator &gt; returns true if left UInt256 is bigger than right UInt256
        /// Example: UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) &gt; UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        /// </summary>
        /// <param name="left">a UInt256 object</param>
        /// <param name="right">another UInt256 object</param>
        /// <returns>returns true if left UInt256 is bigger than right UInt256.otherwise,return false</returns>
        public static bool operator >(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) > 0;
        }

        // <summary>
        // <c>&gt;=</c>操作符,比较第一个UInt256对象是否大于等于第二个UInt256对象
        // </summary>
        // <param name="left">第一个UInt256对象</param>
        // <param name="right">第二个UInt256对象</param>
        // <returns>如果第一个UInt256对象大于等于第二个UInt256对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// Operator &gt;= returns true if left UInt256 is bigger or equals to right UInt256
        /// Example: UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) &gt;= UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        /// </summary>
        /// <param name="left">a UInt256 object</param>
        /// <param name="right">a UInt256 object</param>
        /// <returns>returns true if left UInt256 is bigger or equals to right UInt256.otherwise,return false</returns>
        public static bool operator >=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) >= 0;
        }

        // <summary>
        // <c>&lt;</c>操作符,比较第一个UInt256对象是否小于第二个UInt256对象
        // </summary>
        // <param name="left">第一个UInt256对象</param>
        // <param name="right">第二个UInt256对象</param>
        // <returns>如果第一个UInt256对象小于第二个UInt256对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// Operator &lt; returns true if left UInt256 is less than right UInt256
        /// Example: UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) &lt; UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        /// </summary>
        /// <param name="left">a UInt256 object</param>
        /// <param name="right">another UInt256 object</param>
        /// <returns>returns true if left UInt256 is less than right UInt256.otherwise,return false</returns>
        public static bool operator <(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) < 0;
        }

        // <summary>
        // <c>&lt;=</c>操作符,比较第一个UInt256对象是否小于等于第二个UInt256对象
        // </summary>
        // <param name="left">第一个UInt256对象</param>
        // <param name="right">第二个UInt256对象</param>
        // <returns>如果第一个UInt256对象小于等于第二个UInt256对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// Operator &lt;= returns true if left UInt256 is less or equals to right UInt256
        /// Example: UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) &lt;= UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        /// </summary>
        /// <param name="left">a UInt256 object</param>
        /// <param name="right">another UInt256 object</param>
        /// <returns>returns true if left UInt256 is less or equals to right UInt256.otherwise,return false</returns>
        public static bool operator <=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) <= 0;
        }
    }
}
