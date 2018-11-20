namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 资产类型
    /// </summary>
    public enum AssetType : byte
    {
        /// <summary>
        /// 带有信任类型资产
        /// </summary>
        CreditFlag = 0x40,

        /// <summary>
        /// 带有权益类型资产, 转账时还需要收款人进行签名
        /// </summary>
        DutyFlag = 0x80,

        /// <summary>
        /// NEO 资产
        /// </summary>
        GoverningToken = 0x00,

        /// <summary>
        /// GAS 资产
        /// </summary>
        UtilityToken = 0x01,
        Currency = 0x08,

        /// <summary>
        /// 股权类资产
        /// </summary>
        Share = DutyFlag | 0x10,
        Invoice = DutyFlag | 0x18,

        /// <summary>
        /// Token类资产
        /// </summary>
        Token = CreditFlag | 0x20,
    }
}
