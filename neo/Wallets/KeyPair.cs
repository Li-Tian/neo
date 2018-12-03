using Neo.Cryptography;
using Neo.SmartContract;
using System;
using System.Linq;
using System.Text;

namespace Neo.Wallets
{
    /// <summary>
    /// NEO使用KeyPair对象存储私钥和公钥，公钥使用一个ECPoint对象形式表示，私钥使用一个字节数组形式表示.
    /// </summary>
    public class KeyPair : IEquatable<KeyPair>
    {
        /// <summary>
        /// 私钥，使用一个字节数组形式表示
        /// </summary>
        public readonly byte[] PrivateKey;
        /// <summary>
        /// 公钥，使用一个ECPoint对象形式表示
        /// </summary>
        public readonly Cryptography.ECC.ECPoint PublicKey;

        /// <summary>
        /// 公钥的哈希，由压缩型公钥求解Hash160(一次Sha256和RIPEMD160运算)得到
        /// </summary>
        public UInt160 PublicKeyHash => PublicKey.EncodePoint(true).ToScriptHash();

        /// <summary>
        /// 构造方法，传入一个私钥,由ECC算法产生其对应的公钥
        /// </summary>
        /// <param name="privateKey">导入的私钥</param>
        public KeyPair(byte[] privateKey)
        {
            if (privateKey.Length != 32 && privateKey.Length != 96 && privateKey.Length != 104)
                throw new ArgumentException();
            this.PrivateKey = new byte[32];
            Buffer.BlockCopy(privateKey, privateKey.Length - 32, PrivateKey, 0, 32);
            if (privateKey.Length == 32)
            {
                this.PublicKey = Cryptography.ECC.ECCurve.Secp256r1.G * privateKey;
            }
            else
            {
                this.PublicKey = Cryptography.ECC.ECPoint.FromBytes(privateKey, Cryptography.ECC.ECCurve.Secp256r1);
            }
        }

        /// <summary>
        /// 判断2个KeyPair对象是否相等
        /// </summary>
        /// <param name="other">另一个KeyPair对象</param>
        /// <returns>
        /// 如果两个KeyPair相等返回<c>true</c>, 否则返回<c>false</c>
        /// </returns>
        public bool Equals(KeyPair other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return PublicKey.Equals(other.PublicKey);
        }


        /// <summary>
        /// 判断KeyPair对象与另一个对象是否相等
        /// </summary>
        /// <param name="obj">另一个被比较的Object</param>
        /// <returns>如果两个Object相等则返回<c>true</c>, 否则返回<c>false</c></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as KeyPair);
        }

        /// <summary>
        /// 导出wif格式私钥
        /// </summary>
        /// <returns>wif格式私钥字符串</returns>
        public string Export()
        {
            byte[] data = new byte[34];
            data[0] = 0x80;
            Buffer.BlockCopy(PrivateKey, 0, data, 1, 32);
            data[33] = 0x01;
            string wif = data.Base58CheckEncode();
            Array.Clear(data, 0, data.Length);
            return wif;
        }

        /// <summary>
        /// 导出NEP2格式私钥
        /// </summary>
        /// <param name="passphrase">用户设置的密码</param>
        /// <param name="N">产生密钥所需要的参数N， 默认为16384</param>
        /// <param name="r">产生密钥所需要的参数r， 默认为8</param>
        /// <param name="p">产生密钥所需要的参数p， 默认为8</param>
        /// <returns>NEP2格式私钥字符串</returns>
        public string Export(string passphrase, int N = 16384, int r = 8, int p = 8)
        {
            UInt160 script_hash = Contract.CreateSignatureRedeemScript(PublicKey).ToScriptHash();
            string address = script_hash.ToAddress();
            byte[] addresshash = Encoding.ASCII.GetBytes(address).Sha256().Sha256().Take(4).ToArray();
            byte[] derivedkey = SCrypt.DeriveKey(Encoding.UTF8.GetBytes(passphrase), addresshash, N, r, p, 64);
            byte[] derivedhalf1 = derivedkey.Take(32).ToArray();
            byte[] derivedhalf2 = derivedkey.Skip(32).ToArray();
            byte[] encryptedkey = XOR(PrivateKey, derivedhalf1).AES256Encrypt(derivedhalf2);
            byte[] buffer = new byte[39];
            buffer[0] = 0x01;
            buffer[1] = 0x42;
            buffer[2] = 0xe0;
            Buffer.BlockCopy(addresshash, 0, buffer, 3, addresshash.Length);
            Buffer.BlockCopy(encryptedkey, 0, buffer, 7, encryptedkey.Length);
            return buffer.Base58CheckEncode();
        }

        /// <summary>
        /// 求公钥的HashCode
        /// </summary>
        /// <returns>公钥的HashCode</returns>
        public override int GetHashCode()
        {
            return PublicKey.GetHashCode();
        }

        /// <summary>
        /// 将公钥转换成字符串返回
        /// </summary>
        /// <returns>返回公钥转换成的字符串</returns>
        public override string ToString()
        {
            return PublicKey.ToString();
        }


        private static byte[] XOR(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) throw new ArgumentException();
            return x.Zip(y, (a, b) => (byte)(a ^ b)).ToArray();
        }
    }
}
