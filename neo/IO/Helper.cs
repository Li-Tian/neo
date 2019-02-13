using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Neo.IO
{
    // <summary>
    // IO读写的辅助方法类
    // </summary>
    /// <summary>
    /// IO read and write helper class
    /// </summary>
    public static class Helper
    {
        // <summary>
        // 将2进制数据流反序列化出一个指定类型的对象
        // </summary>
        // <typeparam name="T">指定的数据类型</typeparam>
        // <param name="value">2进制数据流</param>
        // <param name="start">读取数据流的起始位置</param>
        // <returns>反序列化出一个可序列化对象</returns>
        /// <summary>
        /// Deserialize a byte array as a specified type of object,and the object should implement a serializable interface
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="value">a byte array</param>
        /// <param name="start">read the starting position of the data</param>
        /// <returns>a serializable object</returns>
        public static T AsSerializable<T>(this byte[] value, int start = 0) where T : ISerializable, new()
        {
            using (MemoryStream ms = new MemoryStream(value, start, value.Length - start, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                return reader.ReadSerializable<T>();
            }
        }
        // <summary>
        // 将2进制数据流反序列化出一个指定数据类型的可序列化对象，
        // </summary>
        // <param name="value">a byte array</param>
        // <param name="type">specified type</param>
        // <returns>可序列化对象</returns>
        // <exception cref="System.InvalidCastException">指定数据类型未实现序列化接口时抛出</exception>
        /// <summary>
        /// Deserialize a byte array as a specified type of object,and the object should implement a serializable interface
        /// </summary>
        /// <param name="value">a byte array</param>
        /// <param name="type">specified type</param>
        /// <returns>a serializable object</returns>
        /// <exception cref="System.InvalidCastException">
        /// the object did not implement a serializable interface
        /// </exception>
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
        // <summary>
        // 将2进制数据流反序列化出一个指定数据类型的可序列化对象数组
        // </summary>
        // <typeparam name="T">指定数据类型</typeparam>
        // <param name="value">2进制数据流</param>
        // <param name="max">对象数组最大容量，默认最大容量为16777216</param>
        // <returns>可序列化对象数组</returns>
        /// <summary>
        /// Deserialize a byte array as an array of serializable objects of specified type
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="value">a byte array</param>
        /// <param name="max">the max size of the array，default value is 16777216</param>
        /// <returns>an array of serializable objects of specified type</returns>
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
        // <summary>
        // 从2进制读取器读取分组后的数据，并按分组规则恢复分组前数据后输出<br/>
        // 分组规则：<br/>
        // 1、读取16字节数据，再读取一字节数据表示0x00的补充个数<br/>
        // 2、去除16字节数据中补充的0x00<br/>
        // 3、循环步骤1、2，直到数据全部读完后，输出恢复后的数据<br/>
        // </summary>
        // <param name="reader">2进制读取器</param>
        // <returns>恢复后的数据</returns>
        // <exception cref="System.FormatException">数据无法按分组规则恢复时抛出</exception>
        /// <summary>
        /// Read the grouped data from the binary reader and restore the pre-packet data according to the grouping rule and output it.
        /// Grouping rules:<br/>
        /// 1, read 16 bytes of data, and then read one byte of data to represent the amount of additional 0x00.
        /// 2, remove the 0x00s added to the 16-byte data.
        /// 3, loop steps 1, 2, until the data is completely read, output the restored data.
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <returns>restored data</returns>
        /// <exception cref="System.FormatException">data cannot be recovered by grouping rules</exception>
        public static byte[] ReadBytesWithGrouping(this BinaryReader reader)
        {
            // TODO 此方法在参数 value 的长度正好是16的整数倍时能否正常工作。需要测试。
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
        // <summary>
        // 从2进制读取器中读取限定长度的字符串
        // </summary>
        // <param name="reader">2进制读取器</param>
        // <param name="length">限定长度</param>
        // <returns>读取的字符串</returns>
        /// <summary>
        /// Reads a string of the specified length from the binary reader
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <param name="length">specified length</param>
        /// <returns>string</returns>
        public static string ReadFixedString(this BinaryReader reader, int length)
        {
            byte[] data = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(data.TakeWhile(p => p != 0).ToArray());
        }
        // <summary>
        // 读取2进制读取器的数据反序列化出一个可序列化的指定数据类型的对象
        // </summary>
        // <typeparam name="T">指定数据类型</typeparam>
        // <param name="reader">2进制读取器</param>
        // <returns>一个可序列化的指定数据类型的对象</returns>
        /// <summary>
        /// Deserializes data readed from the binary reader as a serializable object of specified type
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="reader">BinaryReader</param>
        /// <returns>a serializable object of the specified type</returns>
        public static T ReadSerializable<T>(this BinaryReader reader) where T : ISerializable, new()
        {
            T obj = new T();
            obj.Deserialize(reader);
            return obj;
        }
        // <summary>
        // 读取2进制读取器的数据反序列化出一个可序列化的指定数据类型的对象数组
        // </summary>
        // <typeparam name="T">指定数据类型</typeparam>
        // <param name="reader">2进制读取器</param>
        // <param name="max">数组最大容量，默认最大容量为16777216</param>
        // <returns>一个可序列化的指定数据类型的对象数组</returns>
        /// <summary>
        /// Deserializes data readed from the binary reader as an array of serializable objects of specified type
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="reader">BinaryReader</param>
        /// <param name="max">max size of the array,default value is 16777216</param>
        /// <returns>an array of serializable objects of specified type</returns>
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
        // <summary>
        // 从2进制读取器中读取一个指定大小的字节数组数据
        // </summary>
        // <param name="reader">2进制读取器</param>
        // <param name="max">最大可读取数</param>
        // <returns>读取的数组数据</returns>
        /// <summary>
        /// Read a byte array of data of specified size from a binary reader
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <param name="max">specified size</param>
        /// <returns>a byte array of data</returns>
        public static byte[] ReadVarBytes(this BinaryReader reader, int max = 0x1000000)
        {
            return reader.ReadBytes((int)reader.ReadVarInt((ulong)max));
        }
        // <summary>
        // 从2进制读取器中读取一个限定大小的变长数据，并转换为ulong。
        // </summary>
        // <param name="reader">2进制读取器</param>
        // <param name="max">最大可读取数</param>
        // <returns>读取的数据</returns>
        // <exception cref="System.FormatException">读取的数据的数值大于最大可读取数时抛出</exception>
        /// <summary>
        /// Reads a limited size variable length data from the binary reader and converts it to ulong.
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <param name="max">the maximum readable number</param>
        /// <returns>read data</returns>
        /// <exception cref="System.FormatException">
        /// the value of the read data is larger than the maximum readable number
        /// </exception>
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
        // <summary>
        // 从2进制读取器中读取一个限定长度的字节数组，并按照 UTF8 编码转换成字符串。
        // </summary>
        // <param name="reader">2进制读取器</param>
        // <param name="max">限定长度</param>
        // <returns>读取的的字符串数据</returns>
        /// <summary>
        /// Reads a specified size string from the binary reader and convert it into a string according to UTF8 encoding.
        /// </summary>
        /// <param name="reader">BinaryReader</param>
        /// <param name="max">specified size</param>
        /// <returns>read string</returns>
        public static string ReadVarString(this BinaryReader reader, int max = 0x1000000)
        {
            return Encoding.UTF8.GetString(reader.ReadVarBytes(max));
        }
        // <summary>
        // 将可序列化对象序列化成一个字节数组
        // </summary>
        // <param name="value">可序列化对象</param>
        // <returns>序列化成的字节数组</returns>
        /// <summary>
        /// Serialize serializable object into a byte array
        /// </summary>
        /// <param name="value">serializable object</param>
        /// <returns>byte array</returns>
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
        // <summary>
        // 将指定数据类型的可序列化对象数组序列化成一个字节数组
        // </summary>
        // <typeparam name="T">指定数据类型</typeparam>
        // <param name="value">可序列化对象数组</param>
        // <returns>序列化后的字节数组</returns>
        /// <summary>
        /// Serializes an array of serializable objects of the specified type into a byte array
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="value">an array of serializable objects</param>
        /// <returns>byte array</returns>
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
        // <summary>
        // 将可序列化对象序列化
        // </summary>
        // <param name="writer">二进制输出器</param>
        // <param name="value">可序列化对象</param>
        /// <summary>
        /// Serialize serializable object
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">serializable object</param>
        public static void Write(this BinaryWriter writer, ISerializable value)
        {
            value.Serialize(writer);
        }
        // <summary>
        // 将指定数据类型的可序列化对象数组序列化
        // </summary>
        // <typeparam name="T">指定的数据类型</typeparam>
        // <param name="writer">二进制输出器</param>
        // <param name="value">可序列化对象数组</param>
        /// <summary>
        /// Serializes an array of serializable objects of the specified type into a byte array
        /// </summary>
        /// <typeparam name="T">specified type</typeparam>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">an array of serializable objects</param>
        public static void Write<T>(this BinaryWriter writer, T[] value) where T : ISerializable
        {
            writer.WriteVarInt(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                value[i].Serialize(writer);
            }
        }
        // <summary>
        // 将字节数组数据按分组规则分组后输出<br/>
        // 分组规则执行步骤：<br/>
        // 1、取字节数组中16个字节数据输出，并在之后输出1个0x00<br/>
        // 2、循环步骤1，直到字节数组中未输出数据不足16个字节时，计算16-待输出数据字节个数的差值padding。<br/>
        // 3、将字节数组中待输出数据输出，并在其之后输出padding个0x00<br/>
        // 4、最后输出1字节等于padding数值的数据<br/>
        // </summary>
        // <param name="writer">2进制输出器</param>
        // <param name="value">字节数组数据</param>
        /// <summary>
        /// The byte array data is grouped according to the grouping rules and output.
        /// Group rule execution steps:<br/>
        /// 1. Take 16 bytes of data from the byte array and output.Output 1 0x00 afterwards<br/>
        /// 2, loop step 1, until the size of the rest of the byte array is less than 16 bytes, 
        /// calculate the padding.The padding equals 16 - the size of the rest of the byte array
        /// 3. Output the rest of the byte array, and output specified amount 0x00s after it.
        /// specified amount equlals the value of padding<br/>
        /// 4, finally output 1 byte data.The value of data equal the value of padding.
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">byte array data</param>
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
        // <summary>
        // 将字符串按 UTF 编码格式转换成字节数组，然后输出到 writer。不足指定长度时在之后补0x00.
        // </summary>
        // <param name="writer">2进制输出器</param>
        // <param name="value">字符串数据</param>
        // <param name="length">限定长度</param>
        // <exception cref="System.ArgumentException">字符串长度和序列化长度超过限定长度后抛出</exception>
        /// <summary>
        /// Converted a string to a byte array according to UTF8 encoding and output.
        /// When the length of the string is less than the specified length, it will attach  0x00s afterwards.
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">string</param>
        /// <param name="length">specified length</param>
        /// <exception cref="System.ArgumentException">If the length of string or the size of the array converted by the string larger than  specified length</exception>
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
        // <summary>
        // 将字节数组形式的数据按一定的格式序列化输出，输出格式为字节数组长度+字节数组的数据
        // </summary>
        // <param name="writer">二进制输出器</param>
        // <param name="value">待序列化的字节数组</param>
        /// <summary>
        /// Serialize the byte array in a specified format, and the format is  byte array length + byte array.
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">byte array</param>
        public static void WriteVarBytes(this BinaryWriter writer, byte[] value)
        {
            writer.WriteVarInt(value.Length);
            writer.Write(value);
        }
        // <summary>
        // 将长整形数据按可变长整数的格式序列化输出<br/>
        // 输出格式为：<br/>
        // 1）value小于0xFD，用一个字节的形式输出<br/>
        // 2）value小于等于0xFFFF，用0xFD+字节数组的形式输出<br/>
        // 3）value小于等于0xFFFFFFFF，用0xFE+字节数组的形式输出<br/>
        // 4) 其他情况，用0xFF+字节数组的形式输出
        // </summary>
        // <param name="writer">2进制输出器</param>
        // <param name="value">长整形数据</param>
        // <exception cref="System.ArgumentException">长整形数据小于0时抛出</exception>
        /// <summary>
        /// serialize the long integer data<br/>
        /// The output format is:<br/>
        /// 1) value is less than 0xFD, output in the form of one byte<br/>
        /// 2) value is less than or equal to 0xFFFF, output in the form of 0xFD+byte array
        /// 3) value is less than or equal to 0xFFFFFFFF, output in the form of 0xFE+byte array
        /// 4) Other cases, output as 0xFF+byte array
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">value</param>
        /// <exception cref="System.ArgumentException">If value less than 0</exception>
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
        // <summary>
        // 将字符串按照 UTF8编码成字节数组，然后序列化输出
        // </summary>
        // <param name="writer">二进制输出器</param>
        // <param name="value">待序列化的字符串</param>
        /// <summary>
        /// Encode the string into a byte array according to UTF8, then serialize the array
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="value">string</param>
        public static void WriteVarString(this BinaryWriter writer, string value)
        {
            writer.WriteVarBytes(Encoding.UTF8.GetBytes(value));
        }
    }
}
