using Neo.IO;
using Neo.IO.Json;
using System;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 状态基类
    /// </summary>
    public abstract class StateBase : ISerializable
    {
        /// <summary>
        /// 状态版本号，固定为0
        /// </summary>
        public const byte StateVersion = 0;

        /// <summary>
        /// 存储大小
        /// </summary>
        public virtual int Size => sizeof(byte);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输入流</param>
        public virtual void Deserialize(BinaryReader reader)
        {
            if (reader.ReadByte() != StateVersion) throw new FormatException();
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer">二进制输出流</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(StateVersion);
        }

        /// <summary>
        /// 转成Json对象
        /// </summary>
        /// <returns>JObject</returns>
        public virtual JObject ToJson()
        {
            JObject json = new JObject();
            json["version"] = StateVersion;
            return json;
        }
    }
}
