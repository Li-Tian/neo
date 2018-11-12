using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Neo.Cryptography
{
    /// <summary>
    /// Base58类是能将数字转化成一种基于文本的二进制编码格式,更简洁方便地表示长串的数字.
    /// Base58不含Base64中的0（数字0）、O（大写字母o）、l（小写字母 L）、I（大写字母i），以及“+”和“/”两个字符。
    /// </summary>
    public static class Base58
    {
        public const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        /// <summary>
        /// 将一个Base56的字符串解析为一个字节数组
        /// </summary>
        /// <param name="input">被解码的Base58字符串</param>
        /// <exception cref="FormatException">不是一个标准的Base58字符串</exception>
        /// <returns>解码后的字节数组</returns>
        public static byte[] Decode(string input)
        {
            BigInteger bi = BigInteger.Zero;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                int index = Alphabet.IndexOf(input[i]);
                if (index == -1)
                    throw new FormatException();
                bi += index * BigInteger.Pow(58, input.Length - 1 - i);
            }
            byte[] bytes = bi.ToByteArray();
            Array.Reverse(bytes);
            bool stripSignByte = bytes.Length > 1 && bytes[0] == 0 && bytes[1] >= 0x80;
            int leadingZeros = 0;
            for (int i = 0; i < input.Length && input[i] == Alphabet[0]; i++)
            {
                leadingZeros++;
            }
            byte[] tmp = new byte[bytes.Length - (stripSignByte ? 1 : 0) + leadingZeros];
            Array.Copy(bytes, stripSignByte ? 1 : 0, tmp, leadingZeros, tmp.Length - leadingZeros);
            return tmp;
        }

        /// <summary>
        /// 将一个表示一个长数字的字节数组编码为基于Base58的字符串.其中在字节数组会加入一个前缀0x00.
        /// </summary>
        /// <param name="input">被编码的字节数组</param>
        /// <returns>编码后的Base58字符串</returns>
        public static string Encode(byte[] input)
        {
            BigInteger value = new BigInteger(new byte[1].Concat(input).Reverse().ToArray());
            StringBuilder sb = new StringBuilder();
            while (value >= 58)
            {
                BigInteger mod = value % 58;
                sb.Insert(0, Alphabet[(int)mod]);
                value /= 58;
            }
            sb.Insert(0, Alphabet[(int)value]);
            foreach (byte b in input)
            {
                if (b == 0)
                    sb.Insert(0, Alphabet[0]);
                else
                    break;
            }
            return sb.ToString();
        }
    }
}
