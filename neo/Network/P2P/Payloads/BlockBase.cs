using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 区块基类
    /// </summary>
    public abstract class BlockBase : IVerifiable
    {
        /// <summary>
        /// 区块版本号
        /// </summary>
        public uint Version;
        
        /// <summary>
        /// 上一个区块hash
        /// </summary>
        public UInt256 PrevHash;

        /// <summary>
        /// 交易的梅克尔根
        /// </summary>
        public UInt256 MerkleRoot;

        /// <summary>
        /// 区块时间戳
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// 区块高度
        /// </summary>
        public uint Index;

        /// <summary>
        /// 共识附加数据，默认为block nonce
        /// </summary>
        public ulong ConsensusData;

        /// <summary>
        /// 下一个区块共识地址，为共识节点三分之二多方签名合约地址
        /// </summary>
        public UInt160 NextConsensus;

        /// <summary>
        /// 见证人
        /// </summary>
        public Witness Witness;

        private UInt256 _hash = null;

        /// <summary>
        /// 区块hash
        /// </summary>
        public UInt256 Hash
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
        /// 存储大小
        /// </summary>
        public virtual int Size => sizeof(uint) + PrevHash.Size + MerkleRoot.Size + sizeof(uint) + sizeof(uint) + sizeof(ulong) + NextConsensus.Size + 1 + Witness.Size;


        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Deserialize(BinaryReader reader)
        {
            ((IVerifiable)this).DeserializeUnsigned(reader);
            if (reader.ReadByte() != 1) throw new FormatException();
            Witness = reader.ReadSerializable<Witness>();
        }

        void IVerifiable.DeserializeUnsigned(BinaryReader reader)
        {
            Version = reader.ReadUInt32();
            PrevHash = reader.ReadSerializable<UInt256>();
            MerkleRoot = reader.ReadSerializable<UInt256>();
            Timestamp = reader.ReadUInt32();
            Index = reader.ReadUInt32();
            ConsensusData = reader.ReadUInt64();
            NextConsensus = reader.ReadSerializable<UInt160>();
        }

        byte[] IScriptContainer.GetMessage()
        {
            return this.GetHashData();
        }

        UInt160[] IVerifiable.GetScriptHashesForVerifying(Snapshot snapshot)
        {
            if (PrevHash == UInt256.Zero)
                return new[] { Witness.ScriptHash };
            Header prev_header = snapshot.GetHeader(PrevHash);
            if (prev_header == null) throw new InvalidOperationException();
            return new UInt160[] { prev_header.NextConsensus };
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>Version</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>PrevHash</term>
        /// <description>上一个区块hash</description>
        /// </item>
        /// <item>
        /// <term>MerkleRoot</term>
        /// <description>梅克尔树</description>
        /// </item>
        /// <item>
        /// <term>Timestamp</term>
        /// <description>时间戳</description>
        /// </item>
        /// <item>
        /// <term>Index</term>
        /// <description>区块高度</description>
        /// </item>
        /// <item>
        /// <term>ConsensusData</term>
        /// <description>共识数据，默认为block nonce</description>
        /// </item>
        /// <item>
        /// <term>NextConsensus</term>
        /// <description>下一个区块共识地址</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Serialize(BinaryWriter writer)
        {
            ((IVerifiable)this).SerializeUnsigned(writer);
            writer.Write((byte)1); writer.Write(Witness);
        }

        void IVerifiable.SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(PrevHash);
            writer.Write(MerkleRoot);
            writer.Write(Timestamp);
            writer.Write(Index);
            writer.Write(ConsensusData);
            writer.Write(NextConsensus);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public virtual JObject ToJson()
        {
            JObject json = new JObject();
            json["hash"] = Hash.ToString();
            json["size"] = Size;
            json["version"] = Version;
            json["previousblockhash"] = PrevHash.ToString();
            json["merkleroot"] = MerkleRoot.ToString();
            json["time"] = Timestamp;
            json["index"] = Index;
            json["nonce"] = ConsensusData.ToString("x16");
            json["nextconsensus"] = NextConsensus.ToAddress();
            json["script"] = Witness.ToJson();
            return json;
        }

        /// <summary>
        /// 根据当前区块快照，校验该区块
        /// </summary>
        /// <param name="snapshot">区块快照</param>
        /// <remark>
        /// 若满足以下4个条件之一，则验证节点为false；
        /// 1）若上一个区块不存在
        /// 2）若上一个区块高度加一不等于当前区块高度
        /// 3）若者上一个区块时间戳大于当前区块时间戳
        /// 4）若见证人校验失败
        /// </remark>
        public virtual bool Verify(Snapshot snapshot)
        {
            Header prev_header = snapshot.GetHeader(PrevHash);
            if (prev_header == null) return false;
            if (prev_header.Index + 1 != Index) return false;
            if (prev_header.Timestamp >= Timestamp) return false;
            if (!this.VerifyWitnesses(snapshot)) return false;
            return true;
        }
    }
}
