using Neo.Cryptography.ECC;
using Neo.IO;
using System.IO;

namespace Neo.Ledger
{
    // <summary>
    // 验证人状态
    // </summary>
    /// <summary>
    /// the state of validator
    /// </summary>
    public class ValidatorState : StateBase, ICloneable<ValidatorState>
    {
        // <summary>
        // 验证人公钥
        // </summary>
        /// <summary>
        /// The public key of validators
        /// </summary>
        public ECPoint PublicKey;

        // <summary>
        // Is it registered
        // </summary>
        public bool Registered;

        // <summary>
        // 投票数
        // </summary>
        /// <summary>
        /// The votes on it
        /// </summary>
        public Fixed8 Votes;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage
        /// </summary>
        public override int Size => base.Size + PublicKey.Size + sizeof(bool) + Votes.Size;

        // <summary>
        // 构造函数
        // </summary>
        /// <summary>
        /// Empty constructor
        /// </summary>
        public ValidatorState() { }


        // <summary>
        // 构造函数
        // </summary>
        // <param name="pubkey">验证人公钥</param>
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="pubkey">The public key of validators</param>
        public ValidatorState(ECPoint pubkey)
        {
            this.PublicKey = pubkey;
            this.Registered = false;
            this.Votes = Fixed8.Zero;
        }
        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>replica of object</returns>
        ValidatorState ICloneable<ValidatorState>.Clone()
        {
            return new ValidatorState
            {
                PublicKey = PublicKey,
                Registered = Registered,
                Votes = Votes
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
            PublicKey = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
            Registered = reader.ReadBoolean();
            Votes = reader.ReadSerializable<Fixed8>();
        }

        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本</param>
        /// <summary>
        /// Copy from replication
        /// </summary>
        /// <param name="replica">Replication of other validator state</param>
        void ICloneable<ValidatorState>.FromReplica(ValidatorState replica)
        {
            PublicKey = replica.PublicKey;
            Registered = replica.Registered;
            Votes = replica.Votes;
        }


        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>PublicKey</term>
        // <description>验证人公钥</description>
        // </item>
        // <item>
        // <term>Registered</term>
        // <description>是否注册</description>
        // </item>
        // <item>
        // <term>Votes</term>
        // <description>投票数</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>

        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The verison of state</description>
        /// </item>
        /// <item>
        /// <term>PublicKey</term>
        /// <description>The public key of validator</description>
        /// </item>
        /// <item>
        /// <term>Registered</term>
        /// <description>Is it registerd</description>
        /// </item>
        /// <item>
        /// <term>Votes</term>
        /// <description>The number of votes</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">The binary output stream writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(PublicKey);
            writer.Write(Registered);
            writer.Write(Votes);
        }
    }
}
