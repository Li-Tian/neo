using Neo.Cryptography.ECC;
using Neo.IO.Caching;
using Neo.IO.Wrappers;
using Neo.Ledger;

namespace Neo.Persistence
{
    /// <summary>
    /// 持久化操作接口
    /// </summary>
    public interface IPersistence
    {
        /// <summary>
        /// 区块
        /// </summary>
        DataCache<UInt256, BlockState> Blocks { get; }

        /// <summary>
        /// 交易
        /// </summary>
        DataCache<UInt256, TransactionState> Transactions { get; }

        /// <summary>
        /// 账户
        /// </summary>
        DataCache<UInt160, AccountState> Accounts { get; }

        /// <summary>
        /// UTXO
        /// </summary>
        DataCache<UInt256, UnspentCoinState> UnspentCoins { get; }

        /// <summary>
        /// 已花费的交易
        /// </summary>
        DataCache<UInt256, SpentCoinState> SpentCoins { get; }

        /// <summary>
        /// 验证人
        /// </summary>
        DataCache<ECPoint, ValidatorState> Validators { get; }

        /// <summary>
        /// 资产
        /// </summary>
        DataCache<UInt256, AssetState> Assets { get; }

        /// <summary>
        /// 合约
        /// </summary>
        DataCache<UInt160, ContractState> Contracts { get; }

        /// <summary>
        /// 合约的键值对存储
        /// </summary>
        DataCache<StorageKey, StorageItem> Storages { get; }

        /// <summary>
        /// 区块头hash列表
        /// </summary>
        DataCache<UInt32Wrapper, HeaderHashList> HeaderHashList { get; }

        /// <summary>
        /// 验证人个数的投票
        /// </summary>
        MetaDataCache<ValidatorsCountState> ValidatorsCount { get; }

        /// <summary>
        /// 区块索引
        /// </summary>
        MetaDataCache<HashIndexState> BlockHashIndex { get; }

        /// <summary>
        /// 区块头索引
        /// </summary>
        MetaDataCache<HashIndexState> HeaderHashIndex { get; }
    }
}
