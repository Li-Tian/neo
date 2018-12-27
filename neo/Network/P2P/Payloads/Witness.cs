using Neo.IO;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.VM;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 见证人。
    // 验证时先读取验证脚本(VerificationScript)压入堆栈，
    // 然后再读取执行脚本(InvocationScript)并压入堆栈，
    // 然后执行并判定结果。
    // </summary>
    /// <summary>
    /// Witness
    /// When verifying, first read the verification script (VerificationScript)and push it onto the stack.
    /// Then read the execution script (InvocationScript) and push it onto the stack.
    /// Then execute and determine the result.
    /// </summary>
    public class Witness : ISerializable
    {
        // <summary>
        // 执行脚本，补全参数
        // </summary>
        /// <summary>
        /// Invocation script，push the required data into the stack
        /// </summary>
        public byte[] InvocationScript;

        // <summary>
        // 验证脚本
        // </summary>
        /// <summary>
        /// Verification scripts
        /// </summary>
        public byte[] VerificationScript;

        private UInt160 _scriptHash;

        // <summary>
        // 验证脚本的哈希
        // </summary>
        /// <summary>
        /// Verification scripts hash
        /// </summary>
        public virtual UInt160 ScriptHash
        {
            get
            {
                if (_scriptHash == null)
                {
                    _scriptHash = VerificationScript.ToScriptHash();
                }
                return _scriptHash;
            }
        }

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// size
        /// </summary>
        public int Size => InvocationScript.GetVarSize() + VerificationScript.GetVarSize();

        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入</param>
        /// <summary>
        /// Deserialize method
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            InvocationScript = reader.ReadVarBytes(65536);
            VerificationScript = reader.ReadVarBytes(65536);
        }
        // <summary>
        // 序列化
        // </summary>
        // <param name="writer">二进制输出</param>
        /// <summary>
        /// Serialize method
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.WriteVarBytes(InvocationScript);
            writer.WriteVarBytes(VerificationScript);
        }

        // <summary>
        // 转成json对象
        // </summary>
        // <returns></returns>
        /// <summary>
        /// Convert to JObject object
        /// </summary>
        /// <returns>JObject object</returns>
        public JObject ToJson()
        {
            JObject json = new JObject();
            json["invocation"] = InvocationScript.ToHexString();
            json["verification"] = VerificationScript.ToHexString();
            return json;
        }
    }
}
