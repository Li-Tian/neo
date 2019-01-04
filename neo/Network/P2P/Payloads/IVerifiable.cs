using Neo.IO;
using Neo.Persistence;
using Neo.VM;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 封装的待签名验证接口
    // </summary>
    /// <summary>
    /// An interface for signature verification
    /// </summary>
    public interface IVerifiable : ISerializable, IScriptContainer
    {
        // <summary>
        // 见证人列表
        // </summary>
        /// <summary>
        /// Witness list
        /// </summary>
        Witness[] Witnesses { get; set; }

        // <summary>
        // 反序列化待签名数据
        // </summary>
        // <param name="reader">2进制读取器</param>
        /// <summary>
        /// Deserialize unsigned data
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        void DeserializeUnsigned(BinaryReader reader);


        // <summary>
        // 获取等待签名验证的脚本hash集合
        // </summary>
        // <param name="snapshot">数据库快照</param>
        // <returns>验证脚本hash集合</returns>
        /// <summary>
        /// Get the script hash collection waiting for signature verification
        /// </summary>
        /// <param name="snapshot">Snapshot</param>
        /// <returns>the script hash collection waiting for signature verification</returns>
        UInt160[] GetScriptHashesForVerifying(Snapshot snapshot);

        // <summary>
        // 序列化待签名数据
        // </summary>
        // <param name="writer">2进制输出器</param>
        /// <summary>
        /// Serialize unsigned data
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        void SerializeUnsigned(BinaryWriter writer);
    }
}
