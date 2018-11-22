using Neo.VM;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Neo.Cryptography
{
    /// <summary>
    /// Neo扩展算法的实现类，提供一些额外的算法组合接口的实现
    /// </summary>
    public class Crypto : ICrypto
    {
        /// <summary>
        /// 默认初始值，可以不用使用时再额外创建Crypto对象
        /// </summary>
        public static readonly Crypto Default = new Crypto();
        /// <summary>
        /// 将数据做一次Sha256和RIPEMD160运算，这个过程被称为Hash160
        /// </summary>
        /// <param name="message">待处理的数据</param>
        /// <returns>运算结果</returns>
        public byte[] Hash160(byte[] message)
        {
            return message.Sha256().RIPEMD160();
        }
        /// <summary>
        /// 将数据做2次Sha256运算，这个过程被称为Hash256
        /// </summary>
        /// <param name="message">待处理的数据</param>
        /// <returns>运算结果</returns>
        public byte[] Hash256(byte[] message)
        {
            return message.Sha256().Sha256();
        }
        /// <summary>
        /// 使用ECDsa算法，利用私钥和公钥对数据进行签名
        /// 使用的椭圆曲线是nistP256，哈希算法是SHA256
        /// </summary>
        /// <param name="message">待签名数据</param>
        /// <param name="prikey">私钥</param>
        /// <param name="pubkey">公钥</param>
        /// <returns>数据的签名</returns>
        public byte[] Sign(byte[] message, byte[] prikey, byte[] pubkey)
        {
            using (var ecdsa = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = prikey,
                Q = new ECPoint
                {
                    X = pubkey.Take(32).ToArray(),
                    Y = pubkey.Skip(32).ToArray()
                }
            }))
            {
                return ecdsa.SignData(message, HashAlgorithmName.SHA256);
            }
        }
        /// <summary>
        /// 验证签名
        /// 使用的椭圆曲线是nistP256，哈希算法是SHA256
        /// </summary>
        /// <param name="message">数据消息</param>
        /// <param name="signature">待验证的消息的签名</param>
        /// <param name="pubkey">公钥</param>
        /// <returns>验证结果，验证通过返回true,否则返回false</returns>
        /// <exception cref="System.ArgumentException">公钥格式错误时抛出</exception>
        public bool VerifySignature(byte[] message, byte[] signature, byte[] pubkey)
        {
            if (pubkey.Length == 33 && (pubkey[0] == 0x02 || pubkey[0] == 0x03))
            {
                try
                {
                    pubkey = Cryptography.ECC.ECPoint.DecodePoint(pubkey, Cryptography.ECC.ECCurve.Secp256r1).EncodePoint(false).Skip(1).ToArray();
                }
                catch
                {
                    return false;
                }
            }
            else if (pubkey.Length == 65 && pubkey[0] == 0x04)
            {
                pubkey = pubkey.Skip(1).ToArray();
            }
            else if (pubkey.Length != 64)
            {
                throw new ArgumentException();
            }
            using (var ecdsa = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = pubkey.Take(32).ToArray(),
                    Y = pubkey.Skip(32).ToArray()
                }
            }))
            {
                return ecdsa.VerifyData(message, signature, HashAlgorithmName.SHA256);
            }
        }
    }
}
