using Neo.IO;
using System;
using System.Globalization;
using System.IO;

namespace Neo
{
    // <summary>
    // 用一个long类型的value加上固定的10^8的常数，来表达一个10^-8 64位的定点数
    // </summary>
    /// <summary>
    /// Accurate to 10^-8 64-bit fixed-point numbers minimize rounding errors.
    /// By controlling the accuracy of the multiplier, rounding errors can be completely eliminated.
    /// </summary>
    public struct Fixed8 : IComparable<Fixed8>, IEquatable<Fixed8>, IFormattable, ISerializable
    {
        private const long D = 100_000_000;
        internal long value;
        // <summary>
        // MaxValue字段代表了一个有效值为long最大值的Fixed8对象
        // </summary>
        /// <summary>
        /// The MaxValue field represents a Fixed8 object with a valid value of the maximum value of long.
        /// </summary>
        public static readonly Fixed8 MaxValue = new Fixed8 { value = long.MaxValue };
        // <summary>
        // MinValue字段代表了一个有效值为long最小值的Fixed8对象
        // </summary>
        /// <summary>
        /// The MinValue field represents a Fixed8 object with a valid value of the minimum  value of long.
        /// </summary>
        public static readonly Fixed8 MinValue = new Fixed8 { value = long.MinValue };
        // <summary>
        //  One 字段代表了有效值为100_000_000的Fixed8对象, 其值代表了1
        // </summary>
        /// <summary>
        ///  The One field represents a Fixed8 object with a valid value of 100_000_000, and its value represents 1
        /// </summary>
        public static readonly Fixed8 One = new Fixed8 { value = D };
        // <summary>
        // Satoshi字段代表了有效值为1的Fixed8对象, 其值代表了交易中最小的交易单位0.00000001
        // </summary>
        /// <summary>
        /// The Satoshi field represents a Fixed8 object with a valid value of 1, and its value represents the smallest transaction unit in the transaction.
        /// </summary>
        public static readonly Fixed8 Satoshi = new Fixed8 { value = 1 };
        // <summary>
        //  Zero 字段代表了有效值为0的Fixed8对象, 其值代表了0
        // </summary>
        /// <summary>
        ///  The Zero field represents a Fixed8 object with a valid value of 0, and its value represents 0.
        /// </summary>
        public static readonly Fixed8 Zero = default(Fixed8);
        // <summary>
        // Size属性代表了这个类型的大小，用字节来表示
        // </summary>
        /// <summary>
        /// The Size field represents the size of this type, expressed in bytes.
        /// </summary>
        public int Size => sizeof(long);

        // <summary>
        // 将一个long类型的数值传入这个Fixed8对象作为有效数.
        // </summary>
        // <param name="data">这个Fixed8对象的有效数</param>
        /// <summary>
        /// Constructor.Pass a long type data into this Fixed8 object as a valid value.
        /// </summary>
        /// <param name="data">a long type data</param>
        public Fixed8(long data)
        {
            this.value = data;
        }

        // <summary>
        // 返回这个数的绝对值,如果这个数是正数或0，直接返回原值。如果是负数，返回它的负数.
        // </summary>
        // <returns>返回一个数值已经是原来绝对值的Fixed8对象</returns>
        /// <summary>
        /// Returns the absolute value of this object. 
        /// If the value is positive or 0, it returns value directly. 
        /// If it is negative, return its negative value.
        /// </summary>
        /// <returns>Returns the absolute value of this object</returns>
        public Fixed8 Abs()
        {
            if (value >= 0) return this;
            return new Fixed8
            {
                value = -value
            };
        }

        // <summary>
        // 返回不小于这个数的最小整数的Fixed8.
        // <list type="bullet">
        // <item>
        // <description>如果余数等于0， 直接返回该数本身</description>
        // </item>
        // <item>
        // <description>如果余数大于0，将数值减去余数并加上基数D，得到比这个数大的最小整数的Fixed8</description>
        // </item>
        // <item>
        // <description>如果余数小于0，将数值减去余数，得到比这个数大的最小整数的Fixed8</description>
        // </item>
        // </list>
        // </summary>
        // <returns>返回不小于这个数的最小整数的Fixed8值</returns>
        /// <summary>
        /// Returns Fixed8 with a minimum integer value not less than this object.
        /// <list type="bullet">
        /// <item>
        /// <description>If the remainder is equal to 0, return the object itself directly</description>
        /// </item>
        /// <item>
        /// <description>
        /// If the remainder is greater than 0,<br/> 
        /// subtract the remainder from the value and add the base D <br/> 
        /// to get the smallest integer Fixed8 object larger than this number.<br/> 
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If the remainder is less than 0, <br/> 
        /// subtract the remainder  from the value <br/> 
        /// to get the smallest integer larger than this number.<br/> 
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns>Returns Fixed8 with a minimum integer value not less than this object.</returns>
        public Fixed8 Ceiling()
        {
            long remainder = value % D;
            if (remainder == 0) return this;
            if (remainder > 0)
                return new Fixed8
                {
                    value = value - remainder + D
                };
            else
                return new Fixed8
                {
                    value = value - remainder
                };
        }

        // <summary>
        // 比较两个Fixed8对象的数值
        // </summary>
        // <param name="other">另一个被比较的Fixed8格式的数</param>
        // <returns>返回0如果两个数相等. 如果这个Fixed8数小于参数other则返回一个负数, 如果这个Fixed8数大于参数other则返回一个正数</returns>
        /// <summary>
        /// Compare the values ​​of two Fixed8 objects
        /// </summary>
        /// <param name="other">another Fixed8 object</param>
        /// <returns>
        /// Returns 0 if the values of two Fixed8 objects are equal.
        /// If the value of this Fixed8 is less than another, it returns a negative number.
        /// If the value of this Fixed8 is greater than another, it returns a positive number.
        /// </returns>
        public int CompareTo(Fixed8 other)
        {
            return value.CompareTo(other.value);
        }

        // <summary>
        // 将字节流中的值读入并存入这个Fixed8对象的有效值value中
        // </summary>
        // <param name="reader">用于读取字节流</param>
        /// <summary>
        /// Read the data in the binary reader and stores it in the Fixed8 object.
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            value = reader.ReadInt64();
        }

        // <summary>
        // 判断两个Fixed8数是否相等
        // </summary>
        // <param name="other">用来和这个Fixed8比较的Fixed8对象</param>
        // <returns>如果两者数值相等则返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Determine if two Fixed8 object are equal
        /// </summary>
        /// <param name="other">another Fixed8 object</param>
        /// <returns>Returns<c>true</c> if the values of two object ​​are equal, otherwise returns<c>false</c></returns>
        public bool Equals(Fixed8 other)
        {
            return value.Equals(other.value);
        }

        // <summary>
        // 当前这个Fixed8和另外一个对象进行比较
        // </summary>
        // <param name="obj">用来和这个Fixed8比较的对象</param>
        // <returns>如果另外一个对象obj也是Fixed8类型且两者数值相等则返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Compare this Fixed8 object to another object
        /// </summary>
        /// <param name="obj">another Fixed8 object</param>
        /// <returns>Returns <c>true</c> if another object obj is also of type Fixed8 and the values ​​are equal, otherwise returns <c>false</c></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Fixed8)) return false;
            return Equals((Fixed8)obj);
        }

        // <summary>
        // 从一个decimal数转化为一个Fixed8对象
        // </summary>
        // <param name="value">被转换的数</param>
        // <exception cref="OverflowException">如果这个数太大或者太小,不能转换成Fixed对象</exception>
        // <returns>转换后的Fixed8对象</returns>
        /// <summary>
        /// Convert decimal data to a Fixed8 object
        /// </summary>
        /// <param name="value">decimal data</param>
        /// <exception cref="OverflowException">If data is too large or too small to be converted to a Fixed8 object.</exception>
        /// <returns>a Fixed8 object</returns>
        public static Fixed8 FromDecimal(decimal value)
        {
            value *= D;
            if (value < long.MinValue || value > long.MaxValue)
                throw new OverflowException();
            return new Fixed8
            {
                value = (long)value
            };
        }

        // <summary>
        // 返回这个Fixed8中的有效数 value 的值
        // </summary>
        // <returns>Fixed8对象中的有效数值</returns>
        /// <summary>
        ///  Returns the valid value of the Fixed8 object
        /// </summary>
        /// <returns>valid value of the Fixed8 object</returns>
        public long GetData() => value;

        // <summary>
        // 返回这个Fixed8数的hashcode
        // </summary>
        // <returns>这个Fixed8数的hashcode</returns>
        /// <summary>
        /// Returns the hash code of the Fixed8 object
        /// </summary>
        /// <returns>hash code of the Fixed8 object</returns>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        // <summary>
        // 返回一组Fixed8数中的最大值
        // </summary>
        // <param name="first">第一个被比较的Fixed8对象</param>
        // <param name="others">其他多个被比较的Fixed8对象</param>
        // <returns>其中最大的Fixed8对象</returns>
        /// <summary>
        /// Returns the maximum of a Fixed8 object set
        /// </summary>
        /// <param name="first">the first compared Fixed8 object</param>
        /// <param name="others">other compared Fixed8 objects</param>
        /// <returns>the maximum of a Fixed8 object set</returns>
        public static Fixed8 Max(Fixed8 first, params Fixed8[] others)
        {
            foreach (Fixed8 other in others)
            {
                if (first.CompareTo(other) < 0)
                    first = other;
            }
            return first;
        }

        // <summary>
        // 返回一组Fixed8数中的最小值
        // </summary>
        // <param name="first">第一个被比较的Fixed8对象</param>
        // <param name="others">其他多个被比较的Fixed8对象</param>
        // <returns>其中最小的Fixed8对象</returns>
        /// <summary>
        /// Returns the minimum of a Fixed8 object set
        /// </summary>
        /// <param name="first">the first compared Fixed8 object</param>
        /// <param name="others">other compared Fixed8 objects</param>
        /// <returns>the minimum of a Fixed8 object set</returns>
        public static Fixed8 Min(Fixed8 first, params Fixed8[] others)
        {
            foreach (Fixed8 other in others)
            {
                if (first.CompareTo(other) > 0)
                    first = other;
            }
            return first;
        }

        // <summary>
        // 将一个表示decimal小数的字符串转换为Fixed8对象
        // </summary>
        // <param name="s">待转化的字符串</param>
        // <returns>转换后的Fixed8对象</returns>
        /// <summary>
        /// Convert a string representing decimal data to a Fixed8 object
        /// </summary>
        /// <param name="s">a string representing decimal data</param>
        /// <returns>Fixed8 object</returns>
        public static Fixed8 Parse(string s)
        {
            return FromDecimal(decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        // <summary>
        // 将这个Fixed8对象的有效值序列化写入流
        // </summary>
        // <param name="writer">用于写入字节流</param>
        /// <summary>
        /// Serialize method
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(value);
        }

        // <summary>
        // 将这个Fixed8以 C# <c>CultureInfo.InvariantCulture</c>格式转换成字符串
        // </summary>
        // <returns>转换后的字符串</returns>
        /// <summary>
        /// Convert the Fixed8 object to a C# <c>CultureInfo.InvariantCulture</c> format string
        /// </summary>
        /// <returns>a C# <c>CultureInfo.InvariantCulture</c> format string</returns>
        public override string ToString()
        {
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }

        // <summary>
        // 将这个Fixed8以特定的格式转换成字符串
        // </summary>
        // <param name="format">转换成字符串表达时候需要的格式</param>
        // <returns>转换后的字符串</returns>
        /// <summary>
        /// Convert the Fixed8 object to a specified format string
        /// </summary>
        /// <param name="format">specified format</param>
        /// <returns>a specified format string</returns>
        public string ToString(string format)
        {
            return ((decimal)this).ToString(format);
        }

        // <summary>
        // 将这个Fixed8以特定的格式转换成字符串
        // </summary>
        // <param name="format">需要转换的格式</param>
        // <param name="formatProvider">为了在转换格式时候处理不同语言格式产生的差异， 传入用一个实现了IformatProvider接口的对象</param>
        // <returns>转换后的字符串</returns>
        /// <summary>
        /// Convert the Fixed8 object to a specified format string
        /// </summary>
        /// <param name="format">specified format</param>
        /// <param name="formatProvider">To handle the differences caused by different language formats when converting the format, an object that implements the IformatProvider interface is passed in.</param>
        /// <returns>a specified format string</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ((decimal)this).ToString(format, formatProvider);
        }

        // <summary>
        // 将一个字符串解析为一个Fixed8对象
        // </summary>
        // <param name="s">待解析的字符串</param>
        // <param name="result">解析后转换成的Fixed8对象</param>
        // <returns>如果能够解析成Fixed8对象, 则返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Parse a string into a Fixed8 object
        /// </summary>
        /// <param name="s">string</param>
        /// <param name="result">Fixed8 object</param>
        /// <returns>Return <c>true</c> if it can be parsed into a Fixed8 object, otherwise return<c>false</c></returns>
        public static bool TryParse(string s, out Fixed8 result)
        {
            decimal d;
            if (!decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
            {
                result = default(Fixed8);
                return false;
            }
            d *= D;
            if (d < long.MinValue || d > long.MaxValue)
            {
                result = default(Fixed8);
                return false;
            }
            result = new Fixed8
            {
                value = (long)d
            };
            return true;
        }

        // <summary>
        // 将一个Fixed8对象转换为decimal格式
        // </summary>
        // <param name="value">被转换的Fixed8对象</param>
        // <returns>转换为decimal格式后的值</returns>
        /// <summary>
        /// Convert a Fixed8 object to a decimal type data
        /// </summary>
        /// <param name="value">Fixed8 object</param>
        /// <returns>decimal type data</returns>
        public static explicit operator decimal(Fixed8 value)
        {
            return value.value / (decimal)D;
        }

        // <summary>
        //  将一个Fixed8对象转换为long格式
        // </summary>
        // <param name="value">被转换的Fixed8对象</param>
        // <returns>转换为long格式后的值</returns>
        /// <summary>
        /// Convert a Fixed8 object to a long type data
        /// </summary>
        /// <param name="value">Fixed8 object</param>
        /// <returns>long type data</returns>
        public static explicit operator long(Fixed8 value)
        {
            return value.value / D;
        }


        // <summary>
        // <c>==</c>操作符,比较两个Fixed8对象是否相等
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>如果两个Fixed8数值相等则返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// <c>==</c> operator,to compare the equality of two Fixed8 objects
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>Return true if two Fixed8 objects are equal.Otherwise,return false</returns>
        public static bool operator ==(Fixed8 x, Fixed8 y)
        {
            return x.Equals(y);
        }

        // <summary>
        // <c>!=</c>操作符,比较两个Fixed8对象是否不相等
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>如果两个Fixed8数值不相等则返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// <c>!=</c> operator,determine if two Fixed8 objects are not equal
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>Return true if two Fixed8 objects are not equal.Otherwise,return false</returns>
        public static bool operator !=(Fixed8 x, Fixed8 y)
        {
            return !x.Equals(y);
        }

        // <summary>
        // <c>&gt;</c>操作符,比较第一个Fixed对象是否大于第二个Fixed8对象
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>如果第一个Fixed对象大于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// <c>&gt;</c> operator,determine if a Fixed8 object is larger than another
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>Return true if a Fixed8 object is larger than another.Otherwise,return false</returns>
        public static bool operator >(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) > 0;
        }

        // <summary>
        // <c>&lt;</c>操作符,比较第一个Fixed对象是否小于第二个Fixed8对象
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>如果第一个Fixed对象小于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// <c>&lt;</c> operator,determine if a Fixed8 object is smaller than another
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>Return true if a Fixed8 object is smaller than another.Otherwise,return false</returns>
        public static bool operator <(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) < 0;
        }

        // <summary>
        // <c>&gt;=</c>操作符,比较第一个Fixed对象是否大于等于第二个Fixed8对象
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>如果第一个Fixed对象大于或者等于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// <c>&gt;=</c> operator,determine if a Fixed8 object is greater or equal to another
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>Return true if a Fixed8 object is greater or equal to another.Otherwise,return false</returns>
        public static bool operator >=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) >= 0;
        }

        // <summary>
        // <c>&lt;=</c>操作符,比较第一个Fixed对象是否小于等于第二个Fixed8对象
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>如果第一个Fixed对象小于或者等于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        /// <summary>
        /// <c>&lt;=</c> operator,determine if a Fixed8 object is less than or equal to another
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>Return true if a Fixed8 object is less than or equal to another.Otherwise,return false</returns>
        public static bool operator <=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) <= 0;
        }

        // <summary>
        // <c>*</c>操作符, 将两个Fixed8对象相乘后返回一个Fixed8对象
        // </summary>
        // <param name="x">第一个Fixed8对象</param>
        // <param name="y">第二个Fixed8对象</param>
        // <returns>相乘后的得到一个Fixed8对象</returns>
        // <exception cref="System.OverflowException">相乘溢出时抛出</exception>
        /// <summary>
        /// <c>*</c> operator, multiplies a Fixed8 object by another Fixed8 object
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">another Fixed8 object</param>
        /// <returns>a Fixed8 object represent product</returns>
        /// <exception cref="System.OverflowException">Throw when multiplying overflow</exception>
        public static Fixed8 operator *(Fixed8 x, Fixed8 y)
        {
            const ulong QUO = (1ul << 63) / (D >> 1);
            const ulong REM = ((1ul << 63) % (D >> 1)) << 1;
            int sign = Math.Sign(x.value) * Math.Sign(y.value);
            ulong ux = (ulong)Math.Abs(x.value);
            ulong uy = (ulong)Math.Abs(y.value);
            ulong xh = ux >> 32;
            ulong xl = ux & 0x00000000fffffffful;
            ulong yh = uy >> 32;
            ulong yl = uy & 0x00000000fffffffful;
            ulong rh = xh * yh;
            ulong rm = xh * yl + xl * yh;
            ulong rl = xl * yl;
            ulong rmh = rm >> 32;
            ulong rml = rm << 32;
            rh += rmh;
            rl += rml;
            if (rl < rml)
                ++rh;
            if (rh >= D)
                throw new OverflowException();
            ulong rd = rh * REM + rl;
            if (rd < rl)
                ++rh;
            ulong r = rh * QUO + rd / D;
            x.value = (long)r * sign;
            return x;
        }

        // <summary>
        // <c>*</c>操作符, 将一个Fixed8的有效数乘以一个数后.
        // </summary>
        // <param name="x">被乘的Fixed8对象</param>
        // <param name="y">乘数</param>
        // <returns>数值乘以一个倍数之后的Fixed8对象</returns>
        /// <summary>
        /// <c>*</c> operator, multiplies a Fixed8 value by a number
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">number</param>
        /// <returns>product</returns>
        public static Fixed8 operator *(Fixed8 x, long y)
        {
            x.value = checked(x.value * y);
            return x;
        }

        // <summary>
        // <c>/</c>操作符, 将一个的Fixed8数值除以一个数
        // </summary>
        // <param name="x">被除的Fixed8对象</param>
        // <param name="y">除数</param>
        // <returns>数值除完一个数之后的Fixed8对象</returns>
        /// <summary>
        /// <c>/</c> operator, divides a Fixed8 value by a number
        /// </summary>
        /// <param name="x">a Fixed8 object</param>
        /// <param name="y">number</param>
        /// <returns>quotient</returns>
        public static Fixed8 operator /(Fixed8 x, long y)
        {
            x.value /= y;
            return x;
        }

        // <summary>
        // <c>+</c>操作符, 计算两个Fixed8对象的数值之和
        // </summary>
        // <param name="x">作为被加数的Fixed8对象</param>
        // <param name="y">作为加数的Fixed8对象</param>
        // <returns>作为两个数之和的Fixed8对象</returns>
        /// <summary>
        /// <c>+</c> operator, calculates the sum of the values ​​of two Fixed8 objects
        /// </summary>
        /// <param name="x">a Fixed8 object as an addend</param>
        /// <param name="y">a Fixed8 object as an addend</param>
        /// <returns>sum of the values ​​of two Fixed8 objects</returns>
        public static Fixed8 operator +(Fixed8 x, Fixed8 y)
        {
            x.value = checked(x.value + y.value);
            return x;
        }

        // <summary>
        // <c>-</c>操作符, 计算两个Fixed8对象的数值之差
        // </summary>
        // <param name="x">作为被减数的Fixed8对象</param>
        // <param name="y">作为减数的Fixed8对象</param>
        // <returns>作为两个数之差的Fixed8对象</returns>
        /// <summary>
        /// <c>-</c> operator, calculates the difference between the values ​​of two Fixed8 objects
        /// </summary>
        /// <param name="x">a Fixed8 object as a subtracted</param>
        /// <param name="y">a Fixed8 object as a subtraction</param>
        /// <returns>a Fixed8 object represent the difference between the values ​​of two Fixed8 objects</returns>
        public static Fixed8 operator -(Fixed8 x, Fixed8 y)
        {
            x.value = checked(x.value - y.value);
            return x;
        }

        // <summary>
        // <c>-</c>操作符,取一个Fixed8对象的相反数
        // </summary>
        // <param name="value">被取反的Fixed8对象</param>
        // <returns>取反后的Fixed8对象</returns>
        /// <summary>
        /// <c>-</c> operator, taking the opposite of a Fixed8 object
        /// </summary>
        /// <param name="value">a Fixed8 object</param>
        /// <returns>the opposite of a Fixed8 object</returns>
        public static Fixed8 operator -(Fixed8 value)
        {
            value.value = -value.value;
            return value;
        }
    }
}
