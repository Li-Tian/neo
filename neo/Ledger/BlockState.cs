using Neo.IO;
using Neo.IO.Json;
using System.IO;

namespace Neo.Ledger
{
    /// <summary>
    /// 区块状态
    /// </summary>
    public class BlockState : StateBase, ICloneable<BlockState>
    {
        /// <summary>
        /// 截止当前块（包括当前块）所有系统手续费总和
        /// </summary>
        public long SystemFeeAmount;

        /// <summary>
        /// 简化版的block
        /// </summary>
        public TrimmedBlock TrimmedBlock;

        /// <summary>
        /// 存储大小
        /// </summary>
        public override int Size => base.Size + sizeof(long) + TrimmedBlock.Size;

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>克隆的对象</returns>
        BlockState ICloneable<BlockState>.Clone()
        {
            return new BlockState
            {
                SystemFeeAmount = SystemFeeAmount,
                TrimmedBlock = TrimmedBlock
            };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">二进制输出流</param>
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            SystemFeeAmount = reader.ReadInt64();
            TrimmedBlock = reader.ReadSerializable<TrimmedBlock>();
        }

        void ICloneable<BlockState>.FromReplica(BlockState replica)
        {
            SystemFeeAmount = replica.SystemFeeAmount;
            TrimmedBlock = replica.TrimmedBlock;
        }

        /// <summary>
        /// 序列化
        /// <list type="bullet">
        /// <item>
        /// <term>StateVersion</term>
        /// <description>状态版本号</description>
        /// </item>
        /// <item>
        /// <term>SystemFeeAmount</term>
        /// <description>系统所有手续费</description>
        /// </item>
        /// <item>
        /// <term>TrimmedBlock</term>
        /// <description>简化版block</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(SystemFeeAmount);
            writer.Write(TrimmedBlock);
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns>json数据对象</returns>
        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["sysfee_amount"] = SystemFeeAmount.ToString();
            json["trimmed"] = TrimmedBlock.ToJson();
            return json;
        }
    }
}
