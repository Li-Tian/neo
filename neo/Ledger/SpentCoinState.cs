using Neo.IO;
using System.Collections.Generic;
using System.IO;

namespace Neo.Ledger
{
    // <summary>
    // 已花费交易的状态（主要用来记录NEO的，方便计算claim GAS)
    // </summary>
    /// <summary>
    /// The state of already spent coin (To record NEO and claim Gas)
    /// </summary>
    public class SpentCoinState : StateBase, ICloneable<SpentCoinState>
    {
        // <summary>
        // 交易hash
        // </summary>
        /// <summary>
        /// The Transcation Hash
        /// </summary>
        public UInt256 TransactionHash;

        // <summary>
        // 交易所在区块高度
        // </summary>
        /// <summary>
        /// The block height where the transaction exists
        /// </summary>
        public uint TransactionHeight;

        // <summary>
        //  已花费的outputs高度信息, 
        //  output.index -> 花费该output的block.Index
        // </summary>
        /// <summary>
        /// The height information of spent transaction output
        /// </summary>
        public Dictionary<ushort, uint> Items;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage
        /// </summary>
        public override int Size => base.Size + TransactionHash.Size + sizeof(uint)
            + IO.Helper.GetVarSize(Items.Count) + Items.Count * (sizeof(ushort) + sizeof(uint));

        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>replica of object</returns>
        SpentCoinState ICloneable<SpentCoinState>.Clone()
        {
            return new SpentCoinState
            {
                TransactionHash = TransactionHash,
                TransactionHeight = TransactionHeight,
                Items = new Dictionary<ushort, uint>(Items)
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

        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本数据</param>
        /// <summary>
        /// Copy from the replication
        /// </summary>
        /// <param name="replica">The replication data</param>
        void ICloneable<SpentCoinState>.FromReplica(SpentCoinState replica)
        {
            TransactionHash = replica.TransactionHash;
            TransactionHeight = replica.TransactionHeight;
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
        // <term>TransactionHash</term>
        // <description>交易hash</description>
        // </item>
        // <item>
        // <term>TransactionHeight</term>
        // <description>交易所在区块高度</description>
        // </item>
        // <item>
        // <term>Items</term>
        // <description>已花费的outputs高度信息</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>
        // 
        /// <summary>
        /// Serialization method
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The state of version</description>
        /// </item>
        /// <item>
        /// <term>TransactionHash</term>
        /// <description>The hash of transaction</description>
        /// </item>
        /// <item>
        /// <term>TransactionHeight</term>
        /// <description>The height of the transaction</description>
        /// </item>
        /// <item>
        /// <term>Items</term>
        /// <description>The height information of spent transaction output </description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">Binary output writer</param>
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
