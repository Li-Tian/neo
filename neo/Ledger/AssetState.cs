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
    /// <summary>
    /// 资产状态
    /// </summary>
    public class AssetState : StateBase, ICloneable<AssetState>
    {

        /// <summary>
        /// 资产Id
        /// </summary>
        public UInt256 AssetId;


        /// <summary>
        /// 资产类型
        /// </summary>
        public AssetType AssetType;

        /// <summary>
        /// 资产名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 资产总量
        /// </summary>
        public Fixed8 Amount;

        /// <summary>
        /// 资产可用额度
        /// </summary>
        public Fixed8 Available;

        /// <summary>
        /// 精度
        /// </summary>
        public byte Precision;

        /// <summary>
        /// 收费模式
        /// </summary>
        public const byte FeeMode = 0;

        /// <summary>
        /// 费用
        /// </summary>
        public Fixed8 Fee;

        /// <summary>
        /// 收费地址
        /// </summary>
        public UInt160 FeeAddress;

        /// <summary>
        /// 所有者地址
        /// </summary>
        public ECPoint Owner;

        /// <summary>
        /// 管理员地址
        /// </summary>
        public UInt160 Admin;

        /// <summary>
        /// 发行者地址
        /// </summary>
        public UInt160 Issuer;

        /// <summary>
        /// 资产过期时间
        /// </summary>
        public uint Expiration;

        /// <summary>
        /// 资产是否冻结
        /// </summary>
        public bool IsFrozen;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + AssetId.Size + sizeof(AssetType) + Name.GetVarSize() + Amount.Size + Available.Size + sizeof(byte) + sizeof(byte) + Fee.Size + FeeAddress.Size + Owner.Size + Admin.Size + Issuer.Size + sizeof(uint) + sizeof(bool);

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

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
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

        /// <summary>
        /// 从副本读取值
        /// </summary>
        /// <param name="replica"></param>
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

        /// <summary>
        /// 查询资产名称
        /// </summary>
        /// <param name="culture">语言环境</param>
        /// <returns>资产名</returns>
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

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>AssetId</term>
        /// <description>资产id</description>
        /// </item>
        /// <item>
        /// <term>AssetType</term>
        /// <description>资产类型</description>
        /// </item>
        /// <item>
        /// <term>Name</term>
        /// <description>资产名称</description>
        /// </item>
        /// <item>
        /// <term>Amount</term>
        /// <description>总量</description>
        /// </item>
        /// <item>
        /// <term>Available</term>
        /// <description>可用量</description>
        /// </item>
        /// <item>
        /// <term>Precision</term>
        /// <description>精度</description>
        /// </item>
        /// <item>
        /// <term>FeeMode</term>
        /// <description>费用模式，目前为0</description>
        /// </item>
        /// <item>
        /// <term>Fee</term>
        /// <description>费用</description>
        /// </item>
        /// <item>
        /// <term>FeeAddress</term>
        /// <description>收费地址</description>
        /// </item>
        /// <item>
        /// <term>Owner</term>
        /// <description>所有者地址</description>
        /// </item>
        /// <item>
        /// <term>Admin</term>
        /// <description>管理员地址</description>
        /// </item>
        /// <item>
        /// <term>Issuer</term>
        /// <description>发行者地址</description>
        /// </item>
        /// <item>
        /// <term>Expiration</term>
        /// <description>资产过期时间</description>
        /// </item>
        /// <item>
        /// <term>IsFrozen</term>
        /// <description>资产是否冻结</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
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

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 转成String
        /// </summary>
        /// <returns>返回资产名字</returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}
