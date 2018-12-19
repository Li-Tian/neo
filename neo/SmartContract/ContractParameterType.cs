namespace Neo.SmartContract
{
    // <summary>
    // 合约参数类型，为byte类型
    // </summary>
    /// <summary>
    /// Contract parameter type
    /// </summary>
    public enum ContractParameterType : byte
    {
        // <summary>
        // Signature
        // </summary>
        Signature = 0x00,
        // <summary>
        // Boolean
        // </summary>
        Boolean = 0x01,
        // <summary>
        // Integer
        // </summary>
        Integer = 0x02,
        // <summary>
        // Hash160
        // </summary>
        Hash160 = 0x03,
        // <summary>
        // Hash256
        // </summary>
        Hash256 = 0x04,
        // <summary>
        // Byte Array
        // </summary>
        ByteArray = 0x05,
        // <summary>
        // Public Key
        // </summary>
        PublicKey = 0x06,
        // <summary>
        // String
        // </summary>
        String = 0x07,
        // <summary>
        // Array
        // </summary>
        Array = 0x10,
        // <summary>
        // Map
        // </summary>
        Map = 0x12,
        // <summary>
        // InteropInterface
        // </summary>
        InteropInterface = 0xf0,
        // <summary>
        // Void
        // </summary>
        Void = 0xff
    }
}
