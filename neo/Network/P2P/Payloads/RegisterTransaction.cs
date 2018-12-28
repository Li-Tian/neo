using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 资产登记交易【已弃用】
    // </summary>
    /// <summary>
    /// Asset registeration transaction
    /// </summary>
    [Obsolete]
    public class RegisterTransaction : Transaction
    {
        // <summary>
        // 资产类型
        // </summary>
        /// <summary>
        /// The type of asset
        /// </summary>
        public AssetType AssetType;

        // <summary>
        // 资产名字
        // </summary>
        /// <summary>
        /// The name of asset
        /// </summary>
        public string Name;

        // <summary>
        // 资产总量
        // </summary>
        /// <summary>
        /// The total amount of asset
        /// </summary>
        public Fixed8 Amount;

        // <summary>
        // 精度
        // </summary>
        /// <summary>
        /// The precision of asset
        /// </summary>
        public byte Precision;

        // <summary>
        // 所有者公钥
        // </summary>
        /// <summary>
        /// The publickey of owner
        /// </summary>
        public ECPoint Owner;

        // <summary>
        // 管理员地址脚本hash
        // </summary>
        /// <summary>
        /// The address hash of admin
        /// </summary>
        public UInt160 Admin;

        private UInt160 _script_hash = null;
        internal UInt160 OwnerScriptHash
        {
            get
            {
                if (_script_hash == null)
                {
                    _script_hash = Contract.CreateSignatureRedeemScript(Owner).ToScriptHash();
                }
                return _script_hash;
            }
        }

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size for storage
        /// </summary>
        public override int Size => base.Size + sizeof(AssetType) + Name.GetVarSize() + Amount.Size + sizeof(byte) + Owner.Size + Admin.Size;

        // <summary>
        // 系统手续费  若资产是NEO，GAS则费用为0
        // </summary>
        /// <summary>
        /// The system fee. If the asset is NEO, the gas fee is 0
        /// </summary>
        public override Fixed8 SystemFee
        {
            get
            {
                if (AssetType == AssetType.GoverningToken || AssetType == AssetType.UtilityToken)
                    return Fixed8.Zero;
                return base.SystemFee;
            }
        }

        // <summary>
        // 构造函数：创建资产登记交易
        // </summary>
        /// <summary>
        /// The constructor function, which creat the transaction registration transaction
        /// </summary>
        public RegisterTransaction()
            : base(TransactionType.RegisterTransaction)
        {
        }

        // <summary>
        // 反序列化非data数据
        // </summary>
        // <param name="reader">二进制输入流</param>
        // <exception cref="FormatException">
        // 1. 如果版本号不为0<br/>
        // 2. 如果资产不是NEO/GAS且未指定Owner
        // </exception>
        /// <summary>
        /// Deserialization of object 
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        /// <exception cref="FormatException">
        /// 1. If the version number is not 0<br/>
        /// 2.  If the asset is not NEO/GAS and not specify owner
        /// </exception>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version != 0) throw new FormatException();
            AssetType = (AssetType)reader.ReadByte();
            Name = reader.ReadVarString(1024);
            Amount = reader.ReadSerializable<Fixed8>();
            Precision = reader.ReadByte();
            Owner = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
            if (Owner.IsInfinity && AssetType != AssetType.GoverningToken && AssetType != AssetType.UtilityToken)
                throw new FormatException();
            Admin = reader.ReadSerializable<UInt160>();
        }

        // <summary>
        // 获取待验证签名的脚本hash集合
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <returns>交易的其他验证脚本 和 资产所有者地址脚本hash</returns>
        /// <summary>
        /// The hash list which is waiting for verifying
        /// </summary>
        /// <param name="snapshot">The snapshot of database</param>
        /// <returns></returns>
        public override UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            UInt160 owner = Contract.CreateSignatureRedeemScript(Owner).ToScriptHash();
            return base.GetScriptHashesForVerifying(snapshot).Union(new[] { owner }).OrderBy(p => p).ToArray();
        }

        // <summary>
        // 反序列化后的处理
        // </summary>
        // <exception cref="System.FormatException">若资产是NEO，GAS，但是hash值不对应时，抛出该异常</exception>
        /// <summary>
        /// If the asset is NEO, GAS, but the hash value is not correct, throw the exception 
        /// </summary>
        /// <exception cref="System.FormatException">若资产是NEO，GAS，但是hash值不对应时，抛出该异常</exception>
        protected override void OnDeserialized()
        {
            base.OnDeserialized();
            if (AssetType == AssetType.GoverningToken && !Hash.Equals(Blockchain.GoverningToken.Hash))
                throw new FormatException();
            if (AssetType == AssetType.UtilityToken && !Hash.Equals(Blockchain.UtilityToken.Hash))
                throw new FormatException();
        }

        /// <summary>
        /// 序列化非data数据
        /// <list type="bullet">
        /// <item>
        /// <term>AssetType</term>
        /// <description>资产类型</description>
        /// </item>
        /// <item>
        /// <term>Name</term>
        /// <description>名字</description>
        /// </item>
        /// <item>
        /// <term>Amount</term>
        /// <description>总量</description>
        /// </item> 
        /// <item>
        /// <term>Precision</term>
        /// <description>精度</description>
        /// </item>
        /// <item>
        /// <term>Owner</term>
        /// <description>所有者</description>
        /// </item>
        /// <item>
        /// <term>Admin</term>
        /// <description>管理员</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write((byte)AssetType);
            writer.WriteVarString(Name);
            writer.Write(Amount);
            writer.Write(Precision);
            writer.Write(Owner);
            writer.Write(Admin);
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
            json["asset"] = new JObject();
            json["asset"]["type"] = AssetType;
            try
            {
                json["asset"]["name"] = Name == "" ? null : JObject.Parse(Name);
            }
            catch (FormatException)
            {
                json["asset"]["name"] = Name;
            }
            json["asset"]["amount"] = Amount.ToString();
            json["asset"]["precision"] = Precision;
            json["asset"]["owner"] = Owner.ToString();
            json["asset"]["admin"] = Admin.ToAddress();
            return json;
        }

        // <summary>
        // 校验交易。已经弃用。禁止注册新的资产。
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <param name="mempool">内存池交易</param>
        // <returns>固定值false，已弃用</returns>
        /// <summary>
        /// The transaction verification which is deprecated
        /// </summary>
        /// <param name="snapshot">The snapshot of database</param>
        /// <param name="mempool">The memory pool</param>
        /// <returns>The fixed valie </returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            return false;
        }
    }
}
