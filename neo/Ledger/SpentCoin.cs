using Neo.Network.P2P.Payloads;

namespace Neo.Ledger
{
    // <summary>
    // 已花费的output状态
    // </summary>
    /// <summary>
    /// The state of already spent output
    /// </summary>
    public class SpentCoin
    {
        // <summary>
        // 已经花费的output
        // </summary>
        /// <summary>
        /// The already spent transaction output
        /// </summary>
        public TransactionOutput Output;

        // <summary>
        // output所在区块高度
        // </summary>
        /// <summary>
        /// The block height of the output
        /// </summary>
        public uint StartHeight;

        // <summary>
        // output被花费的block高度
        // </summary>
        /// <summary>
        /// The height of block in which the transaction output is spent
        /// </summary>
        public uint EndHeight;

        // <summary>
        // 花费量
        // </summary>
        /// <summary>
        /// The value of this transactionOutput
        /// </summary>
        public Fixed8 Value => Output.Value;
    }
}
