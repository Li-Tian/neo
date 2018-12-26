using System;
using System.Numerics;

namespace Neo
{
    /// <summary>
    /// Immutate, arbitrary precision signed decimal numbers. A BigDecimal consists of an unscaled value which is an BigInteger, and a byte scale,
    /// The scale is the number of digits to the right of the decimal point, and the the maxmimum scaled position of decimal point is 255.
    /// </summary>
    public struct BigDecimal
    {
        private readonly BigInteger value;
        private readonly byte decimals;

        /// <summary>
        /// The Value property represents the valid value of the BigDecimal
        /// </summary>
        /// <value>The value of Value property is obtained from the value field of this BigDecimal object</value>
        public BigInteger Value => value;

        /// <summary>
        /// Decimals property represents the number of digits to the right of the decimal point of this BigDecimal object
        /// </summary>
        /// <value>The value of Decimals property is obtained from decimals field.</value>
        public byte Decimals => decimals;

        /// <summary>
        /// The Sign propertu represents the sign of the Value property. It is 1 if Value is positive, it is -1 if the Value is -1, it is 0 if the Value is 0.
        /// </summary>
        /// <value>The value of Sign is obtained from the Sign field of this Big decimal object</value>
        public int Sign => value.Sign;

        /// <summary>
        /// Pass a valud value and the decimal point position number to this BigDecimal object. Then the value of this BigDecimal is value × 10<sup>-decimals</sup> 
        /// </summary>
        /// <param name="value">The valid value part of this BigDecimal object</param>
        /// <param name="decimals">The decimal point position of this BigDecimal</param>
        public BigDecimal(BigInteger value, byte decimals)
        {
            this.value = value;
            this.decimals = decimals;
        }

        /// <summary>
        /// Pass an decimal point position, and return a new BigDecimal Object which use this decimal point position number.
        /// </summary>
        /// <param name="decimals">Decimal point position</param>
        /// <exception cref="ArgumentException">The lose of precistion due to the transform</exception>
        /// <returns>A new BigDecimal Object which is represented by the new decimal point position </returns>
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
        /// Parse the String passed as argument to a BigDecimal object which is represented by specified decimal point position.
        /// </summary>
        /// <param name="s">string value to be parsed. It supports float number format.(e.g. "1.23") And it supports Scientific notation (e.g. "1.23e5")</param>
        /// <param name="decimals">decimal point position</param>
        /// <exception cref="FormatException">If the string can not be transferd to the BigDecimal oibject which is using the specified decimal point postition.</exception>
        /// <returns>The BigDecimal obejct parsed from string</returns>
        public static BigDecimal Parse(string s, byte decimals)
        {
            if (!TryParse(s, decimals, out BigDecimal result))
                throw new FormatException();
            return result;
        }

        /// <summary>
        /// Transfer this BigDecimal object to a Fixed8 Object and return
        /// </summary>
        /// <returns>A Fixed8 object which is transferd from this BigDecimal object</returns>
        /// <exception cref="System.InvalidCastException">If this BigDecimal object can not be transferd to the Fixed8 Object</exception>
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
        /// Transfer this BigDecimal object to string and return
        /// </summary>
        /// <returns>
        /// THe string which represents BigDecimal
        /// </returns>
        public override string ToString()
        {
            BigInteger divisor = BigInteger.Pow(10, decimals);
            BigInteger result = BigInteger.DivRem(value, divisor, out BigInteger remainder);
            if (remainder == 0) return result.ToString();
            return $"{result}.{remainder.ToString("d" + decimals)}".TrimEnd('0');
        }

        /// <summary>
        /// Parse the String passed as argument to a BigDecimal object which is represented by specified decimal point position.
        /// </summary>
        /// <param name="s">string value to be parsed. It supports float number format.(e.g. "1.23") And it supports Scientific notation (e.g. "1.23e5")</param>
        /// <param name="decimals">Decimal point position</param>
        /// <param name="result">The BigDecimal object transferd from String</param>
        /// <returns>If the string can be parsed then it return <c>true</c>,  if the string can not be parsed the return<c>false</c></returns>
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
