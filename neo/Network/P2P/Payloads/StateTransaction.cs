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
    /// <summary>
    /// 投票或申请验证人交易
    /// </summary>
    public class StateTransaction : Transaction
    {
        /// <summary>
        /// 交易描述
        /// </summary>
        public StateDescriptor[] Descriptors;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Descriptors.GetVarSize();

        /// <summary>
        /// 交易手续费
        /// </summary>
        public override Fixed8 SystemFee => Descriptors.Sum(p => p.SystemFee);

        /// <summary>
        /// 创建投票或申请验证人交易
        /// </summary>
        public StateTransaction()
            : base(TransactionType.StateTransaction)
        {
        }

        /// <summary>
        /// 反序列化非data数据
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            Descriptors = reader.ReadSerializableArray<StateDescriptor>(16);
        }

        /// <summary>
        /// 获取验证脚本hash
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <returns>若 StateDescriptor.Field = "Votes"时, 包含投票人地址地址  若 Field="Registered"时，包含申请人的地址脚本hash </returns>
        /// <exception cref="System.InvalidOperationException">若类型不对时，抛出该异常</exception>
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

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["descriptors"] = new JArray(Descriptors.Select(p => p.ToJson()));
            return json;
        }

        /// <summary>
        /// 校验交易
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <param name="mempool">内存池交易</param>
        /// <returns></returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            foreach (StateDescriptor descriptor in Descriptors)
                if (!descriptor.Verify(snapshot))
                    return false;
            return base.Verify(snapshot, mempool);
        }
    }
}
