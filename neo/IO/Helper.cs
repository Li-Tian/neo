using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Neo.IO
{
    /// <summary>
    /// IO读取的辅助方法类
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// 将2进制数据流反序列化出一个指定类型的对象
        /// </summary>
        /// <typeparam name="T">指定的数据类型</typeparam>
        /// <param name="value">2进制数据流</param>
        /// <param name="start">读取数据流的起始位置</param>
        /// <returns>反序列化出一个可序列化对象</returns>
        public static T AsSerializable<T>(this byte[] value, int start = 0) where T : ISerializable, new()
        {
            using (MemoryStream ms = new MemoryStream(value, start, value.Length - start, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                return reader.ReadSerializable<T>();
            }
        }
        /// <summary>
        /// 将2进制数据流反序列化出一个指定数据类型的可序列化对象，
        /// 同时判定指定数据类型是否实现了序列化接口
        /// </summary>
        /// <param name="value">2进制数据流</param>
        /// <param name="type">指定数据类型</param>
        /// <returns>可序列化对象</returns>
        /// <exception cref="System.InvalidCastException">指定数据类型未实现序列化接口时抛出</exception>
        public static ISerializable AsSerializable(this byte[] value, Type type)
        {
            if (!typeof(ISerializable).GetTypeInfo().IsAssignableFrom(type))
                throw new InvalidCastException();
            ISerializable serializable = (ISerializable)Activator.CreateInstance(type);
            using (MemoryStream ms = new MemoryStream(value, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                serializable.Deserialize(reader);
            }
            return serializable;
        }
        /// <summary>
        /// 将2进制数据流反序列化出一个指定数据类型的可序列化对象数组
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="value">2进制数据流</param>
        /// <param name="max">对象数组最大容量，默认最大容量为16777216</param>
        /// <returns>可序列化对象数组</returns>
        public static T[] AsSerializableArray<T>(this byte[] value, int max = 0x1000000) where T : ISerializable, new()
        {
            using (MemoryStream ms = new MemoryStream(value, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                return reader.ReadSerializableArray<T>(max);
            }
        }

        internal static int GetVarSize(int value)
        {
            if (value < 0xFD)
                return sizeof(byte);
            else if (value <= 0xFFFF)
                return sizeof(byte) + sizeof(ushort);
            else
                return sizeof(byte) + sizeof(uint);
        }

        internal static int GetVarSize<T>(this T[] value)
        {
            int value_size;
            Type t = typeof(T);
            if (typeof(ISerializable).IsAssignableFrom(t))
            {
                value_size = value.OfType<ISerializable>().Sum(p => p.Size);
            }
            else if (t.GetTypeInfo().IsEnum)
            {
                int element_size;
                Type u = t.GetTypeInfo().GetEnumUnderlyingType();
                if (u == typeof(sbyte) || u == typeof(byte))
                    element_size = 1;
                else if (u == typeof(short) || u == typeof(ushort))
                    element_size = 2;
                else if (u == typeof(int) || u == typeof(uint))
                    element_size = 4;
                else //if (u == typeof(long) || u == typeof(ulong))
                    element_size = 8;
                value_size = value.Length * element_size;
            }
            else
            {
                value_size = value.Length * Marshal.SizeOf<T>();
            }
            return GetVarSize(value.Length) + value_size;
        }

        internal static int GetVarSize(this string value)
        {
            int size = Encoding.UTF8.GetByteCount(value);
            return GetVarSize(size) + size;
        }
        /// <summary>
        /// 从2进制读取器读取分组后的数据，并按分组规则恢复分组前数据后输出
        /// 分组规则：
        /// 1、读取16字节数据，再读取一字节数据表示0x00的补充个数
        /// 2、去除16字节数据中补充的0
        /// 3、循环步骤1、2，直到数据全部读完后，输出恢复后的数据
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        /// <returns>恢复后的数据</returns>
        /// <exception cref="System.FormatException">数据无法按分组规则恢复时抛出</exception>
        public static byte[] ReadBytesWithGrouping(this BinaryReader reader)
        {
            const int GROUP_SIZE = 16;
            using (MemoryStream ms = new MemoryStream())
            {
                int padding = 0;
                do
                {
                    byte[] group = reader.ReadBytes(GROUP_SIZE);
                    padding = reader.ReadByte();
                    if (padding > GROUP_SIZE)
                        throw new FormatException();
                    int count = GROUP_SIZE - padding;
                    if (count > 0)
                        ms.Write(group, 0, count);
                } while (padding == 0);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// 从2进制读取器中读取限定长度的字符串
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        /// <param name="length">限定长度</param>
        /// <returns>读取的字符串</returns>
        public static string ReadFixedString(this BinaryReader reader, int length)
        {
            byte[] data = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(data.TakeWhile(p => p != 0).ToArray());
        }
        /// <summary>
        /// 读取2进制读取器的数据反序列化出一个可序列化的指定数据类型的对象
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="reader">2进制读取器</param>
        /// <returns>一个可序列化的指定数据类型的对象</returns>
        public static T ReadSerializable<T>(this BinaryReader reader) where T : ISerializable, new()
        {
            T obj = new T();
            obj.Deserialize(reader);
            return obj;
        }
        /// <summary>
        /// 读取2进制读取器的数据反序列化出一个可序列化的指定数据类型的对象数组
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="reader">2进制读取器</param>
        /// <param name="max">数组最大容量，默认最大容量为16777216</param>
        /// <returns>一个可序列化的指定数据类型的对象数组</returns>
        public static T[] ReadSerializableArray<T>(this BinaryReader reader, int max = 0x1000000) where T : ISerializable, new()
        {
            T[] array = new T[reader.ReadVarInt((ulong)max)];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new T();
                array[i].Deserialize(reader);
            }
            return array;
        }
        /// <summary>
        /// 从2进制读取器中读取一个限定大小的数组数据
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        /// <param name="max">最大可读取数</param>
        /// <returns>读取的数组数据</returns>
        public static byte[] ReadVarBytes(this BinaryReader reader, int max = 0x1000000)
        {
            return reader.ReadBytes((int)reader.ReadVarInt((ulong)max));
        }
        /// <summary>
        /// 从2进制读取器中读取一个限定大小的长整型数据
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        /// <param name="max">最大可读取数</param>
        /// <returns>读取的长整型数据</returns>
        /// <exception cref="System.FormatException">读取的长整型数据的数值大于最大可读取数时抛出</exception>
        public static ulong ReadVarInt(this BinaryReader reader, ulong max = ulong.MaxValue)
        {
            byte fb = reader.ReadByte();
            ulong value;
            if (fb == 0xFD)
                value = reader.ReadUInt16();
            else if (fb == 0xFE)
                value = reader.ReadUInt32();
            else if (fb == 0xFF)
                value = reader.ReadUInt64();
            else
                value = fb;
            if (value > max) throw new FormatException();
            return value;
        }
        /// <summary>
        /// 从2进制读取器中读取一个限定长度的字符串数据
        /// </summary>
        /// <param name="reader">2进制读取器</param>
        /// <param name="max">限定长度</param>
        /// <returns>读取的的字符串数据</returns>
        public static string ReadVarString(this BinaryReader reader, int max = 0x1000000)
        {
            return Encoding.UTF8.GetString(reader.ReadVarBytes(max));
        }
        /// <summary>
        /// 将可序列化对象序列化成一个字节数组
        /// </summary>
        /// <param name="value">可序列化对象</param>
        /// <returns>序列化成的字节数组</returns>
        public static byte[] ToArray(this ISerializable value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                value.Serialize(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }
        /// <summary>
        /// 将指定数据类型的可序列化对象数组序列化成一个字节数组
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="value">可序列化对象数组</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] ToByteArray<T>(this T[] value) where T : ISerializable
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                writer.Write(value);
                writer.Flush();
                return ms.ToArray();
            }
        }
        /// <summary>
        /// 将可序列化对象序列化
        /// </summary>
        /// <param name="writer">二进制输出器</param>
        /// <param name="value">可序列化对象</param>
        public static void Write(this BinaryWriter writer, ISerializable value)
        {
            value.Serialize(writer);
        }
        /// <summary>
        /// 将指定数据类型的可序列化对象数组序列化
        /// </summary>
        /// <typeparam name="T">指定的数据类型</typeparam>
        /// <param name="writer">二进制输出器</param>
        /// <param name="value">可序列化对象数组</param>
        public static void Write<T>(this BinaryWriter writer, T[] value) where T : ISerializable
        {
            writer.WriteVarInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                value[i].Serialize(writer);
            }
        }
        /// <summary>
        /// 将字节数组数据按分组规则分组后输出
        /// 分组规则执行步骤：
        /// 1、取字节数组中16个字节数据输出，并在之后输出1个0x00
        /// 2、循环步骤1，直到字节数组中未输出数据不足16个字节时，计算16-未输出数据字节个数的差值padding。
        /// 3、将字节数组中未输出数据输出，并在其之后输出padding个0x00
        /// 4、最后输出1字节等于padding数值的数据
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        /// <param name="value">字节数组数据</param>
        public static void WriteBytesWithGrouping(this BinaryWriter writer, byte[] value)
        {
            const int GROUP_SIZE = 16;
            int index = 0;
            int remain = value.Length;
            while (remain >= GROUP_SIZE)
            {
                writer.Write(value, index, GROUP_SIZE);
                writer.Write((byte)0);
                index += GROUP_SIZE;
                remain -= GROUP_SIZE;
            }
            if (remain > 0)
                writer.Write(value, index, remain);
            int padding = GROUP_SIZE - remain;
            for (int i = 0; i < padding; i++)
                writer.Write((byte)0);
            writer.Write((byte)padding);
        }
        /// <summary>
        /// 将字符串按限定长度序列化输出，不足固定长度的数据会在之后补0x00.
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        /// <param name="value">字符串数据</param>
        /// <param name="length">限定长度</param>
        /// <exception cref="System.ArgumentException">字符串长度和序列化长度超过限定长度后抛出</exception>
        public static void WriteFixedString(this BinaryWriter writer, string value, int length)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value.Length > length)
                throw new ArgumentException();
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > length)
                throw new ArgumentException();
            writer.Write(bytes);
            if (bytes.Length < length)
                writer.Write(new byte[length - bytes.Length]);
        }
        /// <summary>
        /// 将字节数组形式的数据按一定的格式序列化输出，输出格式为字节数组长度+字节数组的数据
        /// </summary>
        /// <param name="writer">二进制输出器</param>
        /// <param name="value">待序列化的字节数组</param>
        public static void WriteVarBytes(this BinaryWriter writer, byte[] value)
        {
            writer.WriteVarInt(value.Length);
            writer.Write(value);
        }
        /// <summary>
        /// 将长整形数据按一定的格式序列化输出
        /// 输出格式为：
        /// 1）value小于0xFD，用一个字节的形式输出
        /// 2）value小于等于0xFFFF，用0xFD+字节数组的形式输出
        /// 3）value小于等于0xFFFFFFFF，用0xFF+字节数组的形式输出
        /// </summary>
        /// <param name="writer">2进制输出器</param>
        /// <param name="value">长整形数据</param>
        /// <exception cref="System.ArgumentException">长整形数据小于0时抛出</exception>
        public static void WriteVarInt(this BinaryWriter writer, long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException();
            if (value < 0xFD)
            {
                writer.Write((byte)value);
            }
            else if (value <= 0xFFFF)
            {
                writer.Write((byte)0xFD);
                writer.Write((ushort)value);
            }
            else if (value <= 0xFFFFFFFF)
            {
                writer.Write((byte)0xFE);
                writer.Write((uint)value);
            }
            else
            {
                writer.Write((byte)0xFF);
                writer.Write(value);
            }
        }
        /// <summary>
        /// 将字符串序列化输出
        /// </summary>
        /// <param name="writer">二进制输出器</param>
        /// <param name="value">待序列化的字符串</param>
        public static void WriteVarString(this BinaryWriter writer, string value)
        {
            writer.WriteVarBytes(Encoding.UTF8.GetBytes(value));
        }
    }
}
