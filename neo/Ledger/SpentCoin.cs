using Neo.Network.P2P.Payloads;

namespace Neo.Ledger
{
    /// <summary>
    /// 已花费的output状态
    /// </summary>
    public class SpentCoin
    {
        /// <summary>
        /// 已经花费的output
        /// </summary>
        public TransactionOutput Output;

        /// <summary>
        /// output所在区块高度
        /// </summary>
        public uint StartHeight;

        /// <summary>
        /// output被花费的block高度
        /// </summary>
        public uint EndHeight;

        /// <summary>
        /// 花费量
        /// </summary>
        public Fixed8 Value => Output.Value;
    }
}
