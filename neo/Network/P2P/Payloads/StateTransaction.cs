using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Persistence;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 投票或申请验证人交易
    // </summary>
    /// <summary>
    /// The Transaction for voting or application for validators
    /// </summary>
    public class StateTransaction : Transaction
    {
        // <summary>
        // 交易描述
        // </summary>
        /// <summary>
        /// The descriptor for transactions
        /// </summary>
        public StateDescriptor[] Descriptors;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The storage of size
        /// </summary>
        public override int Size => base.Size + Descriptors.GetVarSize();

        // <summary>
        // 交易手续费
        // </summary>
        // <summary>
        // The tramsactopm system fee
        /// </summary>
        public override Fixed8 SystemFee => Descriptors.Sum(p => p.SystemFee);

        // <summary>
        // 创建投票或申请验证人交易
        // </summary>
        /// <summary>
        /// Constructor of transaction for voting and validator application
        /// </summary>
        public StateTransaction()
            : base(TransactionType.StateTransaction)
        {
        }

        // <summary>
        // 反序列化非data数据
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization of transaction exclude the data
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            Descriptors = reader.ReadSerializableArray<StateDescriptor>(16);
        }

        // <summary>
        // 获取验证脚本hash
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <returns>
        // 若 StateDescriptor.Field = "Votes"时, 包含投票人地址地址<br/>
        // 若 Field="Registered"时，包含申请人的地址脚本hash
        // </returns>
        // <exception cref="System.InvalidOperationException">若类型不对时，抛出该异常</exception>
        /// <summary>
        /// Get the verification transript hash
        /// </summary>
        /// <param name="snapshot">The snapshot for database</param>
        /// <returns>
        /// If the stateDescriptor Field is Votes, then it includes the address of the votes <br/>
        /// If the Field is "Registered", it includes the address script hash of applicant
        /// </returns>
        public override UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            HashSet<UInt160> hashes = new HashSet<UInt160>(base.GetScriptHashesForVerifying(snapshot));
            foreach (StateDescriptor descriptor in Descriptors)
            {
                switch (descriptor.Type)
                {
                    case StateType.Account:
                        hashes.UnionWith(GetScriptHashesForVerifying_Account(descriptor));
                        break;
                    case StateType.Validator:
                        hashes.UnionWith(GetScriptHashesForVerifying_Validator(descriptor));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return hashes.OrderBy(p => p).ToArray();
        }

        private IEnumerable<UInt160> GetScriptHashesForVerifying_Account(StateDescriptor descriptor)
        {
            switch (descriptor.Field)
            {
                case "Votes":
                    yield return new UInt160(descriptor.Key);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private IEnumerable<UInt160> GetScriptHashesForVerifying_Validator(StateDescriptor descriptor)
        {
            switch (descriptor.Field)
            {
                case "Registered":
                    yield return Contract.CreateSignatureRedeemScript(ECPoint.DecodePoint(descriptor.Key, ECCurve.Secp256r1)).ToScriptHash();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 序列化非data数据
        /// <list type="bullet">
        /// <item>
        /// <term>Descriptors</term>
        /// <desciption>交易描述</desciption>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Descriptors);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>
        /// <summary>
        /// Transfer to json object
        /// </summary>
        /// <returns>Json object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["descriptors"] = new JArray(Descriptors.Select(p => p.ToJson()));
            return json;
        }

        // <summary>
        // 校验交易
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <param name="mempool">内存池交易</param>
        // <returns>
        // 1. 对每个StateDescriptor进行验证 <br/>
        //     1.1 若 descriptor.Type 是 StateType.Validator 时, 若 descriptor.Field 不等于`Registered`时，返回false <br/>
        //     1.2 若 descriptor.Type 是 StateType.Account 时 <br/>
        //         1.2.1 若投票账户持有的NEO数量为0，或者投票账户冻结时，返回false <br/>
        //         1.2.2 若被投账户在备用共识节点列表或尚未申请为验证人时，返回false  <br/>
        // 2. 进行交易的基本验证，若验证失败，则返回false <br/>
        // </returns>
        /// <summary>
        /// The transaction verification
        /// </summary>
        /// <param name="snapshot">The snapshot of database</param>
        /// <param name="mempool">memory pool</param>
        /// <returns>
        // 1. Verify each stateDescriptor <br/>
        ///     1.1 If the descriptor.Type is StateType.Validator and if descriptor.Field is not equal to Registered, return false <br/>
        ///     1.2 若 descriptor.Type 是 StateType.Account 时 <br/>
        ///     1.2 if the descriptor.Type is StateType.Account:
        ///         1.2.1 If NEO hold by the the voting accuntis 0, or when the voting acount is frozen, return false. <br/>
        ///         1.2.2 If the voted account is in the backup list or is not registered as validators, return false.  <br/>
        /// 2. The basic transaction verification. If verified failed, return false<br/> 
        //// </returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            foreach (StateDescriptor descriptor in Descriptors)
                if (!descriptor.Verify(snapshot))
                    return false;
            return base.Verify(snapshot, mempool);
        }
    }
}
