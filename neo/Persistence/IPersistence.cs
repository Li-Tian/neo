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
        /// 区块缓存。通过区块的哈希值快速查找区块状态。
        /// </summary>
        DataCache<UInt256, BlockState> Blocks { get; }

        /// <summary>
        /// 交易缓存。通过交易的哈希值快速查找交易状态。
        /// </summary>
        DataCache<UInt256, TransactionState> Transactions { get; }

        /// <summary>
        /// 账户缓存。通过账户的哈希值快速查找账户状态。
        /// </summary>
        DataCache<UInt160, AccountState> Accounts { get; }

        /// <summary>
        /// UTXO缓存。通过交易哈希快速查找该交易下所有UTXO的状态。
        /// </summary>
        DataCache<UInt256, UnspentCoinState> UnspentCoins { get; }

        /// <summary>
        /// 已花费的UTXO相关信息缓存。通过交易的哈希值快速查找已花费的UTXO的信息。
        /// 包括交易所在的区块高度和交易中已经被花费的UTXO在被花费时的所处区块的高度。
        /// </summary>
        DataCache<UInt256, SpentCoinState> SpentCoins { get; }

        /// <summary>
        /// 验证人的缓存。通过公钥快速查询验证人的状态。包括公钥，是否已经注册，投票数。
        /// </summary>
        DataCache<ECPoint, ValidatorState> Validators { get; }

        /// <summary>
        /// 资产的缓存。通过哈希快速查找资产的状态。
        /// </summary>
        DataCache<UInt256, AssetState> Assets { get; }

        /// <summary>
        /// 智能合约的缓存。通过智能合约的哈希值快速查找智能合约的信息。
        /// </summary>
        DataCache<UInt160, ContractState> Contracts { get; }

        /// <summary>
        /// 合约的键值对存储。通过脚本哈希和存储key查询value。
        /// </summary>
        DataCache<StorageKey, StorageItem> Storages { get; }

        /// <summary>
        /// 区块头哈希列表的缓存。
        /// 每个区块头哈希列表包含2000个区块头的哈希值。
        /// 然后第一个区块头哈希列表的编号是0。
        /// 第二个区块头哈希列表的编号是2000。以此类推。
        /// 这个缓存通过区块头哈希列表的编号快速查找区块头哈希列表。
        /// </summary>
        DataCache<UInt32Wrapper, HeaderHashList> HeaderHashList { get; }

        /// <summary>
        /// 验证人个数的投票池。投票时用来点票。
        /// </summary>
        MetaDataCache<ValidatorsCountState> ValidatorsCount { get; }

        /// <summary>
        /// 区块索引。存放最新的区块的哈希值和高度。
        /// </summary>
        MetaDataCache<HashIndexState> BlockHashIndex { get; }

        /// <summary>
        /// 区块头索引。存放最新的区块头的哈希值和高度。
        /// </summary>
        MetaDataCache<HashIndexState> HeaderHashIndex { get; }
    }
}
