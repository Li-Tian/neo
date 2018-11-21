using Neo.Cryptography.ECC;
using Neo.IO.Caching;
using Neo.IO.Data.LevelDB;
using Neo.IO.Wrappers;
using Neo.Ledger;
using System;
using System.Reflection;

namespace Neo.Persistence.LevelDB
{
    /// <summary>
    /// Leveldb存储器
    /// </summary>
    public class LevelDBStore : Store, IDisposable
    {
        private readonly DB db;

        /// <summary>
        /// 打开leveldb数据库
        /// </summary>
        /// <param name="path">数据库路径</param>
        public LevelDBStore(string path)
        {
            this.db = DB.Open(path, new Options { CreateIfMissing = true });
            if (db.TryGet(ReadOptions.Default, SliceBuilder.Begin(Prefixes.SYS_Version), out Slice value) && Version.TryParse(value.ToString(), out Version version) && version >= Version.Parse("2.9.1"))
                return;
            WriteBatch batch = new WriteBatch();
            ReadOptions options = new ReadOptions { FillCache = false };
            using (Iterator it = db.NewIterator(options))
            {
                for (it.SeekToFirst(); it.Valid(); it.Next())
                {
                    batch.Delete(it.Key());
                }
            }
            db.Put(WriteOptions.Default, SliceBuilder.Begin(Prefixes.SYS_Version), Assembly.GetExecutingAssembly().GetName().Version.ToString());
            db.Write(WriteOptions.Default, batch);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// 获取账户
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt160, AccountState> GetAccounts()
        {
            return new DbCache<UInt160, AccountState>(db, null, null, Prefixes.ST_Account);
        }

        /// <summary>
        /// 获取资产
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt256, AssetState> GetAssets()
        {
            return new DbCache<UInt256, AssetState>(db, null, null, Prefixes.ST_Asset);
        }


        /// <summary>
        /// 获取区块
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt256, BlockState> GetBlocks()
        {
            return new DbCache<UInt256, BlockState>(db, null, null, Prefixes.DATA_Block);
        }

        /// <summary>
        /// 获取合约
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt160, ContractState> GetContracts()
        {
            return new DbCache<UInt160, ContractState>(db, null, null, Prefixes.ST_Contract);
        }

        /// <summary>
        /// 获取快照
        /// </summary>
        /// <returns></returns>
        public override Snapshot GetSnapshot()
        {
            return new DbSnapshot(db);
        }


        /// <summary>
        /// 获取已花费交易
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt256, SpentCoinState> GetSpentCoins()
        {
            return new DbCache<UInt256, SpentCoinState>(db, null, null, Prefixes.ST_SpentCoin);
        }

        /// <summary>
        /// 获取合约的存储
        /// </summary>
        /// <returns></returns>
        public override DataCache<StorageKey, StorageItem> GetStorages()
        {
            return new DbCache<StorageKey, StorageItem>(db, null, null, Prefixes.ST_Storage);
        }

        /// <summary>
        /// 获取交易
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt256, TransactionState> GetTransactions()
        {
            return new DbCache<UInt256, TransactionState>(db, null, null, Prefixes.DATA_Transaction);
        }


        /// <summary>
        /// 获取utxo
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt256, UnspentCoinState> GetUnspentCoins()
        {
            return new DbCache<UInt256, UnspentCoinState>(db, null, null, Prefixes.ST_Coin);
        }


        /// <summary>
        /// 获取验证人
        /// </summary>
        /// <returns></returns>
        public override DataCache<ECPoint, ValidatorState> GetValidators()
        {
            return new DbCache<ECPoint, ValidatorState>(db, null, null, Prefixes.ST_Validator);
        }


        /// <summary>
        /// 获取区块头hash列表
        /// </summary>
        /// <returns></returns>
        public override DataCache<UInt32Wrapper, HeaderHashList> GetHeaderHashList()
        {
            return new DbCache<UInt32Wrapper, HeaderHashList>(db, null, null, Prefixes.IX_HeaderHashList);
        }


        /// <summary>
        /// 获取验证人个数的投票
        /// </summary>
        /// <returns></returns>
        public override MetaDataCache<ValidatorsCountState> GetValidatorsCount()
        {
            return new DbMetaDataCache<ValidatorsCountState>(db, null, null, Prefixes.IX_ValidatorsCount);
        }

        /// <summary>
        /// 获取区块索引
        /// </summary>
        /// <returns></returns>
        public override MetaDataCache<HashIndexState> GetBlockHashIndex()
        {
            return new DbMetaDataCache<HashIndexState>(db, null, null, Prefixes.IX_CurrentBlock);
        }


        /// <summary>
        /// 获取区块头索引
        /// </summary>
        /// <returns></returns>
        public override MetaDataCache<HashIndexState> GetHeaderHashIndex()
        {
            return new DbMetaDataCache<HashIndexState>(db, null, null, Prefixes.IX_CurrentHeader);
        }
    }
}
