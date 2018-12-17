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
    /// P2p consensus message payload
    /// 共识消息传输数据包（包装具体的共识消息. p2p广播时，将存放在Inventory消息的payload中）
    /// </summary>
    public class ConsensusPayload : IInventory
    {
        /// <summary>
        /// Consensus message version
        /// </summary>
        public uint Version;
        
        /// <summary>
        /// The previous block hash
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// The proposal block index
        /// </summary>
        public uint BlockIndex;

        /// <summary>
        /// The sender(the Speaker or Delegates) index in the validators array
        /// 议长的共识节点编号
        /// </summary>
        public ushort ValidatorIndex;

        /// <summary>
        /// Block timestamp
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// Consensus message data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Witness, the executable verification script
        /// </summary>
        public Witness Witness;

        private UInt256 _hash = null;

        /// <summary>
        /// Hash data of the GetHashData( = unsigned data) by using hash256 algorithm
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
        /// P2p inventory message type, equals to InventoryType.Consensus
        /// Inventory类型， InventoryType.Consensus
        /// </summary>
        InventoryType IInventory.InventoryType => InventoryType.Consensus;

        /// <summary>
        /// Witness array
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
        /// ConsensusPayload size
        /// </summary>
        public int Size => sizeof(uint) + PrevHash.Size + sizeof(uint) + sizeof(ushort) + sizeof(uint) + Data.GetVarSize() + 1 + Witness.Size;

        /// <summary>
        /// Deserialize from the reader
        /// </summary>
        /// <param name="reader"></param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            ((IVerifiable)this).DeserializeUnsigned(reader);
            if (reader.ReadByte() != 1) throw new FormatException();
            Witness = reader.ReadSerializable<Witness>();
        }

        /// <summary>
        /// Deserialize from the reader of the unsigned binary data without the witness field
        /// 反序列化待签名数据
        /// </summary>
        /// <param name="reader">BinaryReader input</param>
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
        /// 获取原始的哈希数据
        /// </summary>
        /// <returns>原始的哈希数据</returns>
        byte[] IScriptContainer.GetMessage()
        {
            return this.GetHashData();
        }

        /// <summary>
        /// Get the verification scripts' hashes
        /// 获取议长公钥的签名脚本
        /// </summary>
        /// <param name="snapshot">数据库快照</param>
        /// <returns>The script hash of the sender's signing contract</returns>
        UInt160[] IVerifiable.GetScriptHashesForVerifying(Snapshot snapshot)
        {
            ECPoint[] validators = snapshot.GetValidators();
            if (validators.Length <= ValidatorIndex)
                throw new InvalidOperationException();
            return new[] { Contract.CreateSignatureRedeemScript(validators[ValidatorIndex]).ToScriptHash() };
        }

        /// <summary>
        /// Serialize the message. It includes the fields as follows:
        /// <list type="bullet">
        /// <item>
        /// <term>Version</term>
        /// <description>consensus message version, current is zero</description>
        /// </item>
        /// <item>
        /// <term>PrevHash</term>
        /// <description>the previous block hash</description>
        /// </item>
        /// <item>
        /// <term>BlockIndex</term>
        /// <description>the proposal block index</description>
        /// </item>
        /// <item>
        /// <term>ValidatorIndex</term>
        /// <description>the sender(the Speaker or Delegates) index in the validators array</description>
        /// </item>
        /// <item>
        /// <term>Timestamp</term>
        /// <description>block time stamp</description>
        /// </item>
        /// <item>
        /// <term>Data</term>
        /// <description>consensus message data</description>
        /// </item>
        /// <item>
        /// <term>1</term>
        /// <description>fixed value</description>
        /// </item>
        /// <item>
        /// <term>Witness</term>
        /// <description>Witness, the executable verification script</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">binary writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            ((IVerifiable)this).SerializeUnsigned(writer);
            writer.Write((byte)1); writer.Write(Witness);
        }


        /// <summary>
        /// Serialize the unsigned message. It includes the fields as follows:
        /// 序列化待签名数据
        /// <list type="bullet">
        /// <item>
        /// <term>Version</term>
        /// <description>consensus message version, current is zero</description>
        /// </item>
        /// <item>
        /// <term>PrevHash</term>
        /// <description>the previous block hash</description>
        /// </item>
        /// <item>
        /// <term>BlockIndex</term>
        /// <description>the proposal block index</description>
        /// </item>
        /// <item>
        /// <term>ValidatorIndex</term>
        /// <description>the sender(the Speaker or Delegates) index in the validators array</description>
        /// <description>共识节点编号</description>
        /// <description>共识节点编号the sender(the Speaker or Delegates) index in the validators array</description>
        /// </item>
        /// <item>
        /// <term>Timestamp</term>
        /// <description>block time stamp</description>
        /// </item>
        /// <item>
        /// <term>Data</term>
        /// <description>consensus message data</description>
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
        /// Verify this payload
        /// </summary>
        /// <remarks>
        /// 1) Check if BlockIndex is more than the snapshot.Height<br/>
        /// 2) Verify the witness script
        /// </remarks>
        /// <param name="snapshot">数据库快照</param>
        /// <returns>校验通过返回true，否则返回false</returns>
        public bool Verify(Snapshot snapshot)
        {
            if (BlockIndex <= snapshot.Height)
                return false;
            return this.VerifyWitnesses(snapshot);
        }
    }
}
