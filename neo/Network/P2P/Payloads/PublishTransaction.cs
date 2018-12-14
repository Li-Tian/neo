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
    /// <summary>
    /// 发布智能合约交易【已弃用】
    /// </summary>
    [Obsolete]
    public class PublishTransaction : Transaction
    {
        /// <summary>
        /// 合约脚本
        /// </summary>
        public byte[] Script;

        /// <summary>
        /// 合约参数列表
        /// </summary>
        public ContractParameterType[] ParameterList;

        /// <summary>
        /// 合约返回值类型
        /// </summary>
        public ContractParameterType ReturnType;

        /// <summary>
        /// 是否需要存储空间
        /// </summary>
        public bool NeedStorage;

        /// <summary>
        /// 合约名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 代码版本号
        /// </summary>
        public string CodeVersion;

        /// <summary>
        /// 作者
        /// </summary>
        public string Author;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email;

        /// <summary>
        /// 描述
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

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + Script.GetVarSize() + ParameterList.GetVarSize() + sizeof(ContractParameterType) + Name.GetVarSize() + CodeVersion.GetVarSize() + Author.GetVarSize() + Email.GetVarSize() + Description.GetVarSize();

        /// <summary>
        /// 创建智能合约发布交易
        /// </summary>
        public PublishTransaction()
            : base(TransactionType.PublishTransaction)
        {
        }

        /// <summary>
        /// 反序列化非data数据
        /// </summary>
        /// <param name="reader">二进制输入流</param>
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

        /// <summary>
        /// 序列化非data数据
        /// <list type="bullet">
        /// <item>
        /// <term>Script</term>
        /// <description>合约脚本</description>
        /// </item>
        /// <item>
        /// <term>ParameterList</term>
        /// <description>参数列表</description>
        /// </item>
        /// <item>
        /// <term>ReturnType</term>
        /// <description>返回值类型</description>
        /// </item> 
        /// <item>
        /// <term>NeedStorage</term>
        /// <description>是否需要存储（版本1开始有效）</description>
        /// </item>
        /// <item>
        /// <term>Name</term>
        /// <description>合约名字</description>
        /// </item>
        /// <item>
        /// <term>CodeVersion</term>
        /// <description>代码版本号</description>
        /// </item>
        /// <item>
        /// <term>Author</term>
        /// <description>作者</description>
        /// </item>
        /// <item>
        /// <term>Email</term>
        /// <description>邮箱</description>
        /// </item>
        /// <item>
        /// <term>Description</term>
        /// <description>描述</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
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

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns>json对象</returns>
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


        /// <summary>
        /// 校验脚本。已经弃用。不接受新的PublishTransaction
        /// </summary>
        /// <param name="snapshot">数据库快照</param>
        /// <param name="mempool">内存池交易</param>
        /// <returns>返回固定值false，已弃用</returns>
        public override bool Verify(Snapshot snapshot, IEnumerable<Transaction> mempool)
        {
            return false;
        }
    }
}
