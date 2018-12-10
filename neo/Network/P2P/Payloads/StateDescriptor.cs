using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 投票状态描述：投票，申请
    /// </summary>
    public class StateDescriptor : ISerializable
    {
        /// <summary>
        /// 类型
        /// </summary>
        public StateType Type;

        /// <summary>
        /// 当Field = "Votes"时， 存放投票人地址的脚本hash， Key代表投票人; 当Field = "Registered"时， 存放公钥， Key代表申请人
        /// </summary>
        public byte[] Key;

        /// <summary>
        /// 当Type = 0x40时， Field = "Votes";  当Type = 0x48时， Field = "Registered";
        /// </summary>
        public string Field;

        /// <summary>
        /// 当Type = 0x40时， 代表投票地址列表；  当Type = 0x48时， 代表取消或申请验证人的布尔值
        /// </summary>
        public byte[] Value;

        /// <summary>
        /// 存储大小
        /// </summary>
        public int Size => sizeof(StateType) + Key.GetVarSize() + Field.GetVarSize() + Value.GetVarSize();


        /// <summary>
        /// 交易手续费  若是申请见证人，需要1000个GAS， 否则为0
        /// </summary>
        public Fixed8 SystemFee
        {
            get
            {
                switch (Type)
                {
                    case StateType.Validator:
                        return GetSystemFee_Validator();
                    default:
                        return Fixed8.Zero;
                }
            }
        }


        private void CheckAccountState()
        {
            if (Key.Length != 20) throw new FormatException();
            if (Field != "Votes") throw new FormatException();
        }

        private void CheckValidatorState()
        {
            if (Key.Length != 33) throw new FormatException();
            if (Field != "Registered") throw new FormatException();
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            Type = (StateType)reader.ReadByte();
            if (!Enum.IsDefined(typeof(StateType), Type))
                throw new FormatException();
            Key = reader.ReadVarBytes(100);
            Field = reader.ReadVarString(32);
            Value = reader.ReadVarBytes(65535);
            switch (Type)
            {
                case StateType.Account:
                    CheckAccountState();
                    break;
                case StateType.Validator:
                    CheckValidatorState();
                    break;
            }
        }

        private Fixed8 GetSystemFee_Validator()
        {
            switch (Field)
            {
                case "Registered":
                    if (Value.Any(p => p != 0))
                        return Fixed8.FromDecimal(1000);
                    else
                        return Fixed8.Zero;
                default:
                    throw new InvalidOperationException();
            }
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.WriteVarBytes(Key);
            writer.WriteVarString(Field);
            writer.WriteVarBytes(Value);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns>json对象</returns>
        public JObject ToJson()
        {
            JObject json = new JObject();
            json["type"] = Type;
            json["key"] = Key.ToHexString();
            json["field"] = Field;
            json["value"] = Value.ToHexString();
            return json;
        }

        internal bool Verify(Snapshot snapshot)
        {
            switch (Type)
            {
                case StateType.Account:
                    return VerifyAccountState(snapshot);
                case StateType.Validator:
                    return VerifyValidatorState();
                default:
                    return false;
            }
        }

        private bool VerifyAccountState(Snapshot snapshot)
        {
            switch (Field)
            {
                case "Votes":
                    ECPoint[] pubkeys;
                    try
                    {
                        pubkeys = Value.AsSerializableArray<ECPoint>((int)Blockchain.MaxValidators);
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                    UInt160 hash = new UInt160(Key);
                    AccountState account = snapshot.Accounts.TryGet(hash);
                    if (account?.IsFrozen != false) return false;
                    if (pubkeys.Length > 0)
                    {
                        if (account.GetBalance(Blockchain.GoverningToken.Hash).Equals(Fixed8.Zero)) return false;
                        HashSet<ECPoint> sv = new HashSet<ECPoint>(Blockchain.StandbyValidators);
                        foreach (ECPoint pubkey in pubkeys)
                            if (!sv.Contains(pubkey) && snapshot.Validators.TryGet(pubkey)?.Registered != true)
                                return false;
                    }
                    return true;
                default:
                    return false;
            }
        }

        private bool VerifyValidatorState()
        {
            switch (Field)
            {
                case "Registered":
                    return true;
                default:
                    return false;
            }
        }
    }
}
