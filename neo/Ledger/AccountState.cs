using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.VM;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    /// <summary>
    /// 用户状态
    /// </summary>
    public class AccountState : StateBase, ICloneable<AccountState>
    {
        /// <summary>
        /// 脚本合约hash
        /// </summary>
        public UInt160 ScriptHash;

        /// <summary>
        /// 账户是否冻结
        /// </summary>
        public bool IsFrozen;

        /// <summary>
        /// 投票列表
        /// </summary>
        public ECPoint[] Votes;

        /// <summary>
        /// 全局资产余额
        /// </summary>
        public Dictionary<UInt256, Fixed8> Balances;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + ScriptHash.Size + sizeof(bool) + Votes.GetVarSize()
            + IO.Helper.GetVarSize(Balances.Count) + Balances.Count * (32 + 8);

        /// <summary>
        /// 创建用户状态
        /// </summary>
        public AccountState() { }

        /// <summary>
        /// 创建用户状态
        /// </summary>
        /// <param name="hash">脚本hash</param>
        public AccountState(UInt160 hash)
        {
            this.ScriptHash = hash;
            this.IsFrozen = false;
            this.Votes = new ECPoint[0];
            this.Balances = new Dictionary<UInt256, Fixed8>();
        }

        AccountState ICloneable<AccountState>.Clone()
        {
            return new AccountState
            {
                ScriptHash = ScriptHash,
                IsFrozen = IsFrozen,
                Votes = Votes,
                Balances = Balances.ToDictionary(p => p.Key, p => p.Value)
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            ScriptHash = reader.ReadSerializable<UInt160>();
            IsFrozen = reader.ReadBoolean();
            Votes = new ECPoint[reader.ReadVarInt()];
            for (int i = 0; i < Votes.Length; i++)
                Votes[i] = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
            int count = (int)reader.ReadVarInt();
            Balances = new Dictionary<UInt256, Fixed8>(count);
            for (int i = 0; i < count; i++)
            {
                UInt256 assetId = reader.ReadSerializable<UInt256>();
                Fixed8 value = reader.ReadSerializable<Fixed8>();
                Balances.Add(assetId, value);
            }
        }

        /// <summary>
        /// 从副本拷贝值
        /// </summary>
        /// <param name="replica">副本</param>
        void ICloneable<AccountState>.FromReplica(AccountState replica)
        {
            ScriptHash = replica.ScriptHash;
            IsFrozen = replica.IsFrozen;
            Votes = replica.Votes;
            Balances = replica.Balances;
        }

        /// <summary>
        /// 查询资产剩余
        /// </summary>
        /// <param name="asset_id">资产ID</param>
        /// <returns>资产余额，若没有查询到时，返回0</returns>
        public Fixed8 GetBalance(UInt256 asset_id)
        {
            if (!Balances.TryGetValue(asset_id, out Fixed8 value))
                value = Fixed8.Zero;
            return value;
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>ScriptHash</term>
        /// <description>脚本合约hash</description>
        /// </item>
        /// <item>
        /// <term>IsFrozen</term>
        /// <description>账户是否冻结</description>
        /// </item>
        /// <item>
        /// <term>Votes</term>
        /// <description>投票列表</description>
        /// </item>
        /// <item>
        /// <term>Balances</term>
        /// <description>全局资产余额</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(ScriptHash);
            writer.Write(IsFrozen);
            writer.Write(Votes);
            var balances = Balances.Where(p => p.Value > Fixed8.Zero).ToArray();
            writer.WriteVarInt(balances.Length);
            foreach (var pair in balances)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns>格式： { 'script_hash': 'xxxx', 'frozen': false, 'votes':['xxxx'], 'balances':{ 'asset_xxx': 'value...' } } </returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["script_hash"] = ScriptHash.ToString();
            json["frozen"] = IsFrozen;
            json["votes"] = new JArray(Votes.Select(p => (JObject)p.ToString()));
            json["balances"] = new JArray(Balances.Select(p =>
            {
                JObject balance = new JObject();
                balance["asset"] = p.Key.ToString();
                balance["value"] = p.Value.ToString();
                return balance;
            }));
            return json;
        }
    }
}
