using Neo.Cryptography.ECC;
using Neo.IO.Caching;
using Neo.IO.Wrappers;
using Neo.Ledger;

namespace Neo.Persistence
{
    /// <summary>
    /// 抽象的持久化存储
    /// </summary>
    public abstract class Store : IPersistence
    {
        //TODO(*)添加see标签
        DataCache<UInt256, BlockState> IPersistence.Blocks => GetBlocks();
        DataCache<UInt256, TransactionState> IPersistence.Transactions => GetTransactions();
        DataCache<UInt160, AccountState> IPersistence.Accounts => GetAccounts();
        DataCache<UInt256, UnspentCoinState> IPersistence.UnspentCoins => GetUnspentCoins();
        DataCache<UInt256, SpentCoinState> IPersistence.SpentCoins => GetSpentCoins();
        DataCache<ECPoint, ValidatorState> IPersistence.Validators => GetValidators();
        DataCache<UInt256, AssetState> IPersistence.Assets => GetAssets();
        DataCache<UInt160, ContractState> IPersistence.Contracts => GetContracts();
        DataCache<StorageKey, StorageItem> IPersistence.Storages => GetStorages();
        DataCache<UInt32Wrapper, HeaderHashList> IPersistence.HeaderHashList => GetHeaderHashList();
        MetaDataCache<ValidatorsCountState> IPersistence.ValidatorsCount => GetValidatorsCount();
        MetaDataCache<HashIndexState> IPersistence.BlockHashIndex => GetBlockHashIndex();
        MetaDataCache<HashIndexState> IPersistence.HeaderHashIndex => GetHeaderHashIndex();

        /// <summary>
        /// 获取区块
        /// </summary>
        /// <returns>区块对应的DataCache</returns>
        public abstract DataCache<UInt256, BlockState> GetBlocks();

        /// <summary>
        /// 获取交易
        /// </summary>
        /// <returns>交易对应的的DbCache</returns>
        public abstract DataCache<UInt256, TransactionState> GetTransactions();

        /// <summary>
        /// 获取账户
        /// </summary>
        /// <returns>账户对应的DataCache</returns>
        public abstract DataCache<UInt160, AccountState> GetAccounts();

        /// <summary>
        /// 获取UTXO
        /// </summary>
        /// <returns>UTXO对应的DataCache</returns>
        public abstract DataCache<UInt256, UnspentCoinState> GetUnspentCoins();

        /// <summary>
        /// 获取已花费Coins
        /// </summary>
        /// <returns>已花费Coins对应的DataCache</returns>
        public abstract DataCache<UInt256, SpentCoinState> GetSpentCoins();

        /// <summary>
        /// 获取验证人
        /// </summary>
        /// <returns>验证人对应的DataCache</returns>
        public abstract DataCache<ECPoint, ValidatorState> GetValidators();

        /// <summary>
        /// 获取资产
        /// </summary>
        /// <returns>资产对应的DataCache</returns>
        public abstract DataCache<UInt256, AssetState> GetAssets();

        /// <summary>
        /// 获取合约
        /// </summary>
        /// <returns>合约对应的DataCache</returns>
        public abstract DataCache<UInt160, ContractState> GetContracts();

        /// <summary>
        /// 获取合约存储空间
        /// </summary>
        /// <returns>合约存储空间对应的DataCache</returns>
        public abstract DataCache<StorageKey, StorageItem> GetStorages();

        /// <summary>
        /// 获取区块头hash列表
        /// </summary>
        /// <returns>区块头hash列表对应的DataCache</returns>
        public abstract DataCache<UInt32Wrapper, HeaderHashList> GetHeaderHashList();

        /// <summary>
        /// 获取验证人个数投票
        /// </summary>
        /// <returns>验证人个数投票对应的MetaDataCache</returns>
        public abstract MetaDataCache<ValidatorsCountState> GetValidatorsCount();

        /// <summary>
        /// 获取区块索引
        /// </summary>
        /// <returns>区块索引对应的MetaDataCache</returns>
        public abstract MetaDataCache<HashIndexState> GetBlockHashIndex();

        /// <summary>
        /// 获取区块头索引
        /// </summary>
        /// <returns>区块头索引对应的MetaDataCache</returns>
        public abstract MetaDataCache<HashIndexState> GetHeaderHashIndex();

        /// <summary>
        /// 获取快照
        /// </summary>
        /// <returns>快照</returns>
        public abstract Snapshot GetSnapshot();
    }
}
