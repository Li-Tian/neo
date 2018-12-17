using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// 切片构建器
    /// </summary>
    public class SliceBuilder
    {
        private List<byte> data = new List<byte>();

        private SliceBuilder()
        {
        }

        /// <summary>
        /// 添加byte数据到构建器中
        /// </summary>
        /// <param name="value">需要添加的byte数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(byte value)
        {
            data.Add(value);
            return this;
        }

        /// <summary>
        /// 添加ushort数据到构建器中
        /// </summary>
        /// <param name="value">需要添加的ushort数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(ushort value)
        {
            data.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        /// <summary>
        /// 添加uint数据到构建器中
        /// </summary>
        /// <param name="value">需要添加的uint数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(uint value)
        {
            data.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        /// <summary>
        /// 添加long数据到构建器中
        /// </summary>
        /// <param name="value">需要添加的long数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(long value)
        {
            data.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        /// <summary>
        /// 将字节序列数据添加到构建器中
        /// </summary>
        /// <param name="value">需要添加的字节序列数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(IEnumerable<byte> value)
        {
            data.AddRange(value);
            return this;
        }

        /// <summary>
        /// 将string数据添加到构建器中
        /// </summary>
        /// <param name="value">需要添加的string数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(string value)
        {
            data.AddRange(Encoding.UTF8.GetBytes(value));
            return this;
        }

        /// <summary>
        /// 将可序列化的数据添加到构建器中
        /// </summary>
        /// <param name="value">需要添加的可序列化的数据</param>
        /// <returns>当前的切片构建器</returns>
        public SliceBuilder Add(ISerializable value)
        {
            data.AddRange(value.ToArray());
            return this;
        }

        /// <summary>
        /// 无参的构造器创建
        /// </summary>
        /// <returns>Slice构造器</returns>
        public static SliceBuilder Begin()
        {
            return new SliceBuilder();
        }

        /// <summary>
        /// 创建构造器
        /// </summary>
        /// <param name="prefix">给定某前缀</param>
        /// <returns>创建的构造器对象</returns>
        public static SliceBuilder Begin(byte prefix)
        {
            return new SliceBuilder().Add(prefix);
        }


        /// <summary>
        /// 将Slice构造器转成Slice对象
        /// </summary>
        /// <param name="value">Slice构造器</param>
        public static implicit operator Slice(SliceBuilder value)
        {
            return value.data.ToArray();
        }
    }
}
