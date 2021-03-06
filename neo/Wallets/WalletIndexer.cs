﻿using Neo.IO;
using Neo.IO.Data.LevelDB;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace Neo.Wallets
{
    /// <summary>
    /// 钱包索引
    /// </summary>
    public class WalletIndexer : IDisposable
    {
        /// <summary>
        /// 钱包交易的委托，在收到交易时，调用绑定的方法
        /// </summary>
        public event EventHandler<WalletTransactionEventArgs> WalletTransaction;

        private readonly Dictionary<uint, HashSet<UInt160>> indexes = new Dictionary<uint, HashSet<UInt160>>();
        private readonly Dictionary<UInt160, HashSet<CoinReference>> accounts_tracked = new Dictionary<UInt160, HashSet<CoinReference>>();
        private readonly Dictionary<CoinReference, Coin> coins_tracked = new Dictionary<CoinReference, Coin>();

        private readonly DB db;
        private readonly Thread thread;
        private readonly object SyncRoot = new object();
        private bool disposed = false;
        /// <summary>
        /// 索引的高度
        /// </summary>
        public uint IndexHeight
        {
            get
            {
                lock (SyncRoot)
                {
                    if (indexes.Count == 0) return 0;
                    return indexes.Keys.Min();
                }
            }
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="path">数据库文件的路径</param>
        public WalletIndexer(string path)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(path);
            db = DB.Open(path, new Options { CreateIfMissing = true });
            if (db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), out Slice value) && Version.TryParse(value.ToString(), out Version version) && version >= Version.Parse("2.5.4"))
            {
                ReadOptions options = new ReadOptions { FillCache = false };
                foreach (var group in db.Find(options, SliceBuilder.Begin(DataEntryPrefix.IX_Group), (k, v) => new
                {
                    Height = k.ToUInt32(1),
                    Id = v.ToArray()
                }))
                {
                    UInt160[] accounts = db.Get(options, SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(group.Id)).ToArray().AsSerializableArray<UInt160>();
                    indexes.Add(group.Height, new HashSet<UInt160>(accounts));
                    foreach (UInt160 account in accounts)
                        accounts_tracked.Add(account, new HashSet<CoinReference>());
                }
                foreach (Coin coin in db.Find(options, SliceBuilder.Begin(DataEntryPrefix.ST_Coin), (k, v) => new Coin
                {
                    Reference = k.ToArray().Skip(1).ToArray().AsSerializable<CoinReference>(),
                    Output = v.ToArray().AsSerializable<TransactionOutput>(),
                    State = (CoinState)v.ToArray()[60]
                }))
                {
                    accounts_tracked[coin.Output.ScriptHash].Add(coin.Reference);
                    coins_tracked.Add(coin.Reference, coin);
                }
            }
            else
            {
                WriteBatch batch = new WriteBatch();
                ReadOptions options = new ReadOptions { FillCache = false };
                using (Iterator it = db.NewIterator(options))
                {
                    for (it.SeekToFirst(); it.Valid(); it.Next())
                    {
                        batch.Delete(it.Key());
                    }
                }
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_Version), Assembly.GetExecutingAssembly().GetName().Version.ToString());
                db.Write(WriteOptions.Default, batch);
            }
            thread = new Thread(ProcessBlocks)
            {
                IsBackground = true,
                Name = $"{nameof(WalletIndexer)}.{nameof(ProcessBlocks)}"
            };
            thread.Start();
        }
        /// <summary>
        /// 回收方法
        /// </summary>
        public void Dispose()
        {
            disposed = true;
            thread.Join();
            db.Dispose();
        }
        /// <summary>
        /// 获取钱包索引内与指定账户集合有关联的Coin集合
        /// </summary>
        /// <param name="accounts">指定账户集合</param>
        /// <returns>关联的Coin集合</returns>
        public IEnumerable<Coin> GetCoins(IEnumerable<UInt160> accounts)
        {
            lock (SyncRoot)
            {
                foreach (UInt160 account in accounts)
                    foreach (CoinReference reference in accounts_tracked[account])
                        yield return coins_tracked[reference];
            }
        }

        private static byte[] GetGroupId()
        {
            byte[] groupId = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(groupId);
            }
            return groupId;
        }
        /// <summary>
        /// 查找数据库中与指定账户有关的交易的集合
        /// </summary>
        /// <param name="accounts">指定账户的地址的集合</param>
        /// <returns>有关的交易的哈希的集合</returns>
        public IEnumerable<UInt256> GetTransactions(IEnumerable<UInt160> accounts)
        {
            ReadOptions options = new ReadOptions { FillCache = false };
            IEnumerable<UInt256> results = Enumerable.Empty<UInt256>();
            foreach (UInt160 account in accounts)
                results = results.Union(db.Find(options, SliceBuilder.Begin(DataEntryPrefix.ST_Transaction).Add(account), (k, v) => new UInt256(k.ToArray().Skip(21).ToArray())));
            foreach (UInt256 hash in results)
                yield return hash;
        }

        private void ProcessBlock(Block block, HashSet<UInt160> accounts, WriteBatch batch)
        {
            foreach (Transaction tx in block.Transactions)
            {
                HashSet<UInt160> accounts_changed = new HashSet<UInt160>();
                for (ushort index = 0; index < tx.Outputs.Length; index++)
                {
                    TransactionOutput output = tx.Outputs[index];
                    if (accounts_tracked.ContainsKey(output.ScriptHash))
                    {
                        CoinReference reference = new CoinReference
                        {
                            PrevHash = tx.Hash,
                            PrevIndex = index
                        };
                        if (coins_tracked.TryGetValue(reference, out Coin coin))
                        {
                            coin.State |= CoinState.Confirmed;
                        }
                        else
                        {
                            accounts_tracked[output.ScriptHash].Add(reference);
                            coins_tracked.Add(reference, coin = new Coin
                            {
                                Reference = reference,
                                Output = output,
                                State = CoinState.Confirmed
                            });
                        }
                        batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_Coin).Add(reference), SliceBuilder.Begin().Add(output).Add((byte)coin.State));
                        accounts_changed.Add(output.ScriptHash);
                    }
                }
                foreach (CoinReference input in tx.Inputs)
                {
                    if (coins_tracked.TryGetValue(input, out Coin coin))
                    {
                        if (coin.Output.AssetId.Equals(Blockchain.GoverningToken.Hash))
                        {
                            coin.State |= CoinState.Spent | CoinState.Confirmed;
                            batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_Coin).Add(input), SliceBuilder.Begin().Add(coin.Output).Add((byte)coin.State));
                        }
                        else
                        {
                            accounts_tracked[coin.Output.ScriptHash].Remove(input);
                            coins_tracked.Remove(input);
                            batch.Delete(DataEntryPrefix.ST_Coin, input);
                        }
                        accounts_changed.Add(coin.Output.ScriptHash);
                    }
                }
                switch (tx)
                {
                    case MinerTransaction _:
                    case ContractTransaction _:
#pragma warning disable CS0612
                    case PublishTransaction _:
#pragma warning restore CS0612
                        break;
                    case ClaimTransaction tx_claim:
                        foreach (CoinReference claim in tx_claim.Claims)
                        {
                            if (coins_tracked.TryGetValue(claim, out Coin coin))
                            {
                                accounts_tracked[coin.Output.ScriptHash].Remove(claim);
                                coins_tracked.Remove(claim);
                                batch.Delete(DataEntryPrefix.ST_Coin, claim);
                                accounts_changed.Add(coin.Output.ScriptHash);
                            }
                        }
                        break;
#pragma warning disable CS0612
                    case EnrollmentTransaction tx_enrollment:
                        if (accounts_tracked.ContainsKey(tx_enrollment.ScriptHash))
                            accounts_changed.Add(tx_enrollment.ScriptHash);
                        break;
                    case RegisterTransaction tx_register:
                        if (accounts_tracked.ContainsKey(tx_register.OwnerScriptHash))
                            accounts_changed.Add(tx_register.OwnerScriptHash);
                        break;
#pragma warning restore CS0612
                    default:
                        foreach (UInt160 hash in tx.Witnesses.Select(p => p.ScriptHash))
                            if (accounts_tracked.ContainsKey(hash))
                                accounts_changed.Add(hash);
                        break;
                }
                if (accounts_changed.Count > 0)
                {
                    foreach (UInt160 account in accounts_changed)
                        batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_Transaction).Add(account).Add(tx.Hash), false);
                    WalletTransaction?.Invoke(null, new WalletTransactionEventArgs
                    {
                        Transaction = tx,
                        RelatedAccounts = accounts_changed.ToArray(),
                        Height = block.Index,
                        Time = block.Timestamp
                    });
                }
            }
        }

        private void ProcessBlocks()
        {
            while (!disposed)
            {
                while (!disposed)
                    lock (SyncRoot)
                    {
                        if (indexes.Count == 0) break;
                        uint height = indexes.Keys.Min();
                        Block block = Blockchain.Singleton.Store.GetBlock(height);
                        if (block == null) break;
                        WriteBatch batch = new WriteBatch();
                        HashSet<UInt160> accounts = indexes[height];
                        ProcessBlock(block, accounts, batch);
                        ReadOptions options = ReadOptions.Default;
                        byte[] groupId = db.Get(options, SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height)).ToArray();
                        indexes.Remove(height);
                        batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height));
                        height++;
                        if (indexes.TryGetValue(height, out HashSet<UInt160> accounts_next))
                        {
                            accounts_next.UnionWith(accounts);
                            groupId = db.Get(options, SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height)).ToArray();
                            batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(groupId), accounts_next.ToArray().ToByteArray());
                        }
                        else
                        {
                            indexes.Add(height, accounts);
                            batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height), groupId);
                        }
                        db.Write(WriteOptions.Default, batch);
                    }
                for (int i = 0; i < 20 && !disposed; i++)
                    Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 重建钱包索引
        /// </summary>
        public void RebuildIndex()
        {
            lock (SyncRoot)
            {
                WriteBatch batch = new WriteBatch();
                ReadOptions options = new ReadOptions { FillCache = false };
                foreach (uint height in indexes.Keys)
                {
                    byte[] groupId = db.Get(options, SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height)).ToArray();
                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height));
                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(groupId));
                }
                indexes.Clear();
                if (accounts_tracked.Count > 0)
                {
                    indexes[0] = new HashSet<UInt160>(accounts_tracked.Keys);
                    byte[] groupId = GetGroupId();
                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(0u), groupId);
                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(groupId), accounts_tracked.Keys.ToArray().ToByteArray());
                    foreach (HashSet<CoinReference> coins in accounts_tracked.Values)
                        coins.Clear();
                }
                foreach (CoinReference reference in coins_tracked.Keys)
                    batch.Delete(DataEntryPrefix.ST_Coin, reference);
                coins_tracked.Clear();
                foreach (Slice key in db.Find(options, SliceBuilder.Begin(DataEntryPrefix.ST_Transaction), (k, v) => k))
                    batch.Delete(key);
                db.Write(WriteOptions.Default, batch);
            }
        }
        /// <summary>
        /// 向钱包索引中注册账户，并存储到数据库中
        /// </summary>
        /// <param name="accounts">需要注册的账户列表集合</param>
        /// <param name="height">钱包高度</param>
        public void RegisterAccounts(IEnumerable<UInt160> accounts, uint height = 0)
        {
            lock (SyncRoot)
            {
                bool index_exists = indexes.TryGetValue(height, out HashSet<UInt160> index);
                if (!index_exists) index = new HashSet<UInt160>();
                foreach (UInt160 account in accounts)
                    if (!accounts_tracked.ContainsKey(account))
                    {
                        index.Add(account);
                        accounts_tracked.Add(account, new HashSet<CoinReference>());
                    }
                if (index.Count > 0)
                {
                    WriteBatch batch = new WriteBatch();
                    byte[] groupId;
                    if (!index_exists)
                    {
                        indexes.Add(height, index);
                        groupId = GetGroupId();
                        batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height), groupId);
                    }
                    else
                    {
                        groupId = db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height)).ToArray();
                    }
                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(groupId), index.ToArray().ToByteArray());
                    db.Write(WriteOptions.Default, batch);
                }
            }
        }
        /// <summary>
        /// 向钱包索引中删除指定账户，并删除数据库中的记录
        /// </summary>
        /// <param name="accounts">需要删除的账户列表集合</param>
        public void UnregisterAccounts(IEnumerable<UInt160> accounts)
        {
            lock (SyncRoot)
            {
                WriteBatch batch = new WriteBatch();
                ReadOptions options = new ReadOptions { FillCache = false };
                foreach (UInt160 account in accounts)
                {
                    if (accounts_tracked.TryGetValue(account, out HashSet<CoinReference> references))
                    {
                        foreach (uint height in indexes.Keys.ToArray())
                        {
                            HashSet<UInt160> index = indexes[height];
                            if (index.Remove(account))
                            {
                                byte[] groupId = db.Get(options, SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height)).ToArray();
                                if (index.Count == 0)
                                {
                                    indexes.Remove(height);
                                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Group).Add(height));
                                    batch.Delete(SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(groupId));
                                }
                                else
                                {
                                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_Accounts).Add(groupId), index.ToArray().ToByteArray());
                                }
                                break;
                            }
                        }
                        accounts_tracked.Remove(account);
                        foreach (CoinReference reference in references)
                        {
                            batch.Delete(DataEntryPrefix.ST_Coin, reference);
                            coins_tracked.Remove(reference);
                        }
                        foreach (Slice key in db.Find(options, SliceBuilder.Begin(DataEntryPrefix.ST_Transaction).Add(account), (k, v) => k))
                            batch.Delete(key);
                    }
                }
                db.Write(WriteOptions.Default, batch);
            }
        }
    }
}
