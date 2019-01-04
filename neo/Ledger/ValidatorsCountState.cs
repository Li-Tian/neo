using Neo.IO;
using System.IO;

namespace Neo.Ledger
{
    // <summary>
    // 验证人个数投票状态
    // </summary>
    /// <summary>
    /// The state of validators's count
    /// </summary>
    public class ValidatorsCountState : StateBase, ICloneable<ValidatorsCountState>
    {
        // <summary>
        // 投票数组， 数组下标(index)即验证人投票个数
        // </summary>
        /// <summary>
        /// The list of votes whose index stands for the count of validators
        /// </summary>
        public Fixed8[] Votes;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage
        /// </summary>
        public override int Size => base.Size + Votes.GetVarSize();

        // <summary>
        // 创建验证人个数投票状态
        // </summary>
        /// <summary>
        /// Constructor of creating the state of validators's count
        /// </summary>
        public ValidatorsCountState()
        {
            this.Votes = new Fixed8[Blockchain.MaxValidators];
        }
        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>replica of object</returns>
        ValidatorsCountState ICloneable<ValidatorsCountState>.Clone()
        {
            return new ValidatorsCountState
            {
                Votes = (Fixed8[])Votes.Clone()
            };
        }

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Votes = reader.ReadSerializableArray<Fixed8>();
        }

        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本</param>
        /// <summary>
        /// Copy from Replication
        /// </summary>
        /// <param name="replica">replication</param>
        void ICloneable<ValidatorsCountState>.FromReplica(ValidatorsCountState replica)
        {
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
        // <term>Votes</term>
        // <description>验证人个数投票情况</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>

        /// <summary>
        /// Serilization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The version of the state</description>
        /// </item>
        /// <item>
        /// <term>Votes</term>
        /// <description>The status of votings</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Votes);
        }
    }
}
