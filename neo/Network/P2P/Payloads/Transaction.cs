using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Caching;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 交易
    /// </summary>
    public abstract class Transaction : IEquatable<Transaction>, IInventory
    {
        /// <summary>
        /// 交易最大存储大小
        /// </summary>
        public const int MaxTransactionSize = 102400;
        /// <summary>
        /// Maximum number of attributes that can be contained within a transaction
        /// </summary>
        private const int MaxTransactionAttributes = 16;

        /// <summary>
        /// Reflection cache for TransactionType
        /// </summary>
        private static ReflectionCache<byte> ReflectionCache = ReflectionCache<byte>.CreateFromEnum<TransactionType>();

        /// <summary>
        /// 交易类型
        /// </summary>
        public readonly TransactionType Type;

        /// <summary>
        /// 交易版本号
        /// </summary>
        public byte Version;

        /// <summary>
        /// 交易属性
        /// </summary>
        public TransactionAttribute[] Attributes;

        /// <summary>
        /// 交易输入
        /// </summary>
        public CoinReference[] Inputs;

        /// <summary>
        /// 交易输出
        /// </summary>
        public TransactionOutput[] Outputs;

        /// <summary>
        /// 见证人
        /// </summary>
        public Witness[] Witnesses { get; set; }

        private UInt256 _hash = null;

        /// <summary>
        /// 交易hash
        /// </summary>
        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = new UInt256(Crypto.Default.Hash256(this.GetHashData()));
                }
                return _hash;
            }
        }

        InventoryType IInventory.InventoryType => InventoryType.TX;

        /// <summary>
        /// 是否是低优先级交易   若是claim交易或网络费用低于某阈值时，则为低优先级交易
        /// </summary>
        public bool IsLowPriority => Type == TransactionType.ClaimTransaction || NetworkFee < Settings.Default.LowPriorityThreshold;

        private Fixed8 _network_fee = -Fixed8.Satoshi;

        /// <summary>
        /// 网络手续费
        /// </summary>
        public virtual Fixed8 NetworkFee
        {
            get
            {
                if (_network_fee == -Fixed8.Satoshi)
                {
                    Fixed8 input = References.Values.Where(p => p.AssetId.Equals(Blockchain.UtilityToken.Hash)).Sum(p => p.Value);
                    Fixed8 output = Outputs.Where(p => p.AssetId.Equals(Blockchain.UtilityToken.Hash)).Sum(p => p.Value);
                    _network_fee = input - output - SystemFee;
                }
                return _network_fee;
            }
        }

        private IReadOnlyDictionary<CoinReference, TransactionOutput> _references;

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyDictionary<CoinReference, TransactionOutput> References
        {
            get
            {
                if (_references == null)
                {
                    Dictionary<CoinReference, TransactionOutput> dictionary = new Dictionary<CoinReference, TransactionOutput>();
                    foreach (var group in Inputs.GroupBy(p => p.PrevHash))
                    {
                        Transaction tx = Blockchain.Singleton.Store.GetTransaction(group.Key);
                        if (tx == null) return null;
                        foreach (var reference in group.Select(p => new
                        {
                            Input = p,
                            Output = tx.Outputs[p.PrevIndex]
                        }))
                        {
                            dictionary.Add(reference.Input, reference.Output);
                        }
                    }
                    _references = dictionary;
                }
                return _references;
            }
        }

        /// <summary>
        /// 存储大小
        /// </summary>
        public virtual int Size => sizeof(TransactionType) + sizeof(byte) + Attributes.GetVarSize() + Inputs.GetVarSize() + Outputs.GetVarSize() + Witnesses.GetVarSize();

        /// <summary>
        /// 手续费
        /// </summary>
        public virtual Fixed8 SystemFee => Settings.Default.SystemFee.TryGetValue(Type, out Fixed8 fee) ? fee : Fixed8.Zero;

        /// <summary>
        /// 创建交易
        /// </summary>
        /// <param name="type">交易类型</param>
        protected Transaction(TransactionType type)
        {
            this.Type = type;
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            ((IVerifiable)this).DeserializeUnsigned(reader);
            Witnesses = reader.ReadSerializableArray<Witness>();
            OnDeserialized();
        }

        /// <summary>
        /// 反序列化非data数据
        /// </summary>
        /// <param name="reader"></param>
        protected virtual void DeserializeExclusiveData(BinaryReader reader)
        {
        }

        /// <summary>
        /// 从给定的byte数组中反序列化
        /// </summary>
        /// <param name="value">原数据</param>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public static Transaction DeserializeFrom(byte[] value, int offset = 0)
        {
            using (MemoryStream ms = new MemoryStream(value, offset, value.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                return DeserializeFrom(reader);
            }
        }

        internal static Transaction DeserializeFrom(BinaryReader reader)
        {
            // Looking for type in reflection cache
            Transaction transaction = ReflectionCache.CreateInstance<Transaction>(reader.ReadByte());
            if (transaction == null) throw new FormatException();

            transaction.DeserializeUnsignedWithoutType(reader);
            transaction.Witnesses = reader.ReadSerializableArray<Witness>();
            transaction.OnDeserialized();
            return transaction;
        }

        void IVerifiable.DeserializeUnsigned(BinaryReader reader)
        {
            if ((TransactionType)reader.ReadByte() != Type)
                throw new FormatException();
            DeserializeUnsignedWithoutType(reader);
        }

        private void DeserializeUnsignedWithoutType(BinaryReader reader)
        {
            Version = reader.ReadByte();
            DeserializeExclusiveData(reader);
            Attributes = reader.ReadSerializableArray<TransactionAttribute>(MaxTransactionAttributes);
            Inputs = reader.ReadSerializableArray<CoinReference>();
            Outputs = reader.ReadSerializableArray<TransactionOutput>(ushort.MaxValue + 1);
        }

        /// <summary>
        /// 判断两笔交易是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Transaction other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Hash.Equals(other.Hash);
        }

        /// <summary>
        /// 判断交易是否等于该对象
        /// </summary>
        /// <param name="obj">待比较对象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Transaction);
        }


        /// <summary>
        /// 获取hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        byte[] IScriptContainer.GetMessage()
        {
            return this.GetHashData();
        }

        /// <summary>
        /// 获取验证脚本hash
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <returns>包含：1. 交易输入所指向的收款人地址脚本hash，2. 交易属性为script时，包含该Data， 3. 若资产类型包含AssetType.DutyFlag时，包含收款人地址脚本hash</returns>
        /// <exception cref="System.InvalidOperationException">若输入为空或者资产不存在时，抛出该异常</exception>
        public virtual UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            if (References == null) throw new InvalidOperationException();
            HashSet<UInt160> hashes = new HashSet<UInt160>(Inputs.Select(p => References[p].ScriptHash));
            hashes.UnionWith(Attributes.Where(p => p.Usage == TransactionAttributeUsage.Script).Select(p => new UInt160(p.Data)));
            foreach (var group in Outputs.GroupBy(p => p.AssetId))
            {
                AssetState asset = snapshot.Assets.TryGet(group.Key);
                if (asset == null) throw new InvalidOperationException();
                if (asset.AssetType.HasFlag(AssetType.DutyFlag))
                {
                    hashes.UnionWith(group.Select(p => p.ScriptHash));
                }
            }
            return hashes.OrderBy(p => p).ToArray();
        }

        /// <summary>
        /// 获取交易资产变化
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TransactionResult> GetTransactionResults()
        {
            if (References == null) return null;
            return References.Values.Select(p => new
            {
                p.AssetId,
                p.Value
            }).Concat(Outputs.Select(p => new
            {
                p.AssetId,
                Value = -p.Value
            })).GroupBy(p => p.AssetId, (k, g) => new TransactionResult
            {
                AssetId = k,
                Amount = g.Sum(p => p.Value)
            }).Where(p => p.Amount != Fixed8.Zero);
        }

        protected virtual void OnDeserialized()
        {
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            ((IVerifiable)this).SerializeUnsigned(writer);
            writer.Write(Witnesses);
        }

        protected virtual void SerializeExclusiveData(BinaryWriter writer)
        {
        }

        void IVerifiable.SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(Version);
            SerializeExclusiveData(writer);
            writer.Write(Attributes);
            writer.Write(Inputs);
            writer.Write(Outputs);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public virtual JObject ToJson()
        {
            JObject json = new JObject();
            json["txid"] = Hash.ToString();
            json["size"] = Size;
            json["type"] = Type;
            json["version"] = Version;
            json["attributes"] = Attributes.Select(p => p.ToJson()).ToArray();
            json["vin"] = Inputs.Select(p => p.ToJson()).ToArray();
            json["vout"] = Outputs.Select((p, i) => p.ToJson((ushort)i)).ToArray();
            json["sys_fee"] = SystemFee.ToString();
            json["net_fee"] = NetworkFee.ToString();
            json["scripts"] = Witnesses.Select(p => p.ToJson()).ToArray();
            return json;
        }

        bool IInventory.Verify(Snapshot snapshot)
        {
            return Verify(snapshot, Enumerable.Empty<Transaction>());
        }

        /// <summary>
        /// 校验交易
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <param name="mempool">内存池交易</param>
        /// <returns>1. 若Input存在重复，则返回false <br/>
        /// 2.若内存池交易包含Input交易时，返回false <br/>
        /// 3.若Input是已经花费的交易，则返回false <br/>
        /// 4. 若转账资产不存在，则返回false  <br/>
        /// 5. 若资产是非NEO或非GAS时，且资产过期时，返回false  <br/>
        /// 6. 若转账金额不能整除对应资产的最小精度时，返回false  <br/>
        /// 7. 检查金额关系：  <br/>
        ///    7.1 若 Input.Asset = Output.Asset 时，返回false  <br/>
        ///    7.2 若 Input.Asset > Output.Asset 时，且资金种类大于一种时，返回false  <br/>
        ///    7.3 若 Input.Asset > Output.Asset 时，资金种类不是GAS时，返回false  <br/>
        ///    7.4 若 交易手续费 大于 Input.GAS - output.GAS 时， 返回false  <br/>
        ///    7.5 若 Input.Asset &lt Output.Asset 时：
        ///        7.5.1 若 交易类型是MinerTransaction 或 ClaimTransaction， 且 资产不是GAS时，返回false <br/>
        ///        7.5.2 若交易类型时 IssueTransaction时，且 资产是GAS时，返回false  <br/>
        ///        7.5.3 若是其他交易类型，且 存在增发资产时，返回false  <br/>
        /// 8. 若交易属性，包含类型是 TransactionAttributeUsage.ECDH02 或 TransactionAttributeUsage.ECDH03 时，返回false
        /// 9. 若 VerifyReceivingScripts 验证返回false时（VerificationR触发器验证），返回false。(目前，VerifyReceivingScripts 返回永正）<br/>
        /// 10.若 VerifyWitnesses 验证返回false时（对验证脚本进行验证），则返回false 
        /// </returns>
        public virtual bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            if (Size > MaxTransactionSize) return false;
            for (int i = 1; i < Inputs.Length; i++)
                for (int j = 0; j < i; j++)
                    if (Inputs[i].PrevHash == Inputs[j].PrevHash && Inputs[i].PrevIndex == Inputs[j].PrevIndex)
                        return false;
            if (mempool.Where(p => p != this).SelectMany(p => p.Inputs).Intersect(Inputs).Count() > 0)
                return false;
            if (snapshot.IsDoubleSpend(this))
                return false;
            foreach (var group in Outputs.GroupBy(p => p.AssetId))
            {
                AssetState asset = snapshot.Assets.TryGet(group.Key);
                if (asset == null) return false;
                if (asset.Expiration <= snapshot.Height + 1 && asset.AssetType != AssetType.GoverningToken && asset.AssetType != AssetType.UtilityToken)
                    return false;
                foreach (TransactionOutput output in group)
                    if (output.Value.GetData() % (long)Math.Pow(10, 8 - asset.Precision) != 0)
                        return false;
            }
            TransactionResult[] results = GetTransactionResults()?.ToArray();
            if (results == null) return false;
            TransactionResult[] results_destroy = results.Where(p => p.Amount > Fixed8.Zero).ToArray();
            if (results_destroy.Length > 1) return false;
            if (results_destroy.Length == 1 && results_destroy[0].AssetId != Blockchain.UtilityToken.Hash)
                return false;
            if (SystemFee > Fixed8.Zero && (results_destroy.Length == 0 || results_destroy[0].Amount < SystemFee))
                return false;
            TransactionResult[] results_issue = results.Where(p => p.Amount < Fixed8.Zero).ToArray();
            switch (Type)
            {
                case TransactionType.MinerTransaction:
                case TransactionType.ClaimTransaction:
                    if (results_issue.Any(p => p.AssetId != Blockchain.UtilityToken.Hash))
                        return false;
                    break;
                case TransactionType.IssueTransaction:
                    if (results_issue.Any(p => p.AssetId == Blockchain.UtilityToken.Hash))
                        return false;
                    break;
                default:
                    if (results_issue.Length > 0)
                        return false;
                    break;
            }
            if (Attributes.Count(p => p.Usage == TransactionAttributeUsage.ECDH02 || p.Usage == TransactionAttributeUsage.ECDH03) > 1)
                return false;
            if (!VerifyReceivingScripts()) return false;
            return this.VerifyWitnesses(snapshot);
        }

        private bool VerifyReceivingScripts()
        {
            //TODO: run ApplicationEngine
            //foreach (UInt160 hash in Outputs.Select(p => p.ScriptHash).Distinct())
            //{
            //    ContractState contract = Blockchain.Default.GetContract(hash);
            //    if (contract == null) continue;
            //    if (!contract.Payable) return false;
            //    using (StateReader service = new StateReader())
            //    {
            //        ApplicationEngine engine = new ApplicationEngine(TriggerType.VerificationR, this, Blockchain.Default, service, Fixed8.Zero);
            //        engine.LoadScript(contract.Script, false);
            //        using (ScriptBuilder sb = new ScriptBuilder())
            //        {
            //            sb.EmitPush(0);
            //            sb.Emit(OpCode.PACK);
            //            sb.EmitPush("receiving");
            //            engine.LoadScript(sb.ToArray(), false);
            //        }
            //        if (!engine.Execute()) return false;
            //        if (engine.EvaluationStack.Count != 1 || !engine.EvaluationStack.Pop().GetBoolean()) return false;
            //    }
            //}
            return true;
        }
    }
}
