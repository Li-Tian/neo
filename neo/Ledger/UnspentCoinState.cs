using Neo.IO;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    /// <summary>
    /// UTXO状态
    /// </summary>
    public class UnspentCoinState : StateBase, ICloneable<UnspentCoinState>
    {
        /// <summary>
        /// output项状态列表
        /// </summary>
        public CoinState[] Items;


        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Items.GetVarSize();
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>克隆对象</returns>
        UnspentCoinState ICloneable<UnspentCoinState>.Clone()
        {
            return new UnspentCoinState
            {
                Items = (CoinState[])Items.Clone()
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Items = reader.ReadVarBytes().Select(p => (CoinState)p).ToArray();
        }

        /// <summary>
        /// 从副本复制
        /// </summary>
        /// <param name="replica">副本</param>
        void ICloneable<UnspentCoinState>.FromReplica(UnspentCoinState replica)
        {
            Items = replica.Items;
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>Items</term>
        /// <description>output项状态列表</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteVarBytes(Items.Cast<byte>().ToArray());
        }
    }
}
