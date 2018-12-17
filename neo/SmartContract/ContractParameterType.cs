namespace Neo.SmartContract
{
    /// <summary>
    /// 合约参数类型，为byte类型
    /// </summary>
    public enum ContractParameterType : byte
    {
        /// <summary>
        /// 签名类型
        /// </summary>
        Signature = 0x00,
        /// <summary>
        /// 布尔类型
        /// </summary>
        Boolean = 0x01,
        /// <summary>
        /// 整型
        /// </summary>
        Integer = 0x02,
        /// <summary>
        /// Hash160类型
        /// </summary>
        Hash160 = 0x03,
        /// <summary>
        /// Hash256类型
        /// </summary>
        Hash256 = 0x04,
        /// <summary>
        /// 字节数组类型
        /// </summary>
        ByteArray = 0x05,
        /// <summary>
        /// 公钥类型
        /// </summary>
        PublicKey = 0x06,
        /// <summary>
        /// 字符串类型
        /// </summary>
        String = 0x07,
        /// <summary>
        /// 数组类型
        /// </summary>
        Array = 0x10,
        /// <summary>
        /// Map类型
        /// </summary>
        Map = 0x12,
        /// <summary>
        /// 互操作接口类型
        /// </summary>
        InteropInterface = 0xf0,
        /// <summary>
        /// Void类型
        /// </summary>
        Void = 0xff
    }
}
