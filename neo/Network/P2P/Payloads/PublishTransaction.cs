using Neo.IO;
using Neo.IO.Json;
using Neo.Persistence;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 发布智能合约交易【已弃用】
    // </summary>
    /// <summary>
    /// A transaction for publishing smart contract(given up)
    /// </summary>
    [Obsolete]
    public class PublishTransaction : Transaction
    {
        // <summary>
        // 合约脚本
        // </summary>
        /// <summary>
        /// The transaction script
        /// </summary>
        public byte[] Script;

        // <summary>
        // 合约参数列表
        // </summary>
        /// <summary>
        /// The parameter list of contract
        /// </summary>
        public ContractParameterType[] ParameterList;

        // <summary>
        // 合约返回值类型
        // </summary>
        /// <summary>
        /// The return value type of contract
        /// </summary>
        public ContractParameterType ReturnType;

        // <summary>
        // 是否需要存储空间
        // </summary>
        /// <summary>
        /// If the contract need the storage
        /// </summary>
        public bool NeedStorage;

        // <summary>
        // 合约名字
        // </summary>
        /// <summary>
        /// The name of contract
        /// </summary>
        public string Name;

        // <summary>
        // 代码版本号
        // </summary>
        /// <summary>
        /// The version of code
        /// </summary>
        public string CodeVersion;

        // <summary>
        // 作者
        // </summary>
        /// <summary>
        /// The author
        /// </summary>
        public string Author;

        // <summary>
        // 邮箱
        // </summary>
        /// <summary>
        /// The email of authir
        /// </summary>
        public string Email;

        // <summary>
        // 描述
        // </summary>
        /// <summary>
        /// The description of the contract
        /// </summary>
        public string Description;

        private UInt160 _scriptHash;
        internal UInt160 ScriptHash
        {
            get
            {
                if (_scriptHash == null)
                {
                    _scriptHash = Script.ToScriptHash();
                }
                return _scriptHash;
            }
        }

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size for storage
        /// </summary>
        public override int Size => base.Size + Script.GetVarSize() + ParameterList.GetVarSize() + sizeof(ContractParameterType) + Name.GetVarSize() + CodeVersion.GetVarSize() + Author.GetVarSize() + Email.GetVarSize() + Description.GetVarSize();

        // <summary>
        // 创建智能合约发布交易
        // </summary>
        /// <summary>
        /// Construct the contract publication transaction
        /// </summary>
        public PublishTransaction()
            : base(TransactionType.PublishTransaction)
        {
        }

        // <summary>
        // 反序列化非data数据
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization of this transaction exclude the data
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            if (Version > 1) throw new FormatException();
            Script = reader.ReadVarBytes();
            ParameterList = reader.ReadVarBytes().Select(p => (ContractParameterType)p).ToArray();
            ReturnType = (ContractParameterType)reader.ReadByte();
            if (Version >= 1)
                NeedStorage = reader.ReadBoolean();
            else
                NeedStorage = false;
            Name = reader.ReadVarString(252);
            CodeVersion = reader.ReadVarString(252);
            Author = reader.ReadVarString(252);
            Email = reader.ReadVarString(252);
            Description = reader.ReadVarString(65536);
        }

        // <summary>
        // 序列化非data数据
        // <list type="bullet">
        // <item>
        // <term>Script</term>
        // <description>合约脚本</description>
        // </item>
        // <item>
        // <term>ParameterList</term>
        // <description>参数列表</description>
        // </item>
        // <item>
        // <term>ReturnType</term>
        // <description>返回值类型</description>
        // </item> 
        // <item>
        // <term>NeedStorage</term>
        // <description>是否需要存储（版本1开始有效）</description>
        // </item>
        // <item>
        // <term>Name</term>
        // <description>合约名字</description>
        // </item>
        // <item>
        // <term>CodeVersion</term>
        // <description>代码版本号</description>
        // </item>
        // <item>
        // <term>Author</term>
        // <description>作者</description>
        // </item>
        // <item>
        // <term>Email</term>
        // <description>邮箱</description>
        // </item>
        // <item>
        // <term>Description</term>
        // <description>描述</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>

        /// <summary>
        /// Serialization
        /// <list type="bullet">
        /// <item>
        /// <term>Script</term>
        /// <description>The smart contract script</description>
        /// </item>
        /// <item>
        /// <term>ParameterList</term>
        /// <description>The list of parameters</description>
        /// </item>
        /// <item>
        /// <term>ReturnType</term>
        /// <description>THe return value type</description>
        /// </item> 
        /// <item>
        /// <term>NeedStorage</term>
        /// <description>If it need a storage（valid from 1.0 version）</description>
        /// </item>
        /// <item>
        /// <term>Name</term>
        /// <description>The name of contract</description>
        /// </item>
        /// <item>
        /// <term>CodeVersion</term>
        /// <description>The code of version</description>
        /// </item>
        /// <item>
        /// <term>Author</term>
        /// <description>Author</description>
        /// </item>
        /// <item>
        /// <term>Email</term>
        /// <description>Email</description>
        /// </item>
        /// <item>
        /// <term>Description</term>
        /// <description>Description</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.WriteVarBytes(Script);
            writer.WriteVarBytes(ParameterList.Cast<byte>().ToArray());
            writer.Write((byte)ReturnType);
            if (Version >= 1) writer.Write(NeedStorage);
            writer.WriteVarString(Name);
            writer.WriteVarString(CodeVersion);
            writer.WriteVarString(Author);
            writer.WriteVarString(Email);
            writer.WriteVarString(Description);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns>json对象</returns>
        /// <summary>
        /// Transfer to json object
        /// </summary>
        /// <returns>json object</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["contract"] = new JObject();
            json["contract"]["code"] = new JObject();
            json["contract"]["code"]["hash"] = ScriptHash.ToString();
            json["contract"]["code"]["script"] = Script.ToHexString();
            json["contract"]["code"]["parameters"] = new JArray(ParameterList.Select(p => (JObject)p));
            json["contract"]["code"]["returntype"] = ReturnType;
            json["contract"]["needstorage"] = NeedStorage;
            json["contract"]["name"] = Name;
            json["contract"]["version"] = CodeVersion;
            json["contract"]["author"] = Author;
            json["contract"]["email"] = Email;
            json["contract"]["description"] = Description;
            return json;
        }


        // <summary>
        // 校验脚本。已经弃用。不接受新的PublishTransaction
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <param name="mempool">内存池交易</param>
        // <returns>返回固定值false，已弃用</returns>
        /// <summary>
        /// Verify the transaction script, which is deprecated. Not accept any new publishTransaction
        /// </summary>
        /// <param name="snapshot">The snapshot of dabase</param>
        /// <param name="mempool">The memory pool of transactions</param>
        /// <returns>return the fixed value false, which is deprecated</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            return false;
        }
    }
}
