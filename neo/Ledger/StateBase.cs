using Neo.IO;
using Neo.IO.Json;
using System;
using System.IO;

namespace Neo.Ledger
{
    // <summary>
    // 状态基类
    // </summary>
    /// <summary>
    /// The base class for state
    /// </summary>
    public abstract class StateBase : ISerializable
    {
        // <summary>
        // 状态版本号，固定为0
        // </summary>
        /// <summary>
        /// The version of state, which is 0
        /// </summary>
        public const byte StateVersion = 0;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The storage of this object
        /// </summary>
        public virtual int Size => sizeof(byte);

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        public virtual void Deserialize(BinaryReader reader)
        {
            if (reader.ReadByte() != StateVersion) throw new FormatException();
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // </list>
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// Serilization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The version of state</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(StateVersion);
        }

        // <summary>
        // 转成Json对象.添入状态版本号.
        // </summary>
        // <returns>返回一个包含状态版本号的JObject</returns>
        /// <summary>
        /// Transfer this object to json and insert the version number of state
        /// </summary>
        /// <returns>return a Json object which contains version of state</returns>
        public virtual JObject ToJson()
        {
            JObject json = new JObject();
            json["version"] = StateVersion;
            return json;
        }
    }
}
