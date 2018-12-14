using Neo.Ledger;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 发布资产交易
    /// </summary>
    public class IssueTransaction : Transaction
    {
        /// <summary>
        /// 系统手续费<br/>
        /// 1）若交易版本号大于等于1，手续费为0<br/>
        /// 2）若发布的资产是NEO或GAS，则手续费为0<br/>
        /// 3）否则按基本交易计算系统手续费
        /// </summary>
        public override Fixed8 SystemFee
        {
            get
            {
                if (Version >= 1) return Fixed8.Zero;
                if (Outputs.All(p => p.AssetId == Blockchain.GoverningToken.Hash || p.AssetId == Blockchain.UtilityToken.Hash))
                    return Fixed8.Zero;
                return base.SystemFee;
            }
        }

        /// <summary>
        /// 构造函数：创建发布资产交易
        /// </summary>
        public IssueTransaction()
            : base(TransactionType.IssueTransaction)
        {
        }

        /// <summary>
        /// 反序列化扩展数据
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        /// <exception cref="System.FormatException">若交易版本号大于1，则抛出该异常</exception>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version > 1) throw new FormatException();
        }

        /// <summary>
        /// 获取待验证签名的脚本hash
        /// </summary>
        /// <param name="snapshot">快照</param>
        /// <returns>交易本身的验证脚本，以及发行者的地址脚本hash</returns>
        /// <exception cref="System.InvalidOperationException">若发行的资产不存在，则抛出该异常</exception>
        public override UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            HashSet<UInt160> hashes = new HashSet<UInt160>(base.GetScriptHashesForVerifying(snapshot));
            foreach (TransactionResult result in GetTransactionResults().Where(p => p.Amount < Fixed8.Zero))
            {
                AssetState asset = snapshot.Assets.TryGet(result.AssetId);
                if (asset == null) throw new InvalidOperationException();
                hashes.Add(asset.Issuer);
            }
            return hashes.OrderBy(p => p).ToArray();
        }

        /// <summary>
        /// 校验交易
        /// </summary>
        /// <param name="snapshot">数据库快照</param>
        /// <param name="mempool">内存池交易</param>
        /// <returns>
        /// 1. 进行交易的基本验证，若验证失败，则返回false <br/>
        /// 2. 交易引用的input 不存在时返回false<br/>
        /// 3. 若发行的资产不存在返回false <br/>
        /// 4. 若发行的量为负数时返回false<br/>
        /// 5. 若该交易的发行量加上内存池其他发行量，超过了发行总量，则返回false</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            if (!base.Verify(snapshot, mempool)) return false;
            TransactionResult[] results = GetTransactionResults()?.Where(p => p.Amount < Fixed8.Zero).ToArray();
            if (results == null) return false;
            foreach (TransactionResult r in results)
            {
                AssetState asset = snapshot.Assets.TryGet(r.AssetId);
                if (asset == null) return false;
                if (asset.Amount < Fixed8.Zero) continue;
                Fixed8 quantity_issued = asset.Available + mempool.OfType<IssueTransaction>().Where(p => p != this).SelectMany(p => p.Outputs).Where(p => p.AssetId == r.AssetId).Sum(p => p.Value);
                if (asset.Amount - quantity_issued < -r.Amount) return false;
            }
            return true;
        }
    }
}
