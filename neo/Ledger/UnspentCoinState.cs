using Neo.IO;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    // <summary>
    // UTXO状态
    // </summary>
    /// <summary>
    /// The state of UTXO
    /// </summary>
    public class UnspentCoinState : StateBase, ICloneable<UnspentCoinState>
    {
        // <summary>
        // output项状态列表
        // </summary>
        /// <summary>
        /// The State of coins
        /// </summary>
        public CoinState[] Items;


        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage
        /// </summary>
        public override int Size => base.Size + Items.GetVarSize();

        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>replica of object</returns>
        UnspentCoinState ICloneable<UnspentCoinState>.Clone()
        {
            return new UnspentCoinState
            {
                Items = (CoinState[])Items.Clone()
            };
        }

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserilization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Items = reader.ReadVarBytes().Select(p => (CoinState)p).ToArray();
        }

        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本</param>
        /// <summary>
        /// Copy from replication
        /// </summary>
        /// <param name="replica">Replication</param>
        void ICloneable<UnspentCoinState>.FromReplica(UnspentCoinState replica)
        {
            Items = replica.Items;
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>Items</term>
        // <description>output项状态列表</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>version of state</description>
        /// </item>
        /// <item>
        /// <term>Items</term>
        /// <description>The list of outputs</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteVarBytes(Items.Cast<byte>().ToArray());
        }
    }
}
