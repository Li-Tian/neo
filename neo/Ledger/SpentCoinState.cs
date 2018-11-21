using Neo.IO;
using System.Collections.Generic;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 已花费交易的状态（主要用来记录NEO的，方便计算claim GAS)
    /// </summary>
    public class SpentCoinState : StateBase, ICloneable<SpentCoinState>
    {
        /// <summary>
        /// 交易hash
        /// </summary>
        public UInt256 TransactionHash;

        /// <summary>
        /// 交易所在区块高度
        /// </summary>
        public uint TransactionHeight;

        /// <summary>
        ///  已花费的outputs高度信息, output.index -> 花费该output的block.Index
        /// </summary>
        public Dictionary<ushort, uint> Items;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + TransactionHash.Size + sizeof(uint)
            + IO.Helper.GetVarSize(Items.Count) + Items.Count * (sizeof(ushort) + sizeof(uint));

        SpentCoinState ICloneable<SpentCoinState>.Clone()
        {
            return new SpentCoinState
            {
                TransactionHash = TransactionHash,
                TransactionHeight = TransactionHeight,
                Items = new Dictionary<ushort, uint>(Items)
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TransactionHash = reader.ReadSerializable<UInt256>();
            TransactionHeight = reader.ReadUInt32();
            int count = (int)reader.ReadVarInt();
            Items = new Dictionary<ushort, uint>(count);
            for (int i = 0; i < count; i++)
            {
                ushort index = reader.ReadUInt16();
                uint height = reader.ReadUInt32();
                Items.Add(index, height);
            }
        }

        void ICloneable<SpentCoinState>.FromReplica(SpentCoinState replica)
        {
            TransactionHash = replica.TransactionHash;
            TransactionHeight = replica.TransactionHeight;
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
        /// <term>TransactionHash</term>
        /// <description>交易hash</description>
        /// </item>
        /// <item>
        /// <term>TransactionHeight</term>
        /// <description>交易所在区块高度</description>
        /// </item>
        /// <item>
        /// <term>Items</term>
        /// <description>已花费的outputs高度信息</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(TransactionHash);
            writer.Write(TransactionHeight);
            writer.WriteVarInt(Items.Count);
            foreach (var pair in Items)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }
    }
}
