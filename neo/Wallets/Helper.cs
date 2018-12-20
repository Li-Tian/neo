using Neo.Cryptography;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using System;
using System.Linq;

namespace Neo.Wallets
{
    // <summary>
    // 钱包中的帮助类, 提供了两个转换地址的函数
    // </summary>
    /// <summary>
    /// A Wallet Helper Class.It provides two address translation functions
    /// </summary>
    public static class Helper
    {
        // <summary>
        // 用ecdsa对一个IVerifiable的消息做签名
        // </summary>
        // <param name="verifiable">待处理的消息</param>
        // <param name="key">公钥私钥对</param>
        // <returns>签完名之后的消息</returns>
        /// <summary>
        /// Sign an IVerifiable message with ecdsa
        /// </summary>
        /// <param name="verifiable">Pending Message</param>
        /// <param name="key">A key pair contains public key and private key</param>
        /// <returns>Signed Message</returns>
        public static byte[] Sign(this IVerifiable verifiable, KeyPair key)
        {
            return Crypto.Default.Sign(verifiable.GetHashData(), key.PrivateKey, key.PublicKey.EncodePoint(false).Skip(1).ToArray());
        }

        // <summary>
        // 将一个ScriptHash转换成地址
        // </summary>
        // <param name="scriptHash">被转换的ScriptHash</param>
        // <returns>返回转换后的地址</returns>
        /// <summary>
        /// Convert ScriptHash to address
        /// </summary>
        /// <param name="scriptHash">ScriptHash</param>
        /// <returns>address</returns>
        public static string ToAddress(this UInt160 scriptHash)
        {
            byte[] data = new byte[21];
            data[0] = Settings.Default.AddressVersion;
            Buffer.BlockCopy(scriptHash.ToArray(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }

        // <summary>
        // 将一个地址转换为ScriptHash
        // </summary>
        // <param name="address">待转换的地址</param>
        // <exception cref="FormatException">如果地址转换为字节数组长度不是21或者第一个字节不是指定的字节格式</exception>
        // <returns>转换后的一个UInt160表示的ScriptHash</returns>
        /// <summary>
        /// Convert address to ScriptHash
        /// </summary>
        /// <param name="address">address</param>
        /// <exception cref="FormatException">
        /// If the length of a byte array converted by address is not 21,
        /// or the value of the first byte of this array is illegal format
        /// </exception>
        /// <returns>UInt160 type ScriptHash</returns>
        public static UInt160 ToScriptHash(this string address)
        {
            byte[] data = address.Base58CheckDecode();
            if (data.Length != 21)
                throw new FormatException();
            if (data[0] != Settings.Default.AddressVersion)
                throw new FormatException();
            return new UInt160(data.Skip(1).ToArray());
        }
    }
}
