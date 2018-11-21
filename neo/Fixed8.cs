using Neo.IO;
using System;
using System.Globalization;
using System.IO;

namespace Neo
{
    // <summary>
    // Accurate to 10^-8 64-bit fixed-point numbers minimize rounding errors.
    // By controlling the accuracy of the multiplier, rounding errors can be completely eliminated.
    // </summary>
    /// <summary>
    /// 用一个long类型的value加上固定的10^8的常数，来表达一个10^-8 64位的定点数
    /// </summary>
    public struct Fixed8 : IComparable<Fixed8>, IEquatable<Fixed8>, IFormattable, ISerializable
    {
        private const long D = 100_000_000;
        internal long value;
        /// <summary>
        /// MaxValue字段代表了一个有效值为long最大值的Fixed8对象
        /// </summary>
        public static readonly Fixed8 MaxValue = new Fixed8 { value = long.MaxValue };
        /// <summary>
        /// MinValue字段代表了一个有效值为long最小值的Fixed8对象
        /// </summary>
        public static readonly Fixed8 MinValue = new Fixed8 { value = long.MinValue };
        /// <summary>
        ///  One 字段代表了有效值为100_000_000的Fixed8对象, 其值代表了1
        /// </summary>
        public static readonly Fixed8 One = new Fixed8 { value = D };
        /// <summary>
        /// Satoshi字段代表了有效值为1的Fixed8对象, 其值代表了交易中最小的交易单位0.00000001
        /// </summary>
        public static readonly Fixed8 Satoshi = new Fixed8 { value = 1 };
        /// <summary>
        ///  Zero 字段代表了有效值为0的Fixed8对象, 其值代表了0
        /// </summary>
        public static readonly Fixed8 Zero = default(Fixed8);
        /// <summary>
        /// Size属性代表了这个类型的大小，用字节来表示
        /// </summary>
        public int Size => sizeof(long);

        /// <summary>
        /// 将一个long类型的数值传入这个Fixed8对象作为有效数.
        /// </summary>
        /// <param name="data">这个Fixed8对象的有效数</param>
        public Fixed8(long data)
        {
            this.value = data;
        }

        /// <summary>
        /// 返回这个数的绝对值,如果这个数是正数或0，直接返回原值。如果是负数，返回它的负数.
        /// </summary>
        /// <returns>返回一个数值已经是原来绝对值的Fixed8对象</returns>
        public Fixed8 Abs()
        {
            if (value >= 0) return this;
            return new Fixed8
            {
                value = -value
            };
        }

        /// <summary>
        /// 返回等于这个数或者比这个数大的最小单位的Fixed8.
        /// <list type="bullet">
        /// <item>
        /// <description>如果余数等于0， 直接返回该数本身</description>
        /// </item>
        /// <item>
        /// <description>如果余数大于0，将数值减去余数并加上基数D，得到比这个数大的最小单位的Fixed8</description>
        /// </item>
        /// <item>
        /// <description>如果余数小于0，将数值减去余数D，得到比这个数大的最小单位的Fixed8</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns>返回等于或者大于这个数的最小单位的Fixed8值，并且用Fixed8对象返回</returns>
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

        /// <summary>
        /// 比较两个Fixed8对象的数值
        /// </summary>
        /// <param name="other">另一个被比较的Fixed8格式的数</param>
        /// <returns>返回0如果两个数相等. 如果这个Fixed8数小于参数other返回一个负数, 如果这个Fixed8数大于参数other返回一个正数</returns>
        public int CompareTo(Fixed8 other)
        {
            return value.CompareTo(other.value);
        }

        /// <summary>
        /// 将字节流中的值读入并存入这个Fixed8对象的有效值value中
        /// </summary>
        /// <param name="reader">用于读取字节流</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            value = reader.ReadInt64();
        }

        /// <summary>
        /// 判断两个Fixed8数是否相等
        /// </summary>
        /// <param name="other">用来和这个Fixed8比较的Fixed8对象</param>
        /// <returns>如果两者数值相等则返回<c>true</c>, 否则返回<c>false</c></returns>
        /// 
        public bool Equals(Fixed8 other)
        {
            return value.Equals(other.value);
        }

        /// <summary>
        /// 当前这个Fixed8和另外一个对象进行比较
        /// </summary>
        /// <param name="obj">用来和这个Fixed8比较的对象</param>
        /// <returns>如果另外一个对象obj也是Fixed8类型且两者数值相等则返回<c>true</c>, 否则返回<c>false</c></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Fixed8)) return false;
            return Equals((Fixed8)obj);
        }

        /// <summary>
        /// 从一个decimal数转化为一个Fixed8对象
        /// </summary>
        /// <param name="value">被转换的小数</param>
        /// <exception cref="OverflowException">如果这个数太大或者太小,不能转换成Fixed对象</exception>
        /// <returns>转换后的Fixed8对象</returns>
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

        /// <summary>
        /// 返回这个Fixed8中的有效数值
        /// </summary>
        /// <returns>Fixed8对象中的有效数值</returns>
        public long GetData() => value;

        /// <summary>
        /// 返回这个Fixed8数的hashcode
        /// </summary>
        /// <returns>这个Fixed8数的hashcode</returns>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <summary>
        /// 返回一组Fixed8数中的最大值
        /// </summary>
        /// <param name="first">第一个被比较的Fixed8对象</param>
        /// <param name="others">其他多个被比较的Fixed8对象</param>
        /// <returns>其中最大的Fixed8对象</returns>
        public static Fixed8 Max(Fixed8 first, params Fixed8[] others)
        {
            foreach (Fixed8 other in others)
            {
                if (first.CompareTo(other) < 0)
                    first = other;
            }
            return first;
        }

        /// <summary>
        /// 返回一组Fixed8数中的最小值
        /// </summary>
        /// <param name="first">第一个被比较的Fixed8对象</param>
        /// <param name="others">其他多个被比较的Fixed8对象</param>
        /// <returns>其中最小的Fixed8对象</returns>
        public static Fixed8 Min(Fixed8 first, params Fixed8[] others)
        {
            foreach (Fixed8 other in others)
            {
                if (first.CompareTo(other) > 0)
                    first = other;
            }
            return first;
        }

        /// <summary>
        /// 将一个表示decimal小数的字符串转换为Fixed8对象
        /// </summary>
        /// <param name="s">待转化的字符串</param>
        /// <returns>转换后的Fixed8对象</returns>
        public static Fixed8 Parse(string s)
        {
            return FromDecimal(decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 将这个Fixed8对象的有效值序列化写入文件流
        /// </summary>
        /// <param name="writer">用于写入字节流</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(value);
        }

        /// <summary>
        /// 将这个Fixed8以<c>CultureInfo.InvariantCulture</c>格式转换成字符串
        /// </summary>
        /// <returns>转换后的字符串</returns>
        public override string ToString()
        {
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 将这个Fixed8以特定的格式转换成字符串
        /// </summary>
        /// <param name="format">转换成字符串表达时候需要的格式</param>
        /// <returns>转换后的字符串</returns>
        public string ToString(string format)
        {
            return ((decimal)this).ToString(format);
        }

        /// <summary>
        /// 将这个Fixed8以特定的格式转换成字符串
        /// </summary>
        /// <param name="format">需要转换的格式</param>
        /// <param name="formatProvider">为了在转换格式时候处理不同语言格式产生的差异， 传入用一个实现了IformatProvider接口的对象</param>
        /// <returns>转换后的字符串</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ((decimal)this).ToString(format, formatProvider);
        }

        /// <summary>
        /// 将一个字符串解析为一个Fixed8对象
        /// </summary>
        /// <param name="s">待解析的字符串</param>
        /// <param name="result">解析后转换成的Fixed8对象</param>
        /// <returns>如果能够解析成Fixed8对象, 则返回<c>true</c>, 否则返回<c>false</c></returns>
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

        /// <summary>
        /// 将一个Fixed8对象转换为decimal格式
        /// </summary>
        /// <param name="value">被转换的Fixed8对象</param>
        /// <returns>转换为decimal格式后的值</returns>
        public static explicit operator decimal(Fixed8 value)
        {
            return value.value / (decimal)D;
        }

        /// <summary>
        ///  将一个Fixed8对象转换为long格式
        /// </summary>
        /// <param name="value">被转换的Fixed8对象</param>
        /// <returns>转换为long格式后的值</returns>
        public static explicit operator long(Fixed8 value)
        {
            return value.value / D;
        }


        /// <summary>
        /// <c>==</c>操作符,比较两个Fixed8对象是否相等
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>如果两个Fixed8数值相等则返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator ==(Fixed8 x, Fixed8 y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// <c>!=</c>操作符,比较两个Fixed8对象是否不相等
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>如果两个Fixed8数值不相等则返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator !=(Fixed8 x, Fixed8 y)
        {
            return !x.Equals(y);
        }

        /// <summary>
        /// <c>&gt;</c>操作符,比较第一个Fixed对象是否大于第二个Fixed8对象
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>如果第一个Fixed对象大于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator >(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) > 0;
        }

        /// <summary>
        /// <c>&lt;</c>操作符,比较第一个Fixed对象是否小于第二个Fixed8对象
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>如果第一个Fixed对象小于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator <(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) < 0;
        }

        /// <summary>
        /// <c>&gt;=</c>操作符,比较第一个Fixed对象是否大于第二个Fixed8对象
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>如果第一个Fixed对象大于或者等于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator >=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) >= 0;
        }

        /// <summary>
        /// <c>&lt;=</c>操作符,比较第一个Fixed对象是否大于第二个Fixed8对象
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>如果第一个Fixed对象小于或者等于第二个Fixed8对象返回<c>true</c>,否则返回<c>false</c></returns>
        public static bool operator <=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) <= 0;
        }

        /// <summary>
        /// <c>*</c>操作符, 将两个Fixed8对象相乘后返回一个Fixed8对象
        /// </summary>
        /// <param name="x">第一个Fixed8对象</param>
        /// <param name="y">第二个Fixed8对象</param>
        /// <returns>相乘后的得到一个Fixed8对象</returns>
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

        /// <summary>
        /// <c>*</c>操作符, 将一个Fixed8的有效数乘以一个数后.
        /// </summary>
        /// <param name="x">被乘的Fixed8对象</param>
        /// <param name="y">乘数</param>
        /// <returns>数值乘以一个倍数之后的Fixed8对象</returns>
        public static Fixed8 operator *(Fixed8 x, long y)
        {
            x.value = checked(x.value * y);
            return x;
        }

        /// <summary>
        /// <c>/</c>操作符, 将一个的Fixed8数值除以一个数
        /// </summary>
        /// <param name="x">被除的Fixed8对象</param>
        /// <param name="y">除数</param>
        /// <returns>数值除完一个数之后的Fixed8对象</returns>
        public static Fixed8 operator /(Fixed8 x, long y)
        {
            x.value /= y;
            return x;
        }

        /// <summary>
        /// <c>+</c>操作符, 计算两个Fixed8对象的数值之和
        /// </summary>
        /// <param name="x">作为被加数的Fixed8对象</param>
        /// <param name="y">作为加数的Fixed8对象</param>
        /// <returns>作为两个数之和的Fixed8对象</returns>
        public static Fixed8 operator +(Fixed8 x, Fixed8 y)
        {
            x.value = checked(x.value + y.value);
            return x;
        }

        /// <summary>
        /// <c>-</c>操作符, 计算两个Fixed8对象的数值之差
        /// </summary>
        /// <param name="x">作为被减数的Fixed8对象</param>
        /// <param name="y">作为减数的Fixed8对象</param>
        /// <returns>作为两个数之差的Fixed8对象</returns>
        public static Fixed8 operator -(Fixed8 x, Fixed8 y)
        {
            x.value = checked(x.value - y.value);
            return x;
        }

        /// <summary>
        /// <c>-</c>操作符,取一个Fixed8对象的相反数
        /// </summary>
        /// <param name="value">被取反的Fixed8对象</param>
        /// <returns>取反后的Fixed8对象</returns>
        public static Fixed8 operator -(Fixed8 value)
        {
            value.value = -value.value;
            return value;
        }
    }
}
