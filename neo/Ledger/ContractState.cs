﻿using Neo.IO;
using Neo.IO.Json;
using Neo.SmartContract;
using System.IO;
using System.Linq;

namespace Neo.Ledger
{
    /// <summary>
    /// 合约状态
    /// </summary>
    public class ContractState : StateBase, ICloneable<ContractState>
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
        /// 合约属性状态
        /// </summary>
        public ContractPropertyState ContractProperties;

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
        /// 合约描述
        /// </summary>
        public string Description;

        /// <summary>
        /// 是否包含存储空间
        /// </summary>
        public bool HasStorage => ContractProperties.HasFlag(ContractPropertyState.HasStorage);

        /// <summary>
        /// 是否动态调用
        /// </summary>
        public bool HasDynamicInvoke => ContractProperties.HasFlag(ContractPropertyState.HasDynamicInvoke);

        /// <summary>
        /// 是否可支付（保留功能）
        /// </summary>
        public bool Payable => ContractProperties.HasFlag(ContractPropertyState.Payable);

        private UInt160 _scriptHash;

        /// <summary>
        /// 合约脚本hash
        /// </summary>
        public UInt160 ScriptHash
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
        public override int Size => base.Size + Script.GetVarSize() + ParameterList.GetVarSize() + sizeof(ContractParameterType) + sizeof(bool) + Name.GetVarSize() + CodeVersion.GetVarSize() + Author.GetVarSize() + Email.GetVarSize() + Description.GetVarSize();

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>克隆对象</returns>
        ContractState ICloneable<ContractState>.Clone()
        {
            return new ContractState
            {
                Script = Script,
                ParameterList = ParameterList,
                ReturnType = ReturnType,
                ContractProperties = ContractProperties,
                Name = Name,
                CodeVersion = CodeVersion,
                Author = Author,
                Email = Email,
                Description = Description
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Script = reader.ReadVarBytes();
            ParameterList = reader.ReadVarBytes().Select(p => (ContractParameterType)p).ToArray();
            ReturnType = (ContractParameterType)reader.ReadByte();
            ContractProperties = (ContractPropertyState)reader.ReadByte();
            Name = reader.ReadVarString();
            CodeVersion = reader.ReadVarString();
            Author = reader.ReadVarString();
            Email = reader.ReadVarString();
            Description = reader.ReadVarString();
        }

        /// <summary>
        /// 从参数副本复制数据到此对象
        /// </summary>
        /// <param name="replica">参数副本</param>
        void ICloneable<ContractState>.FromReplica(ContractState replica)
        {
            Script = replica.Script;
            ParameterList = replica.ParameterList;
            ReturnType = replica.ReturnType;
            ContractProperties = replica.ContractProperties;
            Name = replica.Name;
            CodeVersion = replica.CodeVersion;
            Author = replica.Author;
            Email = replica.Email;
            Description = replica.Description;
        }


        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>Script</term>
        /// <description>脚本</description>
        /// </item>
        /// <item>
        /// <term>ParameterList</term>
        /// <description>参数列表</description>
        /// </item>
        /// <item>
        /// <term>ReturnType</term>
        /// <description>合约脚本返回值类型</description>
        /// </item>
        /// <item>
        /// <term>ContractProperties</term>
        /// <description>合约属性状态</description>
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
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteVarBytes(Script);
            writer.WriteVarBytes(ParameterList.Cast<byte>().ToArray());
            writer.Write((byte)ReturnType);
            writer.Write((byte)ContractProperties);
            writer.WriteVarString(Name);
            writer.WriteVarString(CodeVersion);
            writer.WriteVarString(Author);
            writer.WriteVarString(Email);
            writer.WriteVarString(Description);
        }


        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns>将这个ContractState转化成Json对象返回</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["hash"] = ScriptHash.ToString();
            json["script"] = Script.ToHexString();
            json["parameters"] = new JArray(ParameterList.Select(p => (JObject)p));
            json["returntype"] = ReturnType;
            json["name"] = Name;
            json["code_version"] = CodeVersion;
            json["author"] = Author;
            json["email"] = Email;
            json["description"] = Description;
            json["properties"] = new JObject();
            json["properties"]["storage"] = HasStorage;
            json["properties"]["dynamic_invoke"] = HasDynamicInvoke;
            return json;
        }
    }
}
