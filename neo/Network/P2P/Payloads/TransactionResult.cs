﻿namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 封装的交易金额变化类
    /// </summary>
    public class TransactionResult
    {
        /// <summary>
        /// 资产Id
        /// </summary>
        public UInt256 AssetId;

        /// <summary>
        /// 金额变化 = inputs.Asset - outputs.Asset
        /// </summary>
        public Fixed8 Amount;
    }
}
