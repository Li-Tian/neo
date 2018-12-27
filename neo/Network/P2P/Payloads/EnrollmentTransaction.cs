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
    // 注册验证人【已弃用，请使用StateTransaction】
    // </summary>
    /// <summary>
    /// Registered Verifier【Abandoned, please use StateTransaction】
    /// </summary>
    [Obsolete]
    public class EnrollmentTransaction : Transaction
    {
        // <summary>
        // 申请人公钥地址
        // </summary>
        /// <summary>
        /// Applicant public key
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

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// Size
        /// </summary>
        public override int Size => base.Size + PublicKey.Size;


        // <summary>
        // 构造函数：创建注册验证人交易
        // </summary>
        /// <summary>
        /// Constructor：create a EnrollmentTransaction object
        /// </summary>
        public EnrollmentTransaction()
            : base(TransactionType.EnrollmentTransaction)
        {
        }

        // <summary>
        // 反序列化，读取公钥地址
        // </summary>
        // <param name="reader">二进制输入流</param>
        // <exception cref="FormatException">如果交易版本号不等于0</exception>
        /// <summary>
        /// Deserialize method，read publickey from binary reader
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <exception cref="FormatException">the transaction version number is not 0</exception>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
            PublicKey = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
        }

        // <summary>
        // 获取需要签名的交易的hash。包括交易输入的地址和申请人的公钥地址。
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <returns>包括交易输入的地址和申请人的公钥地址。</returns>
        /// <summary>
        /// Get the hash of the transaction that needs to be signed.
        /// This includes transaction input address and the applicant's public key.
        /// </summary>
        /// <param name="snapshot">snapshot</param>
        /// <returns>This includes transaction input address and the applicant's public key.</returns>
        public override UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            return base.GetScriptHashesForVerifying(snapshot).Union(new UInt160[] { ScriptHash }).OrderBy(p => p).ToArray();
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>PublicKey</term>
        // <description>申请人公钥地址</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// Serialize
        /// <list type="bullet">
        /// <item>
        /// <term>PublicKey</term>
        /// <description>applicant's public key</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(PublicKey);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>
        /// <summary>
        /// Convert to JObject object
        /// </summary>
        /// <returns>JObject object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["pubkey"] = PublicKey.ToString();
            return json;
        }

        // <summary>
        // 校验该交易。已弃用该交易。拒绝新的交易。所以固定返回false
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <param name="mempool">内存池交易</param>
        // <returns>返回false，已弃用该交易。拒绝新的交易。</returns>
        /// <summary>
        /// Verify the transaction. This class has been deprecated.Return false by default.
        /// </summary>
        /// <param name="snapshot">database snapshot</param>
        /// <param name="mempool">mempool</param>
        /// <returns>return false by default.</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            return false;
        }
    }
}
