namespace Neo.VM
{
    // <summary>
    // 加密算法接口
    // </summary>
    /// <summary>
    /// Crypto interface
    /// </summary>
    public interface ICrypto
    {
        // <summary>
        // 求数据使用Hash160算法进行运算的结果
        // </summary>
        // <param name="message">待处理的数据</param>
        // <returns>运算结果</returns>
        /// <summary>
        /// Get the result of the data using the Hash160 algorithm
        /// </summary>
        /// <param name="message">Data to be processed</param>
        /// <returns>Operation result</returns>
        byte[] Hash160(byte[] message);
        // <summary>
        // 求数据使用Hash256算法进行运算的结果
        // </summary>
        // <param name="message">待处理的数据</param>
        // <returns>运算结果</returns>
        /// <summary>
        /// Get the result of the data using the Hash256 algorithm
        /// </summary>
        /// <param name="message">Data to be processed</param>
        /// <returns>Operation result</returns>
        byte[] Hash256(byte[] message);
        // <summary>
        // 签名验证算法
        // </summary>
        // <param name="message">数据消息</param>
        // <param name="signature">待验证的消息的签名</param>
        // <param name="pubkey">公钥</param>
        // <returns>验证结果，验证通过返回true,否则返回false</returns>
        /// <summary>
        /// Signature verification algorithm
        /// </summary>
        /// <param name="message">Data message</param>
        /// <param name="signature">The signature of the message to be verified</param>
        /// <param name="pubkey">public key</param>
        /// <returns>Return true if verification passed, false otherwise</returns>
        bool VerifySignature(byte[] message, byte[] signature, byte[] pubkey);
    }
}
