using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    // <summary>
    // 资产状态
    // </summary>
    /// <summary>
    /// The state of asset
    /// </summary>
    public class AssetState : StateBase, ICloneable<AssetState>
    {

        // <summary>
        // 资产Id
        // </summary>
        /// <summary>
        /// The asset Id
        /// </summary>
        public UInt256 AssetId;


        // <summary>
        // 资产类型
        // </summary>
        /// <summary>
        /// The asset type
        /// </summary>
        public AssetType AssetType;

        // <summary>
        // 资产名称
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
        // 资产可用额度
        // </summary>
        /// <summary>
        /// Available asset amount
        /// </summary>
        public Fixed8 Available;

        // <summary>
        // 精度
        // </summary>
        /// <summary>
        /// Precision of this asset
        /// </summary>
        public byte Precision;

        // <summary>
        // 收费模式
        // </summary>
        /// <summary>
        /// The Fee mode of this asset
        /// </summary>
        public const byte FeeMode = 0;

        // <summary>
        // 费用
        // </summary>
        /// <summary>
        /// Fee
        /// </summary>
        public Fixed8 Fee;

        // <summary>
        // 收费地址
        // </summary>
        /// <summary>
        /// The address fee
        /// </summary>
        public UInt160 FeeAddress;

        // <summary>
        // 所有者地址
        // </summary>
        /// <summary>
        /// The owner of this asset
        /// </summary>
        public ECPoint Owner;

        // <summary>
        // 管理员地址
        // </summary>
        /// <summary>
        /// The admin of this asset
        /// </summary>
        public UInt160 Admin;

        // <summary>
        // 发行者地址
        // </summary>
        /// <summary>
        /// The issuer of this asset
        /// </summary>
        public UInt160 Issuer;

        // <summary>
        // 资产过期时间（允许上链的最后区块高度）
        // </summary>
        /// <summary>
        /// The expiration date of this asset(The block height which allow this asset recorded)
        /// </summary>
        public uint Expiration;

        // <summary>
        // 资产是否冻结
        // </summary>
        /// <summary>
        /// If this asset frozen
        /// </summary>
        public bool IsFrozen;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of this object
        /// </summary>
        public override int Size => base.Size + AssetId.Size + sizeof(AssetType) + Name.GetVarSize() + Amount.Size + Available.Size + sizeof(byte) + sizeof(byte) + Fee.Size + FeeAddress.Size + Owner.Size + Admin.Size + Issuer.Size + sizeof(uint) + sizeof(bool);

        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// Clone method
        /// </summary>
        /// <returns>The cloned object</returns>
        AssetState ICloneable<AssetState>.Clone()
        {
            return new AssetState
            {
                AssetId = AssetId,
                AssetType = AssetType,
                Name = Name,
                Amount = Amount,
                Available = Available,
                Precision = Precision,
                //FeeMode = FeeMode,
                Fee = Fee,
                FeeAddress = FeeAddress,
                Owner = Owner,
                Admin = Admin,
                Issuer = Issuer,
                Expiration = Expiration,
                IsFrozen = IsFrozen,
                _names = _names
            };
        }

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input stream</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            AssetId = reader.ReadSerializable<UInt256>();
            AssetType = (AssetType)reader.ReadByte();
            Name = reader.ReadVarString();
            Amount = reader.ReadSerializable<Fixed8>();
            Available = reader.ReadSerializable<Fixed8>();
            Precision = reader.ReadByte();
            reader.ReadByte(); //FeeMode
            Fee = reader.ReadSerializable<Fixed8>(); //Fee
            FeeAddress = reader.ReadSerializable<UInt160>();
            Owner = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
            Admin = reader.ReadSerializable<UInt160>();
            Issuer = reader.ReadSerializable<UInt160>();
            Expiration = reader.ReadUInt32();
            IsFrozen = reader.ReadBoolean();
        }

        // <summary>
        // 从指定参数的副本复制信息到将当前资产.
        // </summary>
        // <param name="replica">资产的拷贝副本</param>
        /// <summary>
        /// Copy the specified replication to the current asset
        /// </summary>
        /// <param name="replica">A replication of asset</param>
        void ICloneable<AssetState>.FromReplica(AssetState replica)
        {
            AssetId = replica.AssetId;
            AssetType = replica.AssetType;
            Name = replica.Name;
            Amount = replica.Amount;
            Available = replica.Available;
            Precision = replica.Precision;
            //FeeMode = replica.FeeMode;
            Fee = replica.Fee;
            FeeAddress = replica.FeeAddress;
            Owner = replica.Owner;
            Admin = replica.Admin;
            Issuer = replica.Issuer;
            Expiration = replica.Expiration;
            IsFrozen = replica.IsFrozen;
            _names = replica._names;
        }

        private Dictionary<CultureInfo, string> _names;

        // <summary>
        // 查询资产名称
        // </summary>
        // <param name="culture">语言环境</param>
        // <returns>资产名</returns>
        /// <summary>
        /// Get the name of this asset
        /// </summary>
        /// <param name="culture">locale environment</param>
        /// <returns>The name of asset</returns>
        public string GetName(CultureInfo culture = null)
        {
            if (AssetType == AssetType.GoverningToken) return "NEO";
            if (AssetType == AssetType.UtilityToken) return "NeoGas";
            if (_names == null)
            {
                JObject name_obj;
                try
                {
                    name_obj = JObject.Parse(Name);
                }
                catch (FormatException)
                {
                    name_obj = Name;
                }
                if (name_obj is JString)
                    _names = new Dictionary<CultureInfo, string> { { new CultureInfo("en"), name_obj.AsString() } };
                else
                    _names = ((JArray)name_obj).Where(p => p.ContainsProperty("lang") && p.ContainsProperty("name")).ToDictionary(p => new CultureInfo(p["lang"].AsString()), p => p["name"].AsString());
            }
            if (culture == null) culture = CultureInfo.CurrentCulture;
            if (_names.TryGetValue(culture, out string name))
            {
                return name;
            }
            else if (_names.TryGetValue(en, out name))
            {
                return name;
            }
            else
            {
                return _names.Values.First();
            }
        }

        private static readonly CultureInfo en = new CultureInfo("en");

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>AssetId</term>
        // <description>资产id</description>
        // </item>
        // <item>
        // <term>AssetType</term>
        // <description>资产类型</description>
        //</item>
        // <item>
        // <term>Name</term>
        // <description>资产名称</description>
        // </item>
        // <item>
        // <term>Amount</term>
        // <description>总量</description>
        // </item>
        // <item>
        // <term>Available</term>
        // <description>可用量</description>
        // </item>
        // <item>
        // <term>Precision</term>
        // <description>精度</description>
        // </item>
        // <item>
        // <term>FeeMode</term>
        // <description>费用模式，目前为0</description>
        // </item>
        // <item>
        // <term>Fee</term>
        // <description>费用</description>
        // </item>
        // <item>
        // <term>FeeAddress</term>
        // <description>收费地址</description>
        // </item>
        // <item>
        // <term>Owner</term>
        // <description>所有者地址</description>
        // </item>
        // <item>
        // <term>Admin</term>
        // <description>管理员地址</description>
        // </item>
        // <item>
        // <term>Issuer</term>
        // <description>发行者地址</description>
        // </item>
        // <item>
        // <term>Expiration</term>
        // <description>资产过期时间</description>
        // </item>
        // <item>
        // <term>IsFrozen</term>
        // <description>资产是否冻结</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>

        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The version of asset state</description>
        /// </item>
        /// <item>
        /// <term>AssetId</term>
        /// <description>The id of this Asset</description>
        /// </item>
        /// <item>
        /// <term>AssetType</term>
        /// <description>The type of this asset</description>
        /// </item>
        /// <item>
        /// <term>Name</term>
        /// <description>The name of this asset</description>
        /// </item>
        /// <item>
        /// <term>Amount</term>
        /// <description>Total amount of this asset</description>
        /// </item>
        /// <item>
        /// <term>Available</term>
        /// <description>Available amount of this asset</description>
        /// </item>
        /// <item>
        /// <term>Precision</term>
        /// <description>The precision</description>
        /// </item>
        /// <item>
        /// <term>FeeMode</term>
        /// <description>The feemode, currently is 0</description>
        /// </item>
        /// <item>
        /// <term>Fee</term>
        /// <description>Fee/description>
        /// </item>
        /// <item>
        /// <term>FeeAddress</term>
        /// <description>The address of charging fee</description>
        /// </item>
        /// <item>
        /// <term>Owner</term>
        /// <description>The address of owner</description>
        /// </item>
        /// <item>
        /// <term>Admin</term>
        /// <description>The address of admin</description>
        /// </item>
        /// <item>
        /// <term>Issuer</term>
        /// <description>The address of issuer</description>
        /// </item>
        /// <item>
        /// <term>Expiration</term>
        /// <description>The expiration date</description>
        /// </item>
        /// <item>
        /// <term>IsFrozen</term>
        /// <description>If this assset if Frozen</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">The binary output stream</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(AssetId);
            writer.Write((byte)AssetType);
            writer.WriteVarString(Name);
            writer.Write(Amount);
            writer.Write(Available);
            writer.Write(Precision);
            writer.Write(FeeMode);
            writer.Write(Fee);
            writer.Write(FeeAddress);
            writer.Write(Owner);
            writer.Write(Admin);
            writer.Write(Issuer);
            writer.Write(Expiration);
            writer.Write(IsFrozen);
        }

        // <summary>
        // 将这个AssetState转成json对象返回
        // </summary>
        // <returns>转换好的json对象</returns>
        /// <summary>
        /// Transfer this asset to a json object
        /// </summary>
        /// <returns>Transferd json object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["id"] = AssetId.ToString();
            json["type"] = AssetType;
            try
            {
                json["name"] = Name == "" ? null : JObject.Parse(Name);
            }
            catch (FormatException)
            {
                json["name"] = Name;
            }
            json["amount"] = Amount.ToString();
            json["available"] = Available.ToString();
            json["precision"] = Precision;
            json["owner"] = Owner.ToString();
            json["admin"] = Admin.ToAddress();
            json["issuer"] = Issuer.ToAddress();
            json["expiration"] = Expiration;
            json["frozen"] = IsFrozen;
            return json;
        }

        // <summary>
        // 转成String
        // </summary>
        // <returns>返回资产名字</returns>
        /// <summary>
        /// Transfer to a String
        /// </summary>
        /// <returns>reurn the name of this asset</returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}
