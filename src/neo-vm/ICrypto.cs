namespace Neo.VM
{
    /// <summary>
    /// 加密算法接口
    /// </summary>
    public interface ICrypto
    {
        /// <summary>
        /// Hash160算法
        /// </summary>
        /// <param name="message">待处理的数据</param>
        /// <returns>运算结果</returns>
        byte[] Hash160(byte[] message);
        /// <summary>
        /// Hash256算法
        /// </summary>
        /// <param name="message">待处理的数据</param>
        /// <returns>运算结果</returns>
        byte[] Hash256(byte[] message);
        /// <summary>
        /// 签名验证算法
        /// </summary>
        /// <param name="message">数据消息</param>
        /// <param name="signature">待验证的消息的签名</param>
        /// <param name="pubkey">公钥</param>
        /// <returns>验证结果，验证通过返回true,否则返回false</returns>
        bool VerifySignature(byte[] message, byte[] signature, byte[] pubkey);
    }
}
