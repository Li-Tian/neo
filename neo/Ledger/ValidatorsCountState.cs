using Neo.IO;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 验证人个数投票状态
    /// </summary>
    public class ValidatorsCountState : StateBase, ICloneable<ValidatorsCountState>
    {
        /// <summary>
        /// 投票数组， 数组脚本即验证人投票个数
        /// </summary>
        public Fixed8[] Votes;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Votes.GetVarSize();

        /// <summary>
        /// 创建验证人个数投票状态
        /// </summary>
        public ValidatorsCountState()
        {
            this.Votes = new Fixed8[Blockchain.MaxValidators];
        }

        ValidatorsCountState ICloneable<ValidatorsCountState>.Clone()
        {
            return new ValidatorsCountState
            {
                Votes = (Fixed8[])Votes.Clone()
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Votes = reader.ReadSerializableArray<Fixed8>();
        }

        void ICloneable<ValidatorsCountState>.FromReplica(ValidatorsCountState replica)
        {
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
        /// <term>Votes</term>
        /// <description>验证人个数投票情况</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Votes);
        }
    }
}
