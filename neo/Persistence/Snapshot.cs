using Neo.Cryptography.ECC;
using Neo.IO.Caching;
using Neo.IO.Wrappers;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Persistence
{
    /// <summary>
    /// 快照
    /// </summary>
    public abstract class Snapshot : IDisposable, IPersistence, IScriptTable
    {
        /// <summary>
        /// 当前正在持久化的区块
        /// </summary>
        public Block PersistingBlock { get; internal set; }

        /// <summary>
        /// 区块
        /// </summary>
        public abstract DataCache<UInt256, BlockState> Blocks { get; }

        /// <summary>
        /// 交易
        /// </summary>
        public abstract DataCache<UInt256, TransactionState> Transactions { get; }

        /// <summary>
        /// 账户
        /// </summary>
        public abstract DataCache<UInt160, AccountState> Accounts { get; }

        /// <summary>
        /// UTXO
        /// </summary>
        public abstract DataCache<UInt256, UnspentCoinState> UnspentCoins { get; }

        /// <summary>
        /// 已花费交易
        /// </summary>
        public abstract DataCache<UInt256, SpentCoinState> SpentCoins { get; }

        /// <summary>
        /// 验证人
        /// </summary>
        public abstract DataCache<ECPoint, ValidatorState> Validators { get; }

        /// <summary>
        /// 资产
        /// </summary>
        public abstract DataCache<UInt256, AssetState> Assets { get; }

        /// <summary>
        /// 合约
        /// </summary>
        public abstract DataCache<UInt160, ContractState> Contracts { get; }

        /// <summary>
        /// 合约存储
        /// </summary>
        public abstract DataCache<StorageKey, StorageItem> Storages { get; }

        /// <summary>
        /// 区块头hash列表
        /// </summary>
        public abstract DataCache<UInt32Wrapper, HeaderHashList> HeaderHashList { get; }

        /// <summary>
        /// 验证人个数投票
        /// </summary>
        public abstract MetaDataCache<ValidatorsCountState> ValidatorsCount { get; }

        /// <summary>
        /// 区块索引
        /// </summary>
        public abstract MetaDataCache<HashIndexState> BlockHashIndex { get; }

        /// <summary>
        /// 区块头索引
        /// </summary>
        public abstract MetaDataCache<HashIndexState> HeaderHashIndex { get; }

        /// <summary>
        /// 当前区块高度
        /// </summary>
        public uint Height => BlockHashIndex.Get().Index;
    
        /// <summary>
        /// 区块头高度
        /// </summary>
        public uint HeaderHeight => HeaderHashIndex.Get().Index;

        /// <summary>
        /// 区块hash
        /// </summary>
        public UInt256 CurrentBlockHash => BlockHashIndex.Get().Hash;

        /// <summary>
        /// 区块头hash
        /// </summary>
        public UInt256 CurrentHeaderHash => HeaderHashIndex.Get().Hash;

        /// <summary>
        /// 计算可以Claim的GAS
        /// </summary>
        /// <param name="inputs">claim指向的交易</param>
        /// <param name="ignoreClaimed">是否忽略已经claims的input</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">若ignoreClaimed设置为false，且发现有已经claim的input时，抛出该异常</exception>
        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            List<SpentCoin> unclaimed = new List<SpentCoin>();
            foreach (var group in inputs.GroupBy(p => p.PrevHash))
            {
                Dictionary<ushort, SpentCoin> claimable = GetUnclaimed(group.Key);
                if (claimable == null || claimable.Count == 0)
                    if (ignoreClaimed)
                        continue;
                    else
                        throw new ArgumentException();
                foreach (CoinReference claim in group)
                {
                    if (!claimable.TryGetValue(claim.PrevIndex, out SpentCoin claimed))
                        if (ignoreClaimed)
                            continue;
                        else
                            throw new ArgumentException();
                    unclaimed.Add(claimed);
                }
            }
            return CalculateBonusInternal(unclaimed);
        }

        /// <summary>
        /// 计算可以claim到的GAS奖励
        /// </summary>
        /// <param name="inputs">已经花费尚未claim的output</param>
        /// <param name="height_end">花费的高度</param>
        /// <returns></returns>
        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint height_end)
        {
            List<SpentCoin> unclaimed = new List<SpentCoin>();
            foreach (var group in inputs.GroupBy(p => p.PrevHash))
            {
                TransactionState tx_state = Transactions.TryGet(group.Key);
                if (tx_state == null) throw new ArgumentException();
                if (tx_state.BlockIndex == height_end) continue;
                foreach (CoinReference claim in group)
                {
                    if (claim.PrevIndex >= tx_state.Transaction.Outputs.Length || !tx_state.Transaction.Outputs[claim.PrevIndex].AssetId.Equals(Blockchain.GoverningToken.Hash))
                        throw new ArgumentException();
                    unclaimed.Add(new SpentCoin
                    {
                        Output = tx_state.Transaction.Outputs[claim.PrevIndex],
                        StartHeight = tx_state.BlockIndex,
                        EndHeight = height_end
                    });
                }
            }
            return CalculateBonusInternal(unclaimed);
        }

        private Fixed8 CalculateBonusInternal(IEnumerable<SpentCoin> unclaimed)
        {
            Fixed8 amount_claimed = Fixed8.Zero;
            foreach (var group in unclaimed.GroupBy(p => new { p.StartHeight, p.EndHeight }))
            {
                uint amount = 0;
                uint ustart = group.Key.StartHeight / Blockchain.DecrementInterval;
                if (ustart < Blockchain.GenerationAmount.Length)
                {
                    uint istart = group.Key.StartHeight % Blockchain.DecrementInterval;
                    uint uend = group.Key.EndHeight / Blockchain.DecrementInterval;
                    uint iend = group.Key.EndHeight % Blockchain.DecrementInterval;
                    if (uend >= Blockchain.GenerationAmount.Length)
                    {
                        uend = (uint)Blockchain.GenerationAmount.Length;
                        iend = 0;
                    }
                    if (iend == 0)
                    {
                        uend--;
                        iend = Blockchain.DecrementInterval;
                    }
                    while (ustart < uend)
                    {
                        amount += (Blockchain.DecrementInterval - istart) * Blockchain.GenerationAmount[ustart];
                        ustart++;
                        istart = 0;
                    }
                    amount += (iend - istart) * Blockchain.GenerationAmount[ustart];
                }
                amount += (uint)(this.GetSysFeeAmount(group.Key.EndHeight - 1) - (group.Key.StartHeight == 0 ? 0 : this.GetSysFeeAmount(group.Key.StartHeight - 1)));
                amount_claimed += group.Sum(p => p.Value) / 100000000 * amount;
            }
            return amount_claimed;
        }

        /// <summary>
        /// 克隆快照
        /// </summary>
        /// <returns></returns>
        public Snapshot Clone()
        {
            return new CloneSnapshot(this);
        }

        /// <summary>
        /// 持久化到磁盘
        /// </summary>
        public virtual void Commit()
        {
            Accounts.DeleteWhere((k, v) => !v.IsFrozen && v.Votes.Length == 0 && v.Balances.All(p => p.Value <= Fixed8.Zero));
            UnspentCoins.DeleteWhere((k, v) => v.Items.All(p => p.HasFlag(CoinState.Spent)));
            SpentCoins.DeleteWhere((k, v) => v.Items.Count == 0);
            Blocks.Commit();
            Transactions.Commit();
            Accounts.Commit();
            UnspentCoins.Commit();
            SpentCoins.Commit();
            Validators.Commit();
            Assets.Commit();
            Contracts.Commit();
            Storages.Commit();
            HeaderHashList.Commit();
            ValidatorsCount.Commit();
            BlockHashIndex.Commit();
            HeaderHashIndex.Commit();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
        }

        byte[] IScriptTable.GetScript(byte[] script_hash)
        {
            return Contracts[new UInt160(script_hash)].Script;
        }

        /// <summary>
        /// 获取一笔交易尚未Claim的outputs
        /// </summary>
        /// <param name="hash">待查询交易hash</param>
        /// <returns></returns>
        public Dictionary<ushort, SpentCoin> GetUnclaimed(UInt256 hash)
        {
            TransactionState tx_state = Transactions.TryGet(hash);
            if (tx_state == null) return null;
            SpentCoinState coin_state = SpentCoins.TryGet(hash);
            if (coin_state != null)
            {
                return coin_state.Items.ToDictionary(p => p.Key, p => new SpentCoin
                {
                    Output = tx_state.Transaction.Outputs[p.Key],
                    StartHeight = tx_state.BlockIndex,
                    EndHeight = p.Value
                });
            }
            else
            {
                return new Dictionary<ushort, SpentCoin>();
            }
        }

        private ECPoint[] _validators = null;

        /// <summary>
        /// 获取当前参与共识的验证人
        /// </summary>
        /// <returns></returns>
        public ECPoint[] GetValidators()
        {
            if (_validators == null)
            {
                _validators = GetValidators(Enumerable.Empty<Transaction>()).ToArray();
            }
            return _validators;
        }

        /// <summary>
        /// 获取参与共识的验证人列表
        /// </summary>
        /// <param name="others">打包的交易</param>
        /// <returns></returns>
        public IEnumerable<ECPoint> GetValidators(IEnumerable<Transaction> others)
        {
            Snapshot snapshot = Clone();
            foreach (Transaction tx in others)
            {
                foreach (TransactionOutput output in tx.Outputs)
                {
                    AccountState account = snapshot.Accounts.GetAndChange(output.ScriptHash, () => new AccountState(output.ScriptHash));
                    if (account.Balances.ContainsKey(output.AssetId))
                        account.Balances[output.AssetId] += output.Value;
                    else
                        account.Balances[output.AssetId] = output.Value;
                    if (output.AssetId.Equals(Blockchain.GoverningToken.Hash) && account.Votes.Length > 0)
                    {
                        foreach (ECPoint pubkey in account.Votes)
                            snapshot.Validators.GetAndChange(pubkey, () => new ValidatorState(pubkey)).Votes += output.Value;
                        snapshot.ValidatorsCount.GetAndChange().Votes[account.Votes.Length - 1] += output.Value;
                    }
                }
                foreach (var group in tx.Inputs.GroupBy(p => p.PrevHash))
                {
                    Transaction tx_prev = snapshot.GetTransaction(group.Key);
                    foreach (CoinReference input in group)
                    {
                        TransactionOutput out_prev = tx_prev.Outputs[input.PrevIndex];
                        AccountState account = snapshot.Accounts.GetAndChange(out_prev.ScriptHash);
                        if (out_prev.AssetId.Equals(Blockchain.GoverningToken.Hash))
                        {
                            if (account.Votes.Length > 0)
                            {
                                foreach (ECPoint pubkey in account.Votes)
                                {
                                    ValidatorState validator = snapshot.Validators.GetAndChange(pubkey);
                                    validator.Votes -= out_prev.Value;
                                    if (!validator.Registered && validator.Votes.Equals(Fixed8.Zero))
                                        snapshot.Validators.Delete(pubkey);
                                }
                                snapshot.ValidatorsCount.GetAndChange().Votes[account.Votes.Length - 1] -= out_prev.Value;
                            }
                        }
                        account.Balances[out_prev.AssetId] -= out_prev.Value;
                    }
                }
                switch (tx)
                {
#pragma warning disable CS0612
                    case EnrollmentTransaction tx_enrollment:
                        snapshot.Validators.GetAndChange(tx_enrollment.PublicKey, () => new ValidatorState(tx_enrollment.PublicKey)).Registered = true;
                        break;
#pragma warning restore CS0612
                    case StateTransaction tx_state:
                        foreach (StateDescriptor descriptor in tx_state.Descriptors)
                            switch (descriptor.Type)
                            {
                                case StateType.Account:
                                    Blockchain.ProcessAccountStateDescriptor(descriptor, snapshot);
                                    break;
                                case StateType.Validator:
                                    Blockchain.ProcessValidatorStateDescriptor(descriptor, snapshot);
                                    break;
                            }
                        break;
                }
            }
            int count = (int)snapshot.ValidatorsCount.Get().Votes.Select((p, i) => new
            {
                Count = i,
                Votes = p
            }).Where(p => p.Votes > Fixed8.Zero).ToArray().WeightedFilter(0.25, 0.75, p => p.Votes.GetData(), (p, w) => new
            {
                p.Count,
                Weight = w
            }).WeightedAverage(p => p.Count, p => p.Weight);
            count = Math.Max(count, Blockchain.StandbyValidators.Length);
            HashSet<ECPoint> sv = new HashSet<ECPoint>(Blockchain.StandbyValidators);
            ECPoint[] pubkeys = snapshot.Validators.Find().Select(p => p.Value).Where(p => (p.Registered && p.Votes > Fixed8.Zero) || sv.Contains(p.PublicKey)).OrderByDescending(p => p.Votes).ThenBy(p => p.PublicKey).Select(p => p.PublicKey).Take(count).ToArray();
            IEnumerable<ECPoint> result;
            if (pubkeys.Length == count)
            {
                result = pubkeys;
            }
            else
            {
                HashSet<ECPoint> hashSet = new HashSet<ECPoint>(pubkeys);
                for (int i = 0; i < Blockchain.StandbyValidators.Length && hashSet.Count < count; i++)
                    hashSet.Add(Blockchain.StandbyValidators[i]);
                result = hashSet;
            }
            return result.OrderBy(p => p);
        }
    }
}
