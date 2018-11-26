using System.IO;

namespace Neo.IO
{
    /// <summary>
    /// 对象的2进制格式序列化和反序列化方法接口
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// 2进制格式序列化的大小
        /// </summary>
        int Size { get; }
        /// <summary>
        /// 对象的2进制格式序列化方法
        /// </summary>
        /// <param name="writer">二进制输出器</param>
        void Serialize(BinaryWriter writer);
        /// <summary>
        /// 对象的2进制格式反序列化方法
        /// </summary>
        /// <param name="reader">二进制读入器</param>
        void Deserialize(BinaryReader reader);
    }
}
