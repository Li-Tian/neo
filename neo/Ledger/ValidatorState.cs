using Neo.Cryptography.ECC;
using Neo.IO;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 验证人状态
    /// </summary>
    public class ValidatorState : StateBase, ICloneable<ValidatorState>
    {
        /// <summary>
        /// 验证人公钥
        /// </summary>
        public ECPoint PublicKey;

        /// <summary>
        /// 是否注册
        /// </summary>
        public bool Registered;

        /// <summary>
        /// 投票数
        /// </summary>
        public Fixed8 Votes;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + PublicKey.Size + sizeof(bool) + Votes.Size;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ValidatorState() { }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pubkey">验证人公钥</param>
        public ValidatorState(ECPoint pubkey)
        {
            this.PublicKey = pubkey;
            this.Registered = false;
            this.Votes = Fixed8.Zero;
        }
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>克隆</returns>
        ValidatorState ICloneable<ValidatorState>.Clone()
        {
            return new ValidatorState
            {
                PublicKey = PublicKey,
                Registered = Registered,
                Votes = Votes
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            PublicKey = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
            Registered = reader.ReadBoolean();
            Votes = reader.ReadSerializable<Fixed8>();
        }

        /// <summary>
        /// 从副本复制
        /// </summary>
        /// <param name="replica">副本</param>
        void ICloneable<ValidatorState>.FromReplica(ValidatorState replica)
        {
            PublicKey = replica.PublicKey;
            Registered = replica.Registered;
            Votes = replica.Votes;
        }


        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>PublicKey</term>
        /// <description>验证人公钥</description>
        /// </item>
        /// <item>
        /// <term>Registered</term>
        /// <description>是否注册</description>
        /// </item>
        /// <item>
        /// <term>Votes</term>
        /// <description>投票数</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(PublicKey);
            writer.Write(Registered);
            writer.Write(Votes);
        }
    }
}
