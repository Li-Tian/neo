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
    // <summary>
    // Claim交易，用于发起提取GAS交易
    // </summary>
    /// <summary>
    /// Claim transaction, used to claim GAS
    /// </summary>
    public class ClaimTransaction : Transaction
    {
        // <summary>
        // 已经花费的GAS outputs
        // </summary>
        /// <summary>
        /// the outputs collection of spent GAS
        /// </summary>
        public CoinReference[] Claims;

        // <summary>
        // 网络费用，默认0
        // </summary>
        /// <summary>
        /// NetworkFee，the default is 0
        /// </summary>
        public override Fixed8 NetworkFee => Fixed8.Zero;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// size for storage
        /// </summary>
        public override int Size => base.Size + Claims.GetVarSize();

        // <summary>
        // 构造函数
        // </summary>
        /// <summary>
        /// constructor
        /// </summary>
        public ClaimTransaction()
            : base(TransactionType.ClaimTransaction)
        {
        }

        // <summary>
        // 反序列化，读取claims数据，其他数据未提取
        // </summary>
        // <param name="reader">二进制输入流</param>
        // <exception cref="FormatException">当交易版本号不为0，或者Claims长度为0时，抛出异常</exception>
        /// <summary>
        /// Deserialize method. Read the claims data in binary reader, other data is not extracted
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <exception cref="FormatException">Throws an exception when the transaction version number is not 0, or the length of claim data is 0.</exception>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
            Claims = reader.ReadSerializableArray<CoinReference>();
            if (Claims.Length == 0) throw new FormatException();
        }

        // <summary>
        // 获取待验证脚本hash
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <returns>验证脚本hash列表，包括output指向的收款人地址。按照哈希值排序。</returns>
        // <exception cref="System.InvalidOperationException">若引用的output不存在时，抛出该异常</exception>
        /// <summary>
        /// get verify script hashes
        /// </summary>
        /// <param name="snapshot">database snapshot</param>
        /// <returns>verify script hashes list.Includes the payee address pointed to by output.Sort by hash value.</returns>
        /// <exception cref="System.InvalidOperationException">If the referenced output doe not exist, throw this exception</exception>
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

        // <summary>
        // 序列化，写出claims数据，其他数据未提取
        // <list type="bullet">
        // <item>
        // <term>Claims</term>
        // <description>已经花费的GAS outputs</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// Serialize method
        /// <list type="bullet">
        /// <item>
        /// <term>Claims</term>
        /// <description>the outputs of spent GAS</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Claims);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>
        /// <summary>
        /// Convert to a JObject object
        /// </summary>
        /// <returns>JObject object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["claims"] = new JArray(Claims.Select(p => p.ToJson()).ToArray());
            return json;
        }

        // <summary>
        // 验证交易
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <param name="mempool">内存池交易</param>
        // <returns>
        // 1. 进行交易的基本验证，若验证失败，则返回false <br/>
        // 2. 若Claims包含重复交易时，返回false <br/>
        // 3. 若Claims与内存池交易存在重复时，返回false <br/>
        // 4. 若此Claim交易引用一笔不存在的Output则返回false<br/>
        // 5. 若此Claim交易的输入GAS之和大于等于输出的GAS之和，返回false <br/>
        // 6. 若Claim交易引用的交易计算出来的GAS量不等于Claim交易所声明的GAS量时，返回false <br/>
        // 7. 若处理过程异常时，返回false <br/>
        // </returns>
        /// <summary>
        /// Verify transcation
        /// </summary>
        /// <param name="snapshot">database snapshot</param>
        /// <param name="mempool">transaction mempool</param>
        /// <returns>
        /// 1. Perform basic verification of the transaction, return false if the verification fails.
        /// 2. If the claims data contains duplicate transactions, return false.
        /// 3. If the claims data overlaps with transactions in mempool , return false.
        /// 4. If this claim transaction references a non-existent Output then return false<br />
        /// 5. If the sum of the input GAS of this claim transaction is greater than or equal to the sum of the output GAS, return false.
        /// 6. If the amount of GAS calculated by the claim transcation reference is not equal to the amount of GAS declared by the claim transcation, return false.
        /// 7. If the processing is abnormal, return false.
        /// </returns>
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
