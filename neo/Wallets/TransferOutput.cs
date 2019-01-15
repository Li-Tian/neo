using Neo.Network.P2P.Payloads;
using System;

namespace Neo.Wallets
{
    // <summary>
    // 转账的输出
    // </summary>
    /// <summary>
    /// Transfer Output
    /// </summary>
    public class TransferOutput
    {
        // <summary>
        // 资产的Id
        // </summary>
        /// <summary>
        /// Asset Id
        /// </summary>
        public UIntBase AssetId;

        // <summary>
        // 资产的数值
        // </summary>
        /// <summary>
        /// asset value
        /// </summary>
        public BigDecimal Value;

        // <summary>
        // 这个TransferOutput的账户地址
        // </summary>
        /// <summary>
        /// account address of this transferoutput
        /// </summary>
        public UInt160 ScriptHash;


        // <summary>
        // 根据AssetId来返回这个TransferOutput是否是一个全局的Asset
        // </summary>
        /// <summary>
        /// Determine if transfer output is a global asset based on asset id
        /// </summary>
        public bool IsGlobalAsset => AssetId.Size == 32;


        // <summary>
        // 当前这个TransferOutput转换为一个TransactionOutput
        // </summary>
        // <returns>返回转化后的TransactionOutput</returns>
        /// <summary>
        /// Convert a transferoutput to a transaction output
        /// </summary>
        /// <returns>TransactionOutput object</returns>
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
