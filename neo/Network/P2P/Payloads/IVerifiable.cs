using Neo.IO;
using Neo.Persistence;
using Neo.VM;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 封装的待签名验证接口
    /// </summary>
    public interface IVerifiable : ISerializable, IScriptContainer
    {
        /// <summary>
        /// 见证人列表
        /// </summary>
        Witness[] Witnesses { get; set; }

        /// <summary>
        /// 反序列化未签名数据
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        void DeserializeUnsigned(BinaryReader reader);


        /// <summary>
        /// 获取验证脚本hash集合
        /// </summary>
        /// <param name="snapshot">数据库快照</param>
        /// <returns>验证脚本hash集合</returns>
        UInt160[] GetScriptHashesForVerifying(Snapshot snapshot);

        /// <summary>
        /// 序列化未签名数据
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        void SerializeUnsigned(BinaryWriter writer);
    }
}
