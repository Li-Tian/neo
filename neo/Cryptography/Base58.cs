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
        /// <summary>
        /// Base58字母表
        /// </summary>
        public const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        /// <summary>
        /// 将一个Base58的字符串解析为一个字节数组
        /// 具体解码步骤：(*)换行
        /// 1、倒序输入的字符串，将其按字母表转换成10进制 Biginteger 数
        /// 2、把 Biginteger 数转换成 byte[] 数据，并将 byte[] 数据倒序排序
        /// 3、统计原输入的字符串中字母表游标为零所对应的字符的个数 count
        /// 4、若 byte[] 数据的长度大于1，且 byte[0] 等于0，byte[1] 大于等于0x80，
        /// 则从 byte[1] 开始截取，否则从 byte[0] 开始截取，得到结果。
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
        /// 将一个表示一个长数字的字节数组编码为基于Base58的字符串.
        /// 具体编码步骤：（*）换行
        /// 1、在 byte[] 数据前添加一个0x00，生成一个新的byte数组，并将新数组做倒序排序
        /// 2、把数组的数据转成10进制BigInteger数
        /// 3、把BigInteger数按字母表转换成58进制字符串
        /// 4、统计原 byte[] 数据中0x00的个数 count，在字符串前补 count 个字母表游标为零所对应的字符
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
