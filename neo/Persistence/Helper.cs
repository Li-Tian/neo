using Neo.Cryptography.ECC;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Persistence
{
    // <summary>
    // Helper类
    // </summary>
    /// <summary>
    /// Helper class
    /// </summary>
    public static class Helper
    {
        // <summary>
        // 是否包含某个区块hash
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">待查询区块hash</param>
        // <returns>存在则返回true.不存在则返回false</returns>
        /// <summary>
        /// Determine whether it contains specified block hash
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">specified block hash</param>
        /// <returns>If it contains,return true.Otherwise,return false</returns>
        public static bool ContainsBlock(this IPersistence persistence, UInt256 hash)
        {
            BlockState state = persistence.Blocks.TryGet(hash);
            if (state == null) return false;
            return state.TrimmedBlock.IsBlock;
        }

        // <summary>
        // 查询是否包含某个交易hash
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">交易hash</param>
        // <returns>存在则返回true,不存在返回false</returns>
        /// <summary>
        /// Determine whether it contains specified transcation hash
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">specified transcation hash</param>
        /// <returns>If it contains,return true.Otherwise,return false</returns>
        public static bool ContainsTransaction(this IPersistence persistence, UInt256 hash)
        {
            TransactionState state = persistence.Transactions.TryGet(hash);
            return state != null;
        }

        // <summary>
        // 获取某个区块
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="index">区块高度</param>
        // <returns>返回指定高度对应的区块，若不存在则返回null</returns>
        /// <summary>
        /// get specified block
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="index">block height</param>
        /// <returns>Returns the block corresponding to the specified height, or null if it does not exist</returns>
        public static Block GetBlock(this IPersistence persistence, uint index)
        {
            UInt256 hash = Blockchain.Singleton.GetBlockHash(index);
            if (hash == null) return null;
            return persistence.GetBlock(hash);
        }

        // <summary>
        // 获取某个区块
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">区块hash</param>
        // <returns>返回指定区块哈希对应的区块，若不存在则返回null</returns>
        /// <summary>
        /// get specified block
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">specified block hash</param>
        /// <returns>Returns the block corresponding to the specified block hash , 
        /// or null if it does not exist</returns>
        public static Block GetBlock(this IPersistence persistence, UInt256 hash)
        {
            BlockState state = persistence.Blocks.TryGet(hash);
            if (state == null) return null;
            if (!state.TrimmedBlock.IsBlock) return null;
            return state.TrimmedBlock.GetBlock(persistence.Transactions);
        }

        // <summary>
        // 获取验证人候选人列表。包括已经登记为验证候选人的列表和备用验证人列表。
        // 不包含是否当选为验证人的信息。
        // </summary>
        // <param name="persistence">持久化器</param>
        // <returns>验证人列表</returns>
        /// <summary>
        /// get a validator candidate list。
        /// Includes a list of verified candidates who had registered and a list of stand validator.
        /// Does not include information on whether or not to be elected as a validator
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <returns>ValidatorState collection</returns>
        public static IEnumerable<ValidatorState> GetEnrollments(this IPersistence persistence)
        {
            HashSet<ECPoint> sv = new HashSet<ECPoint>(Blockchain.StandbyValidators);
            return persistence.Validators.Find().Select(p => p.Value).Where(p => p.Registered || sv.Contains(p.PublicKey));
        }

        // <summary>
        // 获取某个高度的区块头
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="index">区块头高度</param>
        // <returns>指定高度的区块的区块头，如果该高度区块不存在，则返回 null </returns>
        /// <summary>
        /// Get the block header of a specified height
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="index">specified height of block header</param>
        /// <returns>block header of a specified height.Return null if a specified height block does not exist </returns>
        public static Header GetHeader(this IPersistence persistence, uint index)
        {
            UInt256 hash = Blockchain.Singleton.GetBlockHash(index);
            if (hash == null) return null;
            return persistence.GetHeader(hash);
        }

        // <summary>
        // 获取某个区块头
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">区块头hash</param>
        // <returns>指定区块的区块头。不存在时返回null</returns>
        /// <summary>
        /// get block header
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">specified block header hash</param>
        /// <returns>specified block header.Return null if it does not exist</returns>
        public static Header GetHeader(this IPersistence persistence, UInt256 hash)
        {
            return persistence.Blocks.TryGet(hash)?.TrimmedBlock.Header;
        }

        /// <summary>
        /// 获取下一个区块的哈希
        /// </summary>
        /// <param name="persistence">持久化器</param>
        /// <param name="hash">待查询的区块hash</param>
        /// <returns>下一个区块的哈希。不存在时返回null</returns>
        /// <summary>
        /// get next block hash
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">current block hash</param>
        /// <returns>next block hash.Return null if it does not exist</returns>

        public static UInt256 GetNextBlockHash(this IPersistence persistence, UInt256 hash)
        {
            BlockState state = persistence.Blocks.TryGet(hash);
            if (state == null) return null;
            return Blockchain.Singleton.GetBlockHash(state.TrimmedBlock.Index + 1);
        }

        // <summary>
        // 查询到某个高度位置，总的系统手续费
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="height">区块高度</param>
        // <returns>总的系统手续费金额</returns>
        /// <summary>
        /// Query total system fee of specified block
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="height">specified block height</param>
        /// <returns>total system fee amount</returns>
        public static long GetSysFeeAmount(this IPersistence persistence, uint height)
        {
            return persistence.GetSysFeeAmount(Blockchain.Singleton.GetBlockHash(height));
        }

        // <summary>
        // 查询从区块0开始到指定哈希的区块位置（包含该区块）总的系统手续费
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">指定的区块hash</param>
        // <returns>总的系统手续费金额</returns>
        /// <summary>
        /// Query the total system fee from the block 0 to the specified hash location (including specified block)
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">specified block hash</param>
        /// <returns>total system fee amount</returns>
        public static long GetSysFeeAmount(this IPersistence persistence, UInt256 hash)
        {
            BlockState block_state = persistence.Blocks.TryGet(hash);
            if (block_state == null) return 0;
            return block_state.SystemFeeAmount;
        }

        // <summary>
        // 查询交易
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">交易hash</param>
        // <returns>指定哈希对应的交易</returns>
        /// <summary>
        /// query transaction
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">transaction hash</param>
        /// <returns>transaction output corresponding to transaction hash</returns>
        public static Transaction GetTransaction(this IPersistence persistence, UInt256 hash)
        {
            return persistence.Transactions.TryGet(hash)?.Transaction;
        }


        // <summary>
        // 查询未花费交易输出
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">交易hash</param>
        // <param name="index">第几个output</param>
        // <returns>指定索引的未花费交易输出，查询不到交易hash对应的未花费交易输出或未花费交易输出的输出个数小于索引或处于被花费的状态的时候返回null</returns>
        /// <summary>
        /// Query unspent transaction outputs of a transaction
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">transcation hash</param>
        /// <param name="index">transcation output index</param>
        /// <returns>a unspent transaction output of specified index.
        /// Returns null if can not query the unspent transaction outputs corresponding to transaction hash
        /// or the number of unspent transaction output is less than the index or the unspent transaction output is in the state of being spent.
        /// </returns>
        public static TransactionOutput GetUnspent(this IPersistence persistence, UInt256 hash, ushort index)
        {
            UnspentCoinState state = persistence.UnspentCoins.TryGet(hash);
            if (state == null) return null;
            if (index >= state.Items.Length) return null;
            if (state.Items[index].HasFlag(CoinState.Spent)) return null;
            return persistence.GetTransaction(hash).Outputs[index];
        }

        // <summary>
        // 查询某一笔交易的未花费交易输出
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="hash">交易Hash</param>
        // <returns>该交易所有的未花费交易输出</returns>
        /// <summary>
        /// Query  unspent transaction outputs of a transaction
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="hash">transcation hash</param>
        /// <returns>unspent transcation outputs</returns>
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

        // <summary>
        // 检测交易是否需多重支付
        // </summary>
        // <param name="persistence">持久化器</param>
        // <param name="tx">交易hash</param>
        // <returns>
        // 若交易中input所指向的每一笔output都存在，且没有被花费掉，则返回false。
        // 否则返回true。
        // </returns>
        /// <summary>
        /// Check if the transaction is a multiple payment
        /// </summary>
        /// <param name="persistence">persistence</param>
        /// <param name="tx">transaction hash</param>
        /// <returns>
        /// Returns false if each previous transcation output pointed to by current transaction input exists and is not spent.
        /// Otherwise,return true.
        /// </returns>
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
