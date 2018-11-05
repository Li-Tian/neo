using System;
using System.Numerics;

namespace Neo
{
    /// <summary>
    /// 用科学计数法来表示的不可改变的, 任意精度的, 有符号的十进制数. 一个BigDecimal对象由一个BigInteger所表示的有效数和一个8位无符号整数(幂)构成.
    /// <example>
    /// 假设有效数为a, 幂为x
    /// 如果是0或者正数, 那么这个值为有效数a乘以10的x次幂
    /// 如果是负数，那么这个值为有效数a乘以10的负x次幂
    /// </example>
    /// </summary>
    public struct BigDecimal
    {
        private readonly BigInteger value;
        private readonly byte decimals;

        public BigInteger Value => value;
        public byte Decimals => decimals;
        public int Sign => value.Sign;

        /// <summary>
        /// 将一个有效数 value 和幂级数 decimals 传入这个BigDecimal对象, 其数值为 value × 10<sup>deciaml</sup>
        /// </summary>
        /// <param name="value">BigDecimal的有效数部分</param>
        /// <param name="decimals">BigDecimal的幂级数</param>
        public BigDecimal(BigInteger value, byte decimals)
        {
            this.value = value;
            this.decimals = decimals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decimals">幂</param>
        /// <returns></returns>
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
        /// 将传入的字符串解析为以指定幂为表示的BigDecimal对象.
        /// </summary>
        /// <param name="s">被解析的字符串</param>
        /// <param name="decimals">幂</param>
        /// <exception cref="FormatException">如果这个字符串无法转换为BigDecimal对象</exception>
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
        /// 用科学记数法来表达这个BigDecimal，以字符串形式来返回其中记数法表达式前面的有效数部分
        /// </summary>
        /// <returns>
        /// 返回字符串形式的BigDecimal有效数部分
        /// </returns>
        public override string ToString()
        {
            BigInteger divisor = BigInteger.Pow(10, decimals);
            BigInteger result = BigInteger.DivRem(value, divisor, out BigInteger remainder);
            if (remainder == 0) return result.ToString();
            return $"{result}.{remainder.ToString("d" + decimals)}".TrimEnd('0');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="decimals"></param>
        /// <param name="result"></param>
        /// <returns></returns>
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
