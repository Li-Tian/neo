using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 交易状态
    /// </summary>
    public class TransactionState : StateBase, ICloneable<TransactionState>
    {
        /// <summary>
        /// 交易所在区块高度
        /// </summary>
        public uint BlockIndex;

        /// <summary>
        /// 具体的交易
        /// </summary>
        public Transaction Transaction;


        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + sizeof(uint) + Transaction.Size;

        TransactionState ICloneable<TransactionState>.Clone()
        {
            return new TransactionState
            {
                BlockIndex = BlockIndex,
                Transaction = Transaction
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            BlockIndex = reader.ReadUInt32();
            Transaction = Transaction.DeserializeFrom(reader);
        }

        void ICloneable<TransactionState>.FromReplica(TransactionState replica)
        {
            BlockIndex = replica.BlockIndex;
            Transaction = replica.Transaction;
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>BlockIndex</term>
        /// <description>交易所在区块高度</description>
        /// </item>
        /// <item>
        /// <term>Transaction</term>
        /// <description>具体的交易</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(BlockIndex);
            writer.Write(Transaction);
        }


        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["height"] = BlockIndex;
            json["tx"] = Transaction.ToJson();
            return json;
        }
    }
}
