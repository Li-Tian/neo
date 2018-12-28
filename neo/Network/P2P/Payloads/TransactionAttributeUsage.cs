namespace Neo.Network.P2P.Payloads
{

    // <summary>
    // 交易属性用途。新增的交易属性参考 NEP-9
    // </summary>
    /// <summary>
    /// The usage of transaction attribute.
    /// </summary>
    public enum TransactionAttributeUsage : byte
    {
        // <summary>
        // 外部合同的散列值
        // </summary>
        /// <summary>
        /// The hash of the contract
        /// </summary>
        ContractHash = 0x00,

        // <summary>
        // 用于ECDH密钥交换的公钥，该公钥的第一个字节为0x02
        // </summary>
        /// <summary>
        /// The public key which is used for ECDH. The first byte of that public key is 0x02
        /// </summary>
        ECDH02 = 0x02,
        // <summary>
        // 用于ECDH密钥交换的公钥，该公钥的第一个字节为0x03
        // </summary>
        /// <summary>
        /// The public key which is used for ECDH. The first byte of that public key is 0x03
        /// </summary>
        ECDH03 = 0x03,

        // <summary>
        // 用于对交易进行额外的验证, 如股权类转账，存放收款人的脚本hash
        // </summary>
        /// <summary>
        /// The additional verification for the transaction, such as share transfer, and 
        /// </summary>
        Script = 0x20,

        // <summary>
        // 投票
        // </summary>
        /// <summary>
        /// Votes
        /// </summary>
        Vote = 0x30,

        // <summary>
        // 外部介绍信息地址
        // </summary>
        /// <summary>
        /// The description url of external description
        /// </summary>
        DescriptionUrl = 0x81,

        // <summary>
        // 简短的介绍信息
        // </summary>
        /// <summary>
        /// The simple introduction
        /// </summary>
        Description = 0x90,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash1 = 0xa1,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash2 = 0xa2,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash3 = 0xa3,


        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash4 = 0xa4,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash5 = 0xa5,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash6 = 0xa6,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash7 = 0xa7,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash8 = 0xa8,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash9 = 0xa9,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash10 = 0xaa,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash11 = 0xab,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash12 = 0xac,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash13 = 0xad,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash14 = 0xae,

        // <summary>
        // 用于存放自定义的散列值
        // </summary>
        /// <summary>
        /// Save the self defined hash value 
        /// </summary>
        Hash15 = 0xaf,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark = 0xf0,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark1 = 0xf1,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark2 = 0xf2,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark3 = 0xf3,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark4 = 0xf4,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark5 = 0xf5,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark6 = 0xf6,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark7 = 0xf7,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark8 = 0xf8,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark9 = 0xf9,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark10 = 0xfa,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark11 = 0xfb,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark12 = 0xfc,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark13 = 0xfd,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark14 = 0xfe,

        /// <summary>
        /// 用于存放自定义的备注
        /// </summary>
        /// <summary>
        /// Save the self defiend notes
        /// </summary>
        Remark15 = 0xff
    }
}
