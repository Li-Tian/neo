using Neo.IO;
using System;
using System.IO;
using System.Linq;

namespace Neo
{


    // Base class for little-endian unsigned integers. Two classes inherit from this: UInt160 and UInt256.
    // Only basic comparison/serialization are proposed for these classes. For arithmetic purposes, use BigInteger class.
    ///<summary>
    /// 一个little-endian无符号整数的抽象类. 有两个类继承改抽象类：<c>UInt160</c>和<c>UInt256</c>.
    /// 
    /// 这两个类能够做简单的比较和序列化用. 如果需要做数学运算, 请使用BigInteger.
    /// </summary>
    public abstract class UIntBase : IEquatable<UIntBase>, ISerializable
    {
        /// <summary>
        /// Storing unsigned int in a little-endian byte array.
        /// </summary>
        private byte[] data_bytes;

        // <summary>
        // Number of bytes of the unsigned int.
        // Currently, inherited classes use 20-bytes (UInt160) or 32-bytes (UInt256)
        // </summary>
        /// <summary>
        /// 这个无符号整数的字节个数
        /// 有两个继承的Class,一个为20字节(<c>UInt160</c>), 一个为32字节(<c>UInt256</c>).
        /// </summary>
        /// <value>用来存储这个无符号整数的字节数组的长度</value>
        public int Size => data_bytes.Length;

        // <summary>
        // Base constructor receives the intended number of bytes and a byte array. 
        // If byte array is null, it's automatically initialized with given size.
        // </summary>
        /// <summary>
        /// 最基本的构造器， 用来接收字节长度和一个字节数组
        /// 如果没有传入一个字节数组， 则该UnitBase初始化为一个长度为bytes的字节数组
        /// </summary>
        /// <param name="bytes">字节长度</param>
        /// <param name="value">传入的字节数组</param>
        protected UIntBase(int bytes, byte[] value)
        {
            if (value == null)
            {
                this.data_bytes = new byte[bytes];
                return;
            }
            if (value.Length != bytes)
                throw new ArgumentException();
            this.data_bytes = value;
        }

        // <summary>
        // Deserialize function reads the expected size in bytes from the given BinaryReader and stores in data_bytes array.
        // </summary>
        /// <summary>
        /// 反序列化的函数，从一个BinaryReader中读取一个固定字节长度的字节数组，放入当前对象的data_bytes中
        /// </summary>
        /// <param name="reader">一个BinaryReader， 用于读取被反序列化的字节流</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            reader.Read(data_bytes, 0, data_bytes.Length);
        }

        // <summary>
        // Method Equals returns true if objects are equal, false otherwise
        // If null is passed as parameter, this method returns false. If it's a self-reference, it returns true.
        // </summary>
        /// <summary>
        /// 将当前UInt8对象与另一个UInt8对象进行比较
        /// </summary>
        /// <param name="other">另一个被比较的UintBase对象</param>
        /// <returns> 如果传入被比较的对象是一个null, 返回<c>flase</c>.
        /// 如果传入被比较的对象是当前这个对象的引用， 返回<c>ture</c>.
        /// 如果两个对象数值相等， 则返回<c>ture</c>， 否则返回<c>flase</c>.
        /// </returns>
        public bool Equals(UIntBase other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (data_bytes.Length != other.data_bytes.Length)
                return false;
            return data_bytes.SequenceEqual(other.data_bytes);
        }

        // <summary>
        // Method Equals returns true if objects are equal, false otherwise
        // If null is passed as parameter or if it's not a UIntBase object, this method returns false.
        // </summary>
        /// <summary>
        /// 将当前8对象与另一个对象进行比较
        /// </summary>
        /// <param name="obj">另一个被比较的Object</param>
        /// <returns>
        /// 如果传入的被比较的对象是一个null, 返回<c>flase</c>.
        /// 如果传入的被比较的对象不是一个UintBase对象, 返回<c>flase</c>.
        /// 如果都是UnitBase对象，则比较两个数值，如果两个对象数值相等， 则返回<c>ture</c>， 否则返回<c>flase</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (!(obj is UIntBase))
                return false;
            return this.Equals((UIntBase)obj);
        }

        // <summary>
        // Method GetHashCode returns a 32-bit int representing a hash code, composed of the first 4 bytes.
        // </summary>
        /// <summary>
        /// 用这个Uint对象的前4个字节作为HashCode并且返回.
        /// </summary>
        /// <returns>一个32位init所表述的hascose</returns>
        public override int GetHashCode()
        {
            return data_bytes.ToInt32(0);
        }

        // <summary>
        // Method Parse receives a big-endian hex string and stores as a UInt160 or UInt256 little-endian byte array
        // Example: Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01") should create UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        // </summary>
        /// <summary>
        /// 这个解析方法读取一个16进制表示的big-endian字符串然后根据字符串的长度转化为一个little-endian的UInt160或者UInt256的对象
        /// </summary>
        /// <example>"0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01" 会转化为 UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4</example>
        /// <param name="s">被解析的字符串</param>
        /// <returns>返回一个UInt160或者UInt256的对象</returns>
        public static UIntBase Parse(string s)
        {
            if (s.Length == 40 || s.Length == 42)
                return UInt160.Parse(s);
            else if (s.Length == 64 || s.Length == 66)
                return UInt256.Parse(s);
            else
                throw new FormatException();
        }

        // <summary>
        // Method Serialize writes the data_bytes array into a BinaryWriter object
        // </summary>
        /// <summary>
        /// 该序列化方法将内部的字节数组通过一个BinaryWriter写入到一个字节流中
        /// </summary>
        /// <param name="writer">一个BinaryWriter， 用于将序列化后的对象写入字节流</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(data_bytes);
        }

        // <summary>
        // Method ToArray() returns the byte array data_bytes, which stores the little-endian unsigned int
        // </summary>
        /// <summary>
        /// 返回对象的域data_bytes，这个字节数组本身是用来存储这个无符号整数
        /// </summary>
        /// <returns>代表这个无符号整型的字节数组</returns>
        public byte[] ToArray()
        {
            return data_bytes;
        }

        // <summary>
        // Method ToString returns a big-endian string starting by "0x" representing the little-endian unsigned int
        // Example: if this is storing 20-bytes 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4, ToString() should return "0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01"
        // </summary>
        /// <summary>
        /// 将UIntBase对象中little-endian的无符号整数转换为一个以"0x"开头的big-endian表示的字符串
        /// </summary>
        /// <example>如果对象为一个20-bytes的UInt16 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4, 字符串为 "0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01"</example>
        /// <returns></returns>
        public override string ToString()
        {
            return "0x" + data_bytes.Reverse().ToHexString();
        }

        // <summary>
        // Method TryParse tries to parse a big-endian hex string and stores it as a UInt160 or UInt256 little-endian bytes array
        // Example: TryParse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01", result) should create result UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        // </summary>
        /// <summary>
        /// 解析一个big-endian所表示的16进制字符串, 根据指定的泛型类型或者字符串的长度, 将这个字符串转化为UInt160或者Uint256所表示的little-endian的无符号整数
        /// </summary>
        /// <example>"0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01" 转换为 UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4</example>
        /// <typeparam name="T">UInt160或者UInt256类型</typeparam>
        /// <param name="s">被转化的字符串</param>
        /// <param name="result">解析转化后的UInt256或者UInt160对象</param>
        /// <returns>如果能够将该字符串解析为UInt160或者UInt256则返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool TryParse<T>(string s, out T result) where T : UIntBase
        {
            int size;
            if (typeof(T) == typeof(UInt160))
                size = 20;
            else if (typeof(T) == typeof(UInt256))
                size = 32;
            else if (s.Length == 40 || s.Length == 42)
                size = 20;
            else if (s.Length == 64 || s.Length == 66)
                size = 32;
            else
                size = 0;
            if (size == 20)
            {
                if (UInt160.TryParse(s, out UInt160 r))
                {
                    result = (T)(UIntBase)r;
                    return true;
                }
            }
            else if (size == 32)
            {
                if (UInt256.TryParse(s, out UInt256 r))
                {
                    result = (T)(UIntBase)r;
                    return true;
                }
            }
            result = null;
            return false;
        }

        // <summary>
        // Operator == returns true if left UIntBase is equals to right UIntBase
        // If any parameter is null, it returns false. If both are the same object, it returns true.
        // Example: UIntBase(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) == UIntBase(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        // </summary>
        /// <summary>
        /// <c>==</c>操作符,比较两个UintBase对象是否相等
        /// </summary>
        /// <param name="left">第一个UintBase对象</param>
        /// <param name="right">第二个UintBase对象</param>
        /// <returns>
        /// 如果两个UintBase对象引用一个对象，则返回<c>true</c>.
        /// 如果两个对象中其中一个指向<c>null</c>,则返回<c>false</c>.
        /// 如果两个UintBase8数值相等则返回<c>true</c>,否则返回<c>false</c>
        /// </returns>
        public static bool operator ==(UIntBase left, UIntBase right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        // <summary>
        // Operator != returns true if left UIntBase is not equals to right UIntBase
        // Example: UIntBase(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) != UIntBase(01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        // </summary>
        /// <summary>
        ///  <c>!=</c>操作符,比较两个UintBase对象是否不相等
        /// </summary>
        /// <param name="left">第一个UintBase对象</param>
        /// <param name="right">第二个UintBase对象</param>
        /// <returns>
        /// 如果两个UintBase对象引用一个对象，则返回<c>false</c>.
        /// 如果两个对象中其中一个指向<c>null</c>,则返回<c>true</c>.
        /// 如果两个UintBase8数值不相等则返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator !=(UIntBase left, UIntBase right)
        {
            return !(left == right);
        }
    }
}
