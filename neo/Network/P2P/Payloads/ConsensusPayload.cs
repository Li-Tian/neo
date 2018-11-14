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
    /// p2p consensus message payload
    /// </summary>
    public class ConsensusPayload : IInventory
    {
        /// <summary>
        /// consensus message version, current is zero
        /// </summary>
        public uint Version;

        /// <summary>
        /// the previous block hash
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// the proposal block index
        /// </summary>
        public uint BlockIndex;


        /// <summary>
        /// the sender(the Speaker or Delegates) index in the validators array
        /// </summary>
        public ushort ValidatorIndex;
        
        /// <summary>
        /// block time stamp
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// consensus message data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// witness, the executable verification script
        /// </summary>
        public Witness Witness;

        private UInt256 _hash = null;

        /// <summary>
        /// hash of the GetHashData( = unsigned data) by using hash256 algorithm
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
        /// p2p inventory message type, here is InventoryType.Consensus
        /// </summary>
        InventoryType IInventory.InventoryType => InventoryType.Consensus;

        /// <summary>
        /// witnesses, the executable verification scripts
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
        /// deserialize from the reader
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
        /// <term>Witness</term>
        /// <description> </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="reader">binary reader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            ((IVerifiable)this).DeserializeUnsigned(reader);
            if (reader.ReadByte() != 1) throw new FormatException();
            Witness = reader.ReadSerializable<Witness>();
        }

        /// <summary>
        /// deserialize from the reader without the witness field
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
        /// </list>
        /// </summary>
        /// <param name="reader">binary reader</param>
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
        /// script message, here is unsigned data
        /// </summary>
        /// <returns>unsigned data</returns>
        byte[] IScriptContainer.GetMessage()
        {
            return this.GetHashData();
        }

        /// <summary>
        /// get the script hash of the sender's signature contract
        /// </summary>
        /// <param name="snapshot">blockchain snapshot</param>
        /// <returns>script hash</returns>
        UInt160[] IVerifiable.GetScriptHashesForVerifying(Snapshot snapshot)
        {
            ECPoint[] validators = snapshot.GetValidators();
            if (validators.Length <= ValidatorIndex)
                throw new InvalidOperationException();
            return new[] { Contract.CreateSignatureRedeemScript(validators[ValidatorIndex]).ToScriptHash() };
        }

        /// <summary>
        /// serialize the message
        /// </summary>
        /// <param name="writer">binary writer</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            ((IVerifiable)this).SerializeUnsigned(writer);
            writer.Write((byte)1); writer.Write(Witness);
        }

        /// <summary>
        /// serialize the message without the witness field. It includes the fields as follows:
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
        /// verify the message. it include two step: 1) check the BlockIndex is more than the snapshot.Height, 2) verify the witness script
        /// </summary>
        /// <param name="snapshot">blockchain snapshot</param>
        /// <returns></returns>
        public bool Verify(Snapshot snapshot)
        {
            if (BlockIndex <= snapshot.Height)
                return false;
            return this.VerifyWitnesses(snapshot);
        }
    }
}
