namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// Inventory类型
    /// </summary>
    public enum InventoryType : byte
    {
        /// <summary>
        /// 交易
        /// </summary>
        TX = 0x01,
        /// <summary>
        /// 区块
        /// </summary>
        Block = 0x02,
        /// <summary>
        /// 共识
        /// </summary>
        Consensus = 0xe0
    }
}
