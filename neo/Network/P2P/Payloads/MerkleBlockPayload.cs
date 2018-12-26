using Neo.Cryptography;
using Neo.IO;
using System.Collections;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// SPV钱包所需的数据块的实体类
    /// BlockBase的子类
    /// </summary>
    public class MerkleBlockPayload : BlockBase
    {
        // <summary>
        // 交易数量
        // </summary>
        /// <summary>
        /// The number of transactions
        /// </summary>
        public int TxCount;

        // <summary>
        // 数组形式的梅克尔树
        // </summary>
        /// <summary>
        /// The merkle tree in the array format
        /// </summary>
        public UInt256[] Hashes;
        // <summary>
        // 标志位，表示梅克尔树中哪些节点可以省略，
        // 哪些节点不可以省略，用于梅克尔树与数组
        // 的相互转换。(little endian)
        // </summary>
        /// <summary>
        /// The flags , which stands for which nodes are ignored, and which nodes are not ignored. This flag is used for transferring to array<br/>
        /// </summary>
        public byte[] Flags;

        // <summary>
        // 数据块的大小
        // </summary>
        /// <summary>
        /// The size of data blocks
        /// </summary>
        public override int Size => base.Size + sizeof(int) + Hashes.GetVarSize() + Flags.GetVarSize();
        // <summary>
        // 创建SPV钱包验证交易所需的数据块
        // </summary>
        // <param name="block">区块数据</param>
        // <param name="flags">标志位</param>
        // <returns>所需的数据块</returns>
        /// <summary>
        /// The data which need to verify the transaction when create SPV wallet
        /// </summary>
        /// <param name="block">block data</param>
        /// <param name="flags">flags</param>
        /// <returns></returns>
        public static MerkleBlockPayload Create(Block block, BitArray flags)
        {
            MerkleTree tree = new MerkleTree(block.Transactions.Select(p => p.Hash).ToArray());
            tree.Trim(flags);
            byte[] buffer = new byte[(flags.Length + 7) / 8];
            flags.CopyTo(buffer, 0);
            return new MerkleBlockPayload
            {
                Version = block.Version,
                PrevHash = block.PrevHash,
                MerkleRoot = block.MerkleRoot,
                Timestamp = block.Timestamp,
                Index = block.Index,
                ConsensusData = block.ConsensusData,
                NextConsensus = block.NextConsensus,
                Witness = block.Witness,
                TxCount = block.Transactions.Length,
                Hashes = tree.ToHashArray(),
                Flags = buffer
            };
        }
        // <summary>
        // 反序列化方法
        // </summary>
        // <param name="reader">2进制读取器</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TxCount = (int)reader.ReadVarInt(int.MaxValue);
            Hashes = reader.ReadSerializableArray<UInt256>();
            Flags = reader.ReadVarBytes();
        }
        // <summary>
        // 序列化方法
        // </summary>
        // <param name="writer">2进制输出器</param>
        /// <summary>
        /// The serialization method
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteVarInt(TxCount);
            writer.Write(Hashes);
            writer.WriteVarBytes(Flags);
        }
    }
}
