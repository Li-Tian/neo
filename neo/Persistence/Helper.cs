using Neo.Cryptography.ECC;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Persistence
{
    public static class Helper
    {
        /// <summary>
        /// 是否包含某个区块hash
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">待查询区块hash</param>
        /// <returns></returns>
        public static bool ContainsBlock(this IPersistence persistence, UInt256 hash)
        {
            BlockState state = persistence.Blocks.TryGet(hash);
            if (state == null) return false;
            return state.TrimmedBlock.IsBlock;
        }

        /// <summary>
        /// 查询是否包含某个交易hash
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">交易hash</param>
        /// <returns></returns>
        public static bool ContainsTransaction(this IPersistence persistence, UInt256 hash)
        {
            TransactionState state = persistence.Transactions.TryGet(hash);
            return state != null;
        }

        /// <summary>
        /// 获取某个区块
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="index"区块高度></param>
        /// <returns></returns>
        public static Block GetBlock(this IPersistence persistence, uint index)
        {
            UInt256 hash = Blockchain.Singleton.GetBlockHash(index);
            if (hash == null) return null;
            return persistence.GetBlock(hash);
        }

        /// <summary>
        /// 获取某个区块
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">区块hash</param>
        /// <returns></returns>
        public static Block GetBlock(this IPersistence persistence, UInt256 hash)
        {
            BlockState state = persistence.Blocks.TryGet(hash);
            if (state == null) return null;
            if (!state.TrimmedBlock.IsBlock) return null;
            return state.TrimmedBlock.GetBlock(persistence.Transactions);
        }

        /// <summary>
        /// 获取验证人
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <returns></returns>
        public static IEnumerable<ValidatorState> GetEnrollments(this IPersistence persistence)
        {
            HashSet<ECPoint> sv = new HashSet<ECPoint>(Blockchain.StandbyValidators);
            return persistence.Validators.Find().Select(p => p.Value).Where(p => p.Registered || sv.Contains(p.PublicKey));
        }

        /// <summary>
        /// 获取某个高度的区块头
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="index">区块头高度</param>
        /// <returns></returns>
        public static Header GetHeader(this IPersistence persistence, uint index)
        {
            UInt256 hash = Blockchain.Singleton.GetBlockHash(index);
            if (hash == null) return null;
            return persistence.GetHeader(hash);
        }

        /// <summary>
        /// 获取某个区块头
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">区块头hash</param>
        /// <returns></returns>
        public static Header GetHeader(this IPersistence persistence, UInt256 hash)
        {
            return persistence.Blocks.TryGet(hash)?.TrimmedBlock.Header;
        }

        /// <summary>
        /// 获取下一个block的hash
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">待查询的区块hash</param>
        /// <returns></returns>
        public static UInt256 GetNextBlockHash(this IPersistence persistence, UInt256 hash)
        {
            BlockState state = persistence.Blocks.TryGet(hash);
            if (state == null) return null;
            return Blockchain.Singleton.GetBlockHash(state.TrimmedBlock.Index + 1);
        }

        /// <summary>
        /// 查询到某个高度位置，总的系统手续费
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="height">区块高度</param>
        /// <returns></returns>
        public static long GetSysFeeAmount(this IPersistence persistence, uint height)
        {
            return persistence.GetSysFeeAmount(Blockchain.Singleton.GetBlockHash(height));
        }

        /// <summary>
        /// 查询到某个区块位置（包含该区块），总的系统手续费
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">区块hash</param>
        /// <returns></returns>
        public static long GetSysFeeAmount(this IPersistence persistence, UInt256 hash)
        {
            BlockState block_state = persistence.Blocks.TryGet(hash);
            if (block_state == null) return 0;
            return block_state.SystemFeeAmount;
        }

        /// <summary>
        /// 查询交易
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">交易hash</param>
        /// <returns></returns>
        public static Transaction GetTransaction(this IPersistence persistence, UInt256 hash)
        {
            return persistence.Transactions.TryGet(hash)?.Transaction;
        }


        /// <summary>
        /// 查询UTXO
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">交易hash</param>
        /// <param name="index">第几个output</param>
        /// <returns></returns>
        public static TransactionOutput GetUnspent(this IPersistence persistence, UInt256 hash, ushort index)
        {
            UnspentCoinState state = persistence.UnspentCoins.TryGet(hash);
            if (state == null) return null;
            if (index >= state.Items.Length) return null;
            if (state.Items[index].HasFlag(CoinState.Spent)) return null;
            return persistence.GetTransaction(hash).Outputs[index];
        }

        /// <summary>
        /// 查询某一笔交易的UTXO
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">交易Hash</param>
        /// <returns>该交易所有的UTXO</returns>
        public static IEnumerable<TransactionOutput> GetUnspent(this IPersistence persistence, UInt256 hash)
        {
            List<TransactionOutput> outputs = new List<TransactionOutput>();
            UnspentCoinState state = persistence.UnspentCoins.TryGet(hash);
            if (state != null)
            {
                Transaction tx = persistence.GetTransaction(hash);
                for (int i = 0; i < state.Items.Length; i++)
                    if (!state.Items[i].HasFlag(CoinState.Spent))
                        outputs.Add(tx.Outputs[i]);
            }
            return outputs;
        }

        /// <summary>
        /// 检测交易是否需多重支付
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="tx">交易hash</param>
        /// <returns>若交易输入为空，返回false； 若交易输入指向的是已经花费的交易，则返回false</returns>
        public static bool IsDoubleSpend(this IPersistence persistence, Transaction tx)
        {
            if (tx.Inputs.Length == 0) return false;
            foreach (var group in tx.Inputs.GroupBy(p => p.PrevHash))
            {
                UnspentCoinState state = persistence.UnspentCoins.TryGet(group.Key);
                if (state == null) return true;
                if (group.Any(p => p.PrevIndex >= state.Items.Length || state.Items[p.PrevIndex].HasFlag(CoinState.Spent)))
                    return true;
            }
            return false;
        }
    }
}
