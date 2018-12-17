using Neo.Network.P2P.Payloads;
using System;

namespace Neo.Wallets
{
    /// <summary>
    /// 转账的输出
    /// </summary>
    public class TransferOutput
    {
        /// <summary>
        /// 资产的Id
        /// </summary>
        public UIntBase AssetId;

        /// <summary>
        /// 资产的数值
        /// </summary>
        public BigDecimal Value;

        /// <summary>
        /// 这个TransferOutput的账户地址
        /// </summary>
        public UInt160 ScriptHash;


        /// <summary>
        /// 根据AssetId来返回这个TransferOutput是否是一个全局的Asset
        /// </summary>
        public bool IsGlobalAsset => AssetId.Size == 32;


        /// <summary>
        /// 当前这个TransferOutput转换为一个TransactionOutput
        /// </summary>
        /// <returns>返回转化后的TransactionOutput</returns>
        public TransactionOutput ToTxOutput()
        {
            if (AssetId is UInt256 asset_id)
                return new TransactionOutput
                {
                    AssetId = asset_id,
                    Value = Value.ToFixed8(),
                    ScriptHash = ScriptHash
                };
            throw new NotSupportedException();
        }
    }
}
