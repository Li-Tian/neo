using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 共识消息货物（包装具体的共识消息. p2p广播时，将存放在Inventory消息的payload中）
    /// </summary>
    public class ConsensusPayload : IInventory
    {
        /// <summary>
        /// 当前共识协议版本号
        /// </summary>
        public uint Version;

        /// <summary>
        /// 上一个区块hash
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// 提案block高度
        /// </summary>
        public uint BlockIndex;

        /// <summary>
        /// 共识节点编号
        /// </summary>
        public ushort ValidatorIndex;
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// 具体的共识消息
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// 见证人, 可执行验证脚本
        /// </summary>
        public Witness Witness;

        private UInt256 _hash = null;

        /// <summary>
        /// 内容hash, 未签名数据的hash256值
        /// </summary>
        UInt256 IInventory.Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = new UInt256(Crypto.Default.Hash256(this.GetHashData()));
                }
                return _hash;
            }
        }

        /// <summary>
        /// 货物类型， InventoryType.Consensus
        /// </summary>
        InventoryType IInventory.InventoryType => InventoryType.Consensus;

        /// <summary>
        /// 见证人列表
        /// </summary>
        Witness[] IVerifiable.Witnesses
        {
            get
            {
                return new[] { Witness };
            }
            set
            {
                if (value.Length != 1) throw new ArgumentException();
                Witness = value[0];
            }
        }

        /// <summary>
        /// 消息大小
        /// </summary>
        public int Size => sizeof(uint) + PrevHash.Size + sizeof(uint) + sizeof(ushort) + sizeof(uint) + Data.GetVarSize() + 1 + Witness.Size;

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            ((IVerifiable)this).DeserializeUnsigned(reader);
            if (reader.ReadByte() != 1) throw new FormatException();
            Witness = reader.ReadSerializable<Witness>();
        }

        /// <summary>
        /// 序列化未签名数据
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        void IVerifiable.DeserializeUnsigned(BinaryReader reader)
        {
            Version = reader.ReadUInt32();
            PrevHash = reader.ReadSerializable<UInt256>();
            BlockIndex = reader.ReadUInt32();
            ValidatorIndex = reader.ReadUInt16();
            Timestamp = reader.ReadUInt32();
            Data = reader.ReadVarBytes();
        }

        /// <summary>
        /// 脚本容器消息
        /// </summary>
        /// <returns>未签名数据</returns>
        byte[] IScriptContainer.GetMessage()
        {
            return this.GetHashData();
        }

        /// <summary>
        /// 获取验证脚本hash
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <returns>脚本hash列表</returns>
        UInt160[] IVerifiable.GetScriptHashesForVerifying(Snapshot snapshot)
        {
            ECPoint[] validators = snapshot.GetValidators();
            if (validators.Length <= ValidatorIndex)
                throw new InvalidOperationException();
            return new[] { Contract.CreateSignatureRedeemScript(validators[ValidatorIndex]).ToScriptHash() };
        }

        /// <summary>
        /// 序列化数据
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            ((IVerifiable)this).SerializeUnsigned(writer);
            writer.Write((byte)1); writer.Write(Witness);
        }

        /// <summary>
        ///  序列化未签名数据
        /// <list type="bullet">
        /// <item>
        /// <term>Version</term>
        /// <description>版本号</description>
        /// </item>
        /// <item>
        /// <term>PrevHash</term>
        /// <description>上一个区块hash</description>
        /// </item>
        /// <item>
        /// <term>BlockIndex</term>
        /// <description>区块高度</description>
        /// </item>
        /// <item>
        /// <term>ValidatorIndex</term>
        /// <description>共识节点编号</description>
        /// </item>
        /// <item>
        /// <term>Timestamp</term>
        /// <description>时间戳</description>
        /// </item>
        /// <item>
        /// <term>Data</term>
        /// <description>具体的共识数据</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">binary writer</param>
        void IVerifiable.SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(PrevHash);
            writer.Write(BlockIndex);
            writer.Write(ValidatorIndex);
            writer.Write(Timestamp);
            writer.WriteVarBytes(Data);
        }

        /// <summary>
        /// 内容校验
        /// </summary>
        /// <remarks>
        /// 1) 检查BlockIndex 是否大于已经存储的区块高度
        /// 2) 校验验证脚本
        /// </remarks>
        /// <param name="snapshot">区块快照</param>
        /// <returns>校验通过返回true，否则返回false</returns>
        public bool Verify(Snapshot snapshot)
        {
            if (BlockIndex <= snapshot.Height)
                return false;
            return this.VerifyWitnesses(snapshot);
        }
    }
}
