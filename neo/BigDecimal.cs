using System;
using System.Numerics;

namespace Neo
{
    /// <summary>
    /// 不可改变的, 任意精度的, 有符号的小数. 一个BigDecimal对象由一个BigInteger所表示的有效数和一个由byte表示的，对于一个任意长度整数小数点向左移的偏移量，其中小数点后最多255位.
    /// </summary>
    public struct BigDecimal
    {
        private readonly BigInteger value;
        private readonly byte decimals;

        /// <summary>
        /// Value属性代表了这个BigDecimal的有效数
        /// </summary>
        /// <value>Value属性的值由value字段获取</value>
        public BigInteger Value => value;

        /// <summary>
        /// Decimals属性代表了这个BigDecimal的小数位
        /// </summary>
        /// <value>Decimals属性的值由decimals字段获取</value>
        public byte Decimals => decimals;

        /// <summary>
        ///代表有效数的符号.如果为正则值1, 如果为负则值为-1, 如果是0则值为0. 
        /// </summary>
        /// <value>Sign属性的值等于value字段的Sign属性</value>
        public int Sign => value.Sign;

        /// <summary>
        /// 将一个有效数 value 和小数位数 decimals 传入这个BigDecimal对象, 其数值为 value × 10<sup>-deciaml</sup>
        /// </summary>
        /// <param name="value">BigDecimal的有效数部分</param>
        /// <param name="decimals">BigDecimal的小数位数</param>
        public BigDecimal(BigInteger value, byte decimals)
        {
            this.value = value;
            this.decimals = decimals;
        }

        /// <summary>
        /// 传入一个小数位数，返回以这个小数位数表述的新的BigDecimal对象
        /// </summary>
        /// <param name="decimals">小数位数</param>
        /// <exception cref="ArgumentException">由于转换可能引起的精度丢失</exception>
        /// <returns>一个以新的小数位表示的BigDecimal对象</returns>
        public BigDecimal ChangeDecimals(byte decimals)
        {
            if (this.decimals == decimals) return this;
            BigInteger value;
            if (this.decimals < decimals)
            {
                value = this.value * BigInteger.Pow(10, decimals - this.decimals);
            }
            else
            {
                BigInteger divisor = BigInteger.Pow(10, this.decimals - decimals);
                value = BigInteger.DivRem(this.value, divisor, out BigInteger remainder);
                if (remainder > BigInteger.Zero)
                    throw new ArgumentOutOfRangeException();
            }
            return new BigDecimal(value, decimals);
        }

        /// <summary>
        /// 将传入的字符串解析为以指定小数位数为表示的BigDecimal对象.
        /// </summary>
        /// <param name="s">被解析的字符串</param>
        /// <param name="decimals">小数位数</param>
        /// <exception cref="FormatException">如果这个字符串无法转换为以指定小数位所表示的BigDecimal对象</exception>
        /// <returns>字符串解析后的BigDecimal对象</returns>
        public static BigDecimal Parse(string s, byte decimals)
        {
            if (!TryParse(s, decimals, out BigDecimal result))
                throw new FormatException();
            return result;
        }

        /// <summary>
        /// 将这个BigDecimal数转化为Fixed8格式, 并返回这个Fixed8对象.
        /// </summary>
        /// <returns>一个Fixed8对象,由当前这个数转化为Fixed8格式所产生</returns>
        /// <exception cref="System.InvalidCastException">如果这个BigDecimal无法转换为Fixed8格式</exception>
        public Fixed8 ToFixed8()
        {
            try
            {
                return new Fixed8((long)ChangeDecimals(8).value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 将BigDecimal转化成字符串并返回
        /// </summary>
        /// <returns>
        /// 字符串形式的BigDecimal
        /// </returns>
        public override string ToString()
        {
            BigInteger divisor = BigInteger.Pow(10, decimals);
            BigInteger result = BigInteger.DivRem(value, divisor, out BigInteger remainder);
            if (remainder == 0) return result.ToString();
            return $"{result}.{remainder.ToString("d" + decimals)}".TrimEnd('0');
        }

        /// <summary>
        /// 将传入的字符串解析为以指定小数位数为表示的BigDecimal对象
        /// </summary>
        /// <param name="s">被解析的字符串</param>
        /// <param name="decimals">小数位</param>
        /// <param name="result">转换后的BigDecimal对象</param>
        /// <returns>如果成功解析则返回<c>true</c>, 如果不能够解析则反回<c>false</c></returns>
        public static bool TryParse(string s, byte decimals, out BigDecimal result)
        {
            int e = 0;
            int index = s.IndexOfAny(new[] { 'e', 'E' });
            if (index >= 0)
            {
                if (!sbyte.TryParse(s.Substring(index + 1), out sbyte e_temp))
                {
                    result = default(BigDecimal);
                    return false;
                }
                e = e_temp;
                s = s.Substring(0, index);
            }
            index = s.IndexOf('.');
            if (index >= 0)
            {
                s = s.TrimEnd('0');
                e -= s.Length - index - 1;
                s = s.Remove(index, 1);
            }
            int ds = e + decimals;
            if (ds < 0)
            {
                result = default(BigDecimal);
                return false;
            }
            if (ds > 0)
                s += new string('0', ds);
            if (!BigInteger.TryParse(s, out BigInteger value))
            {
                result = default(BigDecimal);
                return false;
            }
            result = new BigDecimal(value, decimals);
            return true;
        }
    }
}
