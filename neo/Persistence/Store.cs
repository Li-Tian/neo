using Neo.Cryptography.ECC;
using Neo.IO.Caching;
using Neo.IO.Wrappers;
using Neo.Ledger;

namespace Neo.Persistence
{
    // <summary>
    // 抽象的持久化存储
    // </summary>
    /// <summary>
    /// Abstract persistent storage
    /// </summary>
    public abstract class Store : IPersistence
    {
        // <summary>
        // 区块缓存。通过区块的哈希值快速查找区块状态。
        // </summary>
        /// <summary>
        /// Block cache. Quickly find the block status by the hash of the block.
        /// </summary>
        DataCache<UInt256, BlockState> IPersistence.Blocks => GetBlocks();
        // <summary>
        // 交易缓存。通过交易的哈希值快速查找交易状态。
        // </summary>
        /// <summary>
        /// Transaction cache. Quickly find transaction status by the hash of the transaction.
        /// </summary>
        DataCache<UInt256, TransactionState> IPersistence.Transactions => GetTransactions();
        // <summary>
        // 账户缓存。通过账户的哈希值快速查找账户状态。
        // </summary>
        /// <summary>
        /// Account cache. Quickly find the account status by the hash of the account.
        /// </summary>
        DataCache<UInt160, AccountState> IPersistence.Accounts => GetAccounts();
        // <summary>
        // UTXO缓存。通过交易哈希快速查找该交易下所有UTXO的状态。
        // </summary>
        /// <summary>
        /// UTXO cache. Quickly find the status of all UTXOs under the transaction by the hash of the transaction.
        /// </summary>
        DataCache<UInt256, UnspentCoinState> IPersistence.UnspentCoins => GetUnspentCoins();
        // <summary>
        // 已花费的UTXO相关信息缓存。通过交易的哈希值快速查找已花费的UTXO的信息。
        // 包括交易所在的区块高度和交易中已经被花费的UTXO在被花费时的所处区块的高度。
        // </summary>
        /// <summary>
        /// The information cacahe about the UTXO that has been spent. <br/>
        /// Quickly find information about the UTXO that has been spent by hashing the transaction. <br/>
        /// This includes the block height of the transaction <br/>
        /// and the height of the block in which the UTXO that has been spent.
        /// </summary>
        DataCache<UInt256, SpentCoinState> IPersistence.SpentCoins => GetSpentCoins();
        // <summary>
        // 验证人的缓存。通过公钥快速查询验证人的状态。包括公钥，是否已经注册，投票数。
        // </summary>
        /// <summary>
        /// The Validators cache. Quickly query the status of the validator through the public key. Including the public key, whether it has been registered, and the number of votes.
        /// </summary>
        DataCache<ECPoint, ValidatorState> IPersistence.Validators => GetValidators();
        // <summary>
        // 资产的缓存。通过哈希快速查找资产的状态。
        // </summary>
        /// <summary>
        /// The assets cache. Quickly find the status of an asset by the hash.
        /// </summary>
        DataCache<UInt256, AssetState> IPersistence.Assets => GetAssets();
        // <summary>
        // 智能合约的缓存。通过智能合约的哈希值快速查找智能合约的信息。
        // </summary>
        /// <summary>
        /// Smart contracts cache. Quickly find information about smart contracts through the hash of smart contracts.
        /// </summary>
        DataCache<UInt160, ContractState> IPersistence.Contracts => GetContracts();
        // <summary>
        // 合约的键值对存储。通过脚本哈希和存储key查询value。
        // </summary>
        /// <summary>
        /// Key-value pair storage for contracts. Query value by script hash and storing key.
        /// </summary>
        DataCache<StorageKey, StorageItem> IPersistence.Storages => GetStorages();
        // <summary>
        // 区块头哈希列表的缓存。
        // 每个区块头哈希列表包含2000个区块头的哈希值。
        // 然后第一个区块头哈希列表的编号是0。
        // 第二个区块头哈希列表的编号是2000。以此类推。
        // 这个缓存通过区块头哈希列表的编号快速查找区块头哈希列表。
        // </summary>
        /// <summary>
        /// Block header hash lists cache. <br/>
        /// Each block header hash list contains the hash values of 2000 block headers. <br/>
        /// The number of the first block header hash list is 0. <br/>
        /// The number of the second block header hash list is 2000. And so on. <br/>
        /// This cache quickly finds the block header hash list by the number of the block header hash list.
        /// </summary>
        DataCache<UInt32Wrapper, HeaderHashList> IPersistence.HeaderHashList => GetHeaderHashList();
        // <summary>
        // 验证人个数的投票池。投票时用来点票。
        // </summary>
        /// <summary>
        /// The voting pool of ValidatorsCount. Used to vote for votes.
        /// </summary>
        MetaDataCache<ValidatorsCountState> IPersistence.ValidatorsCount => GetValidatorsCount();
        // <summary>
        // 区块索引。存放最新的区块的哈希值和高度。
        // </summary>
        /// <summary>
        /// Block index. Stores the hash value and height of the latest block.
        /// </summary>
        MetaDataCache<HashIndexState> IPersistence.BlockHashIndex => GetBlockHashIndex();
        // <summary>
        // 区块头索引。存放最新的区块头的哈希值和高度。
        // </summary>
        /// <summary>
        /// Block header index. Stores the hash and height of the latest block header.
        /// </summary>
        MetaDataCache<HashIndexState> IPersistence.HeaderHashIndex => GetHeaderHashIndex();

        // <summary>
        // 获取区块
        // </summary>
        // <returns>区块对应的DataCache</returns>
        /// <summary>
        /// Get blocks
        /// </summary>
        /// <returns>The DataCache corresponding to the blocks</returns>
        public abstract DataCache<UInt256, BlockState> GetBlocks();

        // <summary>
        // 获取交易
        // </summary>
        // <returns>交易对应的的DbCache</returns>
        /// <summary>
        /// Get transactions
        /// </summary>
        /// <returns>The DataCache corresponding to the transactions</returns>
        public abstract DataCache<UInt256, TransactionState> GetTransactions();

        // <summary>
        // 获取账户
        // </summary>
        // <returns>账户对应的DataCache</returns>
        /// <summary>
        /// Get accounts
        /// </summary>
        /// <returns>The DataCache corresponding to the accounts</returns>
        public abstract DataCache<UInt160, AccountState> GetAccounts();

        // <summary>
        // 获取UTXO
        // </summary>
        // <returns>UTXO对应的DataCache</returns>
        /// <summary>
        /// Get UTXO
        /// </summary>
        /// <returns>The DataCache corresponding to the UTXO</returns>
        public abstract DataCache<UInt256, UnspentCoinState> GetUnspentCoins();

        // <summary>
        // 获取已花费Coins
        // </summary>
        // <returns>已花费Coins对应的DataCache</returns>
        /// <summary>
        /// Get spent coins
        /// </summary>
        /// <returns>The DataCache corresponding to the spent coins</returns>
        public abstract DataCache<UInt256, SpentCoinState> GetSpentCoins();

        // <summary>
        // 获取验证人
        // </summary>
        // <returns>验证人对应的DataCache</returns>
        /// <summary>
        /// Get validators
        /// </summary>
        /// <returns>The DataCache corresponding to the validators</returns>
        public abstract DataCache<ECPoint, ValidatorState> GetValidators();

        // <summary>
        // 获取资产
        // </summary>
        // <returns>资产对应的DataCache</returns>
        /// <summary>
        /// Get assets
        /// </summary>
        /// <returns>The DataCache corresponding to the assets</returns>
        public abstract DataCache<UInt256, AssetState> GetAssets();

        // <summary>
        // 获取合约
        // </summary>
        // <returns>合约对应的DataCache</returns>
        /// <summary>
        /// Get contracts
        /// </summary>
        /// <returns>The DataCache corresponding to the contracts</returns>
        public abstract DataCache<UInt160, ContractState> GetContracts();

        // <summary>
        // 获取合约存储空间
        // </summary>
        // <returns>合约存储空间对应的DataCache</returns>
        /// <summary>
        /// Get storages
        /// </summary>
        /// <returns>The DataCache corresponding to the storages</returns>
        public abstract DataCache<StorageKey, StorageItem> GetStorages();

        // <summary>
        // 获取区块头hash列表
        // </summary>
        // <returns>区块头hash列表对应的DataCache</returns>
        /// <summary>
        /// Get the block header hash list
        /// </summary>
        /// <returns>The DataCache corresponding to the block header hash list</returns>
        public abstract DataCache<UInt32Wrapper, HeaderHashList> GetHeaderHashList();

        // <summary>
        // 获取验证人个数投票
        // </summary>
        // <returns>验证人个数投票对应的MetaDataCache</returns>
        /// <summary>
        /// Get validators count
        /// </summary>
        /// <returns>The MetaDataCache corresponding to the validators count</returns>
        public abstract MetaDataCache<ValidatorsCountState> GetValidatorsCount();

        // <summary>
        // 获取区块索引
        // </summary>
        // <returns>区块索引对应的MetaDataCache</returns>
        /// <summary>
        /// Get block hash index
        /// </summary>
        /// <returns>The MetaDataCache corresponding to the block hash index</returns>
        public abstract MetaDataCache<HashIndexState> GetBlockHashIndex();

        // <summary>
        // 获取区块头索引
        // </summary>
        // <returns>区块头索引对应的MetaDataCache</returns>
        /// <summary>
        /// Get block header hash index
        /// </summary>
        /// <returns>The MetaDataCache corresponding to the block header hash index</returns>
        public abstract MetaDataCache<HashIndexState> GetHeaderHashIndex();

        // <summary>
        // 获取快照
        // </summary>
        // <returns>快照</returns>
        /// <summary>
        /// Get snapshot
        /// </summary>
        /// <returns>snapshot</returns>
        public abstract Snapshot GetSnapshot();
    }
}
