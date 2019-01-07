namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 封装的交易金额变化类
    // </summary>
    /// <summary>
    /// A class descript transaction amount change
    /// </summary>
    public class TransactionResult
    {
        /// <summary>
        /// The asset id
        /// </summary>
        // <summary>
        // 资产Id
        // </summary>
        public UInt256 AssetId;

        // <summary>
        // 金额变化 = inputs.Asset - outputs.Asset
        // </summary>
        /// <summary>
        /// amount change = inputs.Asset - outputs.Asset
        /// </summary>
        public Fixed8 Amount;
    }
}
