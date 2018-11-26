using Neo.IO;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.IO;
using System.Linq;

namespace Neo.Wallets.SQLite
{
    /// <summary>
    /// VerificationContract是SmartContract.Contract的子类
    /// 用于描述智能合约中鉴权合约的方法和属性
    /// </summary>
    public class VerificationContract : SmartContract.Contract, IEquatable<VerificationContract>, ISerializable
    {
        /// <summary>
        /// 鉴权合约的大小，其大小是20+参数列表大小+合约脚本的大小
        /// </summary>
        public int Size => 20 + ParameterList.GetVarSize() + Script.GetVarSize();
        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        public void Deserialize(BinaryReader reader)
        {
            reader.ReadSerializable<UInt160>();
            ParameterList = reader.ReadVarBytes().Select(p => (ContractParameterType)p).ToArray();
            Script = reader.ReadVarBytes();
        }
        /// <summary>
        /// 判断是否等于另一个鉴权合约对象，通过两者的脚本哈希比较
        /// </summary>
        /// <param name="other">另一个鉴权合约对象</param>
        /// <returns>相等则返回true,否则返回false</returns>
        public bool Equals(VerificationContract other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return ScriptHash.Equals(other.ScriptHash);
        }
        /// <summary>
        /// 判断是否等于另一个对象
        /// </summary>
        /// <param name="obj">待比较对象</param>
        /// <returns>等于返回true,否则返回false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as VerificationContract);
        }
        /// <summary>
        /// 获取脚本哈希的hashcode(脚本哈希的前4个字节)
        /// </summary>
        /// <returns>脚本哈希的hashcode</returns>
        public override int GetHashCode()
        {
            return ScriptHash.GetHashCode();
        }
        /// <summary>
        /// 序列化方法
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(new UInt160());
            writer.WriteVarBytes(ParameterList.Select(p => (byte)p).ToArray());
            writer.WriteVarBytes(Script);
        }
    }
}
