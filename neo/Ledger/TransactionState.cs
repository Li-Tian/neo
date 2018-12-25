using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using System.IO;

namespace Neo.Ledger
{
    // <summary>
    // 交易状态
    // </summary>
    /// <summary>
    /// The state of transaction
    /// </summary>
    public class TransactionState : StateBase, ICloneable<TransactionState>
    {
        // <summary>
        // 交易所在区块高度
        // </summary>
        /// <summary>
        /// The height of this transaction
        /// </summary>
        public uint BlockIndex;

        // <summary>
        // 具体的交易
        // </summary>
        /// <summary>
        /// The concrete tranaction
        /// </summary>
        public Transaction Transaction;


        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The storage of this state object
        /// </summary>
        public override int Size => base.Size + sizeof(uint) + Transaction.Size;
        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>The clone target object</returns>
        TransactionState ICloneable<TransactionState>.Clone()
        {
            return new TransactionState
            {
                BlockIndex = BlockIndex,
                Transaction = Transaction
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
            BlockIndex = reader.ReadUInt32();
            Transaction = Transaction.DeserializeFrom(reader);
        }

        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本</param>
        /// <summary>
        /// Copy from replication
        /// </summary>
        /// <param name="replica">replication</param>
        void ICloneable<TransactionState>.FromReplica(TransactionState replica)
        {
            BlockIndex = replica.BlockIndex;
            Transaction = replica.Transaction;
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>BlockIndex</term>
        // <description>交易所在区块高度</description>
        // </item>
        // <item>
        // <term>Transaction</term>
        // <description>具体的交易</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>

        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The version of state</description>
        /// </item>
        /// <item>
        /// <term>BlockIndex</term>
        /// <description>The height of the block which the transaction exists</description>
        /// </item>
        /// <item>
        /// <term>Transaction</term>
        /// <description>The concrete transaction</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(BlockIndex);
            writer.Write(Transaction);
        }


        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>
        /// <summary>
        /// Transfer to Json object
        /// </summary>
        /// <returns>Json object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["height"] = BlockIndex;
            json["tx"] = Transaction.ToJson();
            return json;
        }
    }
}
