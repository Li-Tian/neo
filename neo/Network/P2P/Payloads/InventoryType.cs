namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // Inventory类型
    // </summary>
    /// <summary>
    /// The type of Inventory
    /// </summary>
    public enum InventoryType : byte
    {
        // <summary>
        // 交易
        // </summary>
        /// <summary>
        /// Transaction
        /// </summary>
        TX = 0x01,
        // <summary>
        // 区块
        // </summary>
        /// <summary>
        /// The blocks
        /// </summary>
        Block = 0x02,
        /// <summary>
        /// Consensus data
        /// </summary>
        Consensus = 0xe0
    }
}
