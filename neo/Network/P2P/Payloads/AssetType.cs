namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 资产类型
    // </summary>
    /// <summary>
    /// asset type
    /// </summary>
    public enum AssetType : byte
    {
        // <summary>
        // 带有信任类型资产
        // </summary>
        /// <summary>
        /// Asset with Credit type 
        /// </summary>
        CreditFlag = 0x40,

        // <summary>
        // 带有权益类型资产, 转账时还需要收款人进行签名
        // </summary>
        /// <summary>
        /// Duty type asset, the payee is also required to sign the transfer.
        /// </summary>
        DutyFlag = 0x80,

        // <summary>
        // NEO 资产
        // </summary>
        /// <summary>
        /// NEO asset
        /// </summary>
        GoverningToken = 0x00,

        // <summary>
        // GAS 资产
        // </summary>
        /// <summary>
        /// GAS asset
        /// </summary>
        UtilityToken = 0x01,
        // <summary>
        // 未使用（保留）
        // </summary>
        /// <summary>
        /// Not used (reserved)
        /// </summary>
        Currency = 0x08,

        // <summary>
        // 股权类资产
        // </summary>
        /// <summary>
        /// Equity type assets
        /// </summary>
        Share = DutyFlag | 0x10,

        // <summary>
        // 票据类资产（保留）
        // </summary>
        /// <summary>
        /// Invoice type assets(reserved)
        /// </summary>
        Invoice = DutyFlag | 0x18,

        // <summary>
        // Token类资产
        // </summary>
        /// <summary>
        /// Token type assets
        /// </summary>
        Token = CreditFlag | 0x20,
    }
}
