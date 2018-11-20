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
    /// 注册验证人【已弃用，请使用StateTransactioni】
    /// </summary>
    [Obsolete]
    public class EnrollmentTransaction : Transaction
    {
        /// <summary>
        /// 申请人公钥地址
        /// </summary>
        public ECPoint PublicKey;

        private UInt160 _script_hash = null;
        internal UInt160 ScriptHash
        {
            get
            {
                if (_script_hash == null)
                {
                    _script_hash = Contract.CreateSignatureRedeemScript(PublicKey).ToScriptHash();
                }
                return _script_hash;
            }
        }

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + PublicKey.Size;


        /// <summary>
        /// 创建注册验证人交易
        /// </summary>
        public EnrollmentTransaction()
            : base(TransactionType.EnrollmentTransaction)
        {
        }

        /// <summary>
        /// 反序列化，读取公钥地址
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
            PublicKey = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
        }

        /// <summary>
        /// 获取验证脚本hash
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <returns></returns>
        public override UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            return base.GetScriptHashesForVerifying(snapshot).Union(new UInt160[] { ScriptHash }).OrderBy(p => p).ToArray();
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>PublicKey</term>
        /// <description>申请人公钥地址</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer"></param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(PublicKey);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["pubkey"] = PublicKey.ToString();
            return json;
        }

        /// <summary>
        /// 校验该交易
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <param name="mempool">内存池交易</param>
        /// <returns>返回false，已弃用该交易</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            return false;
        }
    }
}
