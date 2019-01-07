using Neo.IO;
using System.IO;

namespace Neo.Ledger
{

    // <summary>
    // 合约存储项目的数据结构
    // </summary>
    /// <summary>
    /// The data structure of contract storage item
    /// </summary>
    public class StorageItem : StateBase, ICloneable<StorageItem>
    {
        // <summary>
        // 存储的具体值
        // </summary>
        /// <summary>
        /// The value of this item
        /// </summary>
        public byte[] Value;

        // <summary>
        // 是否是常量
        // </summary>
        /// <summary>
        /// Is it constant
        /// </summary>
        public bool IsConstant;

        // <summary>
        // 存储大小
        // </summary>
        /// <summary>
        /// The size of storage of this item
        /// </summary>
        public override int Size => base.Size + Value.GetVarSize() + sizeof(bool);

        // <summary>
        // 克隆
        // </summary>
        // <returns>克隆对象</returns>
        /// <summary>
        /// The clone method
        /// </summary>
        /// <returns>replica of object</returns>
        StorageItem ICloneable<StorageItem>.Clone()
        {
            return new StorageItem
            {
                Value = Value,
                IsConstant = IsConstant
            };
        }


        // <summary>
        // 反序列化
        // </summary>
        // <param name="reader">二进制输入流</param>
        /// <summary>
        /// Deserialization
        /// </summary>
        /// <param name="reader">The binary input reader</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Value = reader.ReadVarBytes();
            IsConstant = reader.ReadBoolean();
        }
        // <summary>
        // 从副本复制
        // </summary>
        // <param name="replica">副本</param>
        /// <summary>
        /// Copy method which copy from replication
        /// </summary>
        /// <param name="replica">The replication</param>
        void ICloneable<StorageItem>.FromReplica(StorageItem replica)
        {
            Value = replica.Value;
            IsConstant = replica.IsConstant;
        }

        // <summary>
        // 序列化
        // <list type="bullet">
        // <item>
        // <term>StateVersion</term>
        // <description>状态版本号</description>
        // </item>
        // <item>
        // <term>Value</term>
        // <description>存储的具体值</description>
        // </item>
        // <item>
        // <term>IsConstant</term>
        // <description>是否是常量</description>
        // </item>
        // </list> 
        // </summary>
        // <param name="writer">二进制输出流</param>
        /// <summary>
        /// The serialization
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>The version of state</description>
        /// </item>
        /// <item>
        /// <term>Value</term>
        /// <description>The value of storage item</description>
        /// </item>
        /// <item>
        /// <term>IsConstant</term>
        /// <description>Is this constant</description>
        /// </item>
        /// </list> 
        /// </summary>
        /// <param name="writer">The binary output writer</param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteVarBytes(Value);
            writer.Write(IsConstant);
        }
    }
}
