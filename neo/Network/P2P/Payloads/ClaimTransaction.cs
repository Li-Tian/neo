using Neo.IO;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// Claim交易，用于发起提取GAS交易
    /// </summary>
    public class ClaimTransaction : Transaction
    {
        /// <summary>
        /// 已经花费的GAS outputs
        /// </summary>
        public CoinReference[] Claims;

        /// <summary>
        /// 网络费用，默认0
        /// </summary>
        public override Fixed8 NetworkFee => Fixed8.Zero;
        
        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Claims.GetVarSize();

        /// <summary>
        /// 创建Claim交易
        /// </summary>
        public ClaimTransaction()
            : base(TransactionType.ClaimTransaction)
        {
        }

        /// <summary>
        /// 反序列化，读取claims数据，其他数据未提取
        /// </summary>
        /// <param name="reader"></param>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
            Claims = reader.ReadSerializableArray<CoinReference>();
            if (Claims.Length == 0) throw new FormatException();
        }

        /// <summary>
        /// 获取待验证脚本hash
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <returns>验证脚本hash集合，包括output指向的收款人地址</returns>
        /// <exception cref="System.InvalidOperationException">若引用的output不存在时，抛出该异常</exception>
        public override UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            HashSet<UInt160> hashes = new HashSet<UInt160>(base.GetScriptHashesForVerifying(snapshot));
            foreach (var group in Claims.GroupBy(p => p.PrevHash))
            {
                Transaction tx = snapshot.GetTransaction(group.Key);
                if (tx == null) throw new InvalidOperationException();
                foreach (CoinReference claim in group)
                {
                    if (tx.Outputs.Length <= claim.PrevIndex) throw new InvalidOperationException();
                    hashes.Add(tx.Outputs[claim.PrevIndex].ScriptHash);
                }
            }
            return hashes.OrderBy(p => p).ToArray();
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>Claims</term>
        /// <description>已经花费的GAS outputs</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Claims);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["claims"] = new JArray(Claims.Select(p => p.ToJson()).ToArray());
            return json;
        }

        /// <summary>
        /// 验证交易
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <param name="mempool">已经花费的GAS outputs</param>
        /// <returns>1. 若Claims包含重复交易时，返回false； 
        /// 2. 若Claims与内存池交易存在重复时，返回false； 
        /// 3. 若GAS的资产没有变动或者减少时，返回false；
        /// 4. 若计算的claim到的Gas不等于GAS的增发量时，返回false
        /// 5. 返回true</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            if (!base.Verify(snapshot, mempool)) return false;
            if (Claims.Length != Claims.Distinct().Count())
                return false;
            if (mempool.OfType<ClaimTransaction>().Where(p => p != this).SelectMany(p => p.Claims).Intersect(Claims).Count() > 0)
                return false;
            TransactionResult result = GetTransactionResults().FirstOrDefault(p => p.AssetId == Blockchain.UtilityToken.Hash);
            if (result == null || result.Amount > Fixed8.Zero) return false;
            try
            {
                return snapshot.CalculateBonus(Claims, false) == -result.Amount;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
