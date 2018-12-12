using Neo.IO;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Neo.Cryptography
{
    /// <summary>
    /// Cryptography中的帮助类, 主要是一些交易和验证中用到的一些hash算法和编码解码算法
    /// </summary>
    public static class Helper
    {
        private static ThreadLocal<SHA256> _sha256 = new ThreadLocal<SHA256>(() => SHA256.Create());
        private static ThreadLocal<RIPEMD160Managed> _ripemd160 = new ThreadLocal<RIPEMD160Managed>(() => new RIPEMD160Managed());

        internal static byte[] AES256Decrypt(this byte[] block, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(block, 0, block.Length);
                }
            }
        }

        internal static byte[] AES256Encrypt(this byte[] block, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(block, 0, block.Length);
                }
            }
        }

        internal static byte[] AesDecrypt(this byte[] data, byte[] key, byte[] iv)
        {
            if (data == null || key == null || iv == null) throw new ArgumentNullException();
            if (data.Length % 16 != 0 || key.Length != 32 || iv.Length != 16) throw new ArgumentException();
            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
                {
                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        internal static byte[] AesEncrypt(this byte[] data, byte[] key, byte[] iv)
        {
            if (data == null || key == null || iv == null) throw new ArgumentNullException();
            if (data.Length % 16 != 0 || key.Length != 32 || iv.Length != 16) throw new ArgumentException();
            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }
        /// <summary>
        /// 将一个Based58Check编码的字符串解码为字节数组
        /// 解码步骤：
        /// 1、把输入的字符串做 Base58 解码，得到 byte[] 数据。
        /// 2、取 byte[] 数据收字节到倒数第4字节前的所有数据 byte[] 称作 data。
        /// 3、把 data 做两次 sha256 得到的哈希值的前4字节作为版本前缀 checksum，
        /// 与 byte[] 数据的后4字节比较是否相同，相同则返回data, 否则判定为数据无效。
        /// </summary>
        /// <param name="input">被解码的字符串</param>
        /// <returns>解码后的字节数组</returns>
        /// <exception cref="System.FormatException">版本前缀 checksum与byte[] 数据的后4字节不同时抛出</exception>
        public static byte[] Base58CheckDecode(this string input)
        {
            byte[] buffer = Base58.Decode(input);
            if (buffer.Length < 4) throw new FormatException();
            byte[] checksum = buffer.Sha256(0, buffer.Length - 4).Sha256();
            if (!buffer.Skip(buffer.Length - 4).SequenceEqual(checksum.Take(4)))
                throw new FormatException();
            return buffer.Take(buffer.Length - 4).ToArray();
        }

        /// <summary>
        /// 将一个字节数组经过Based58Check编码后返回其字符串
        /// 编码步骤：
        /// 1、通过对原 byte[] 数据做两次 sha256 得到原数据的哈希，
        ///    取其前4字节作为版本前缀checksum，添加到原 byte[] 数据的末尾。
        /// 2、把添加了版本前缀的 byte[] 数据做 Base58 编码得到对应的字符串。
        /// </summary>
        /// <param name="data">需要用Based58Check编码的字节数组</param>
        /// <returns>解码后的Base58CheckEncode字符串</returns>
        public static string Base58CheckEncode(this byte[] data)
        {
            byte[] checksum = data.Sha256().Sha256();
            byte[] buffer = new byte[data.Length + 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
            Buffer.BlockCopy(checksum, 0, buffer, data.Length, 4);
            return Base58.Encode(buffer);
        }

        /// <summary>
        /// 使用RIPEMD-160算法的哈希函数来对一个字节集合进行哈希加密处理
        /// </summary>
        /// <param name="value">被哈希函数处理的字节集合</param>
        /// <returns>哈希函数处理过后的字节数组</returns>
        public static byte[] RIPEMD160(this IEnumerable<byte> value)
        {
            return _ripemd160.Value.ComputeHash(value.ToArray());
        }



        /// <summary>
        /// 使用Murmur3算法的哈希函数来对一个字节集合进行哈希加密处理,产生一个32-bit哈希值
        /// </summary>
        /// <param name="value">被哈希函数处理的字节集合</param>
        /// <param name="seed"> Murmur3中用到的一个随机的种子数， 用来防止HshDos攻击a[</param>
        /// <returns>Murmur3函数处理过后的字节数组</returns>   
        public static uint Murmur32(this IEnumerable<byte> value, uint seed)
        {
            using (Murmur3 murmur = new Murmur3(seed))
            {
                return murmur.ComputeHash(value.ToArray()).ToUInt32(0);
            }
        }

        /// <summary>
        /// 使用Sha256算法的哈希函数来对一个字节集合进行哈希加密处理
        /// </summary>
        /// <param name="value">被哈希函数处理的字节集合</param>
        /// <returns>Sha256函数处理过后的字节数组</returns>
        public static byte[] Sha256(this IEnumerable<byte> value)
        {
            return _sha256.Value.ComputeHash(value.ToArray());
        }
        /// <summary>
        /// 使用Sha256算法的哈希函数来对一个字节集合的一部分数据进行哈希加密处理
        /// </summary>
        /// <param name="value">被哈希函数处理的字节集合</param>
        /// <param name="offset">字节数组中被取出做哈希处理的开始字节位置</param>
        /// <param name="count">字节数组中取出来做哈希加密部分的字节大小</param>
        /// <returns>Sha256函数处理过后的字节数组</returns>
        public static byte[] Sha256(this byte[] value, int offset, int count)
        {
            return _sha256.Value.ComputeHash(value, offset, count);
        }

        internal static bool Test(this BloomFilter filter, Transaction tx)
        {
            if (filter.Check(tx.Hash.ToArray())) return true;
            if (tx.Outputs.Any(p => filter.Check(p.ScriptHash.ToArray()))) return true;
            if (tx.Inputs.Any(p => filter.Check(p.ToArray()))) return true;
            if (tx.Witnesses.Any(p => filter.Check(p.ScriptHash.ToArray())))
                return true;
#pragma warning disable CS0612
            if (tx is RegisterTransaction asset)
                if (filter.Check(asset.Admin.ToArray())) return true;
#pragma warning restore CS0612
            return false;
        }

        internal static byte[] ToAesKey(this string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] passwordHash = sha256.ComputeHash(passwordBytes);
                byte[] passwordHash2 = sha256.ComputeHash(passwordHash);
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                Array.Clear(passwordHash, 0, passwordHash.Length);
                return passwordHash2;
            }
        }

        internal static byte[] ToAesKey(this SecureString password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = password.ToArray();
                byte[] passwordHash = sha256.ComputeHash(passwordBytes);
                byte[] passwordHash2 = sha256.ComputeHash(passwordHash);
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                Array.Clear(passwordHash, 0, passwordHash.Length);
                return passwordHash2;
            }
        }

        internal static byte[] ToArray(this SecureString s)
        {
            if (s == null)
                throw new NullReferenceException();
            if (s.Length == 0)
                return new byte[0];
            List<byte> result = new List<byte>();
            IntPtr ptr = SecureStringMarshal.SecureStringToGlobalAllocAnsi(s);
            try
            {
                int i = 0;
                do
                {
                    byte b = Marshal.ReadByte(ptr, i++);
                    if (b == 0)
                        break;
                    result.Add(b);
                } while (true);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocAnsi(ptr);
            }
            return result.ToArray();
        }
    }
}
