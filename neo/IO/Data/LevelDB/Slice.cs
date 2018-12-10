using Neo.Cryptography;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// 封装leveldb的切片，可以存放任何基础类型的值，并支持逻辑运算。与leveldb原生切片不同点是这里存放的是真实值而非引用或指针
    /// </summary>
    public struct Slice : IComparable<Slice>, IEquatable<Slice>
    {
        internal byte[] buffer;

        internal Slice(IntPtr data, UIntPtr length)
        {
            buffer = new byte[(int)length];
            Marshal.Copy(data, buffer, 0, (int)length);
        }

        /// <summary>
        /// 比较大小，按照单个字节进行对比（注，当是前缀关系时，长度最长的大）
        /// </summary>
        /// <param name="other">待对比切片</param>
        /// <returns>大于则返回1，等于则返回0，小于则返回-1</returns>
        public int CompareTo(Slice other)
        {
            for (int i = 0; i < buffer.Length && i < other.buffer.Length; i++)
            {
                int r = buffer[i].CompareTo(other.buffer[i]);
                if (r != 0) return r;
            }
            return buffer.Length.CompareTo(other.buffer.Length);
        }

        /// <summary>
        /// 是否等于该切片
        /// </summary>
        /// <param name="other">待比较切片</param>
        /// <returns>相等则返回true,否则返回false</returns>
        public bool Equals(Slice other)
        {
            if (buffer.Length != other.buffer.Length) return false;
            return buffer.SequenceEqual(other.buffer);
        }

        /// <summary>
        /// 与某对象是否相等
        /// </summary>
        /// <param name="obj">待对比的对象</param>
        /// <returns>注，若obj为null 或不是slice时，返回false</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Slice)) return false;
            return Equals((Slice)obj);
        }

        /// <summary>
        /// 获取hash code
        /// </summary>
        /// <returns>murmur32 hash code</returns>
        public override int GetHashCode()
        {
            return (int)buffer.Murmur32(0);
        }

        /// <summary>
        /// 转成byte数组
        /// </summary>
        /// <returns>若切片为空，返回空的byte数组</returns>
        public byte[] ToArray()
        {
            return buffer ?? new byte[0];
        }

        /// <summary>
        /// 转bool类型
        /// </summary>
        /// <returns>对应的bool值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于bool存储大小时，抛出该异常</exception>
        unsafe public bool ToBoolean()
        {
            if (buffer.Length != sizeof(bool))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((bool*)pbyte);
            }
        }

        /// <summary>
        /// 转byte类型
        /// </summary>
        /// <returns>对应的byte值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于byte存储大小时，抛出该异常</exception>
        public byte ToByte()
        {
            if (buffer.Length != sizeof(byte))
                throw new InvalidCastException();
            return buffer[0];
        }

        /// <summary>
        /// 转double类型
        /// </summary>
        /// <returns>对应的double值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于double存储大小时，抛出该异常</exception>
        unsafe public double ToDouble()
        {
            if (buffer.Length != sizeof(double))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((double*)pbyte);
            }
        }

        /// <summary>
        /// 转int16类型
        /// </summary>
        /// <returns>对应的int16值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于short存储大小时，抛出该异常</exception>
        unsafe public short ToInt16()
        {
            if (buffer.Length != sizeof(short))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((short*)pbyte);
            }
        }

        /// <summary>
        /// 转int32类型
        /// </summary>
        /// <returns>对应的int32值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于int存储大小时，抛出该异常</exception>
        unsafe public int ToInt32()
        {
            if (buffer.Length != sizeof(int))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((int*)pbyte);
            }
        }

        /// <summary>
        /// 转int64类型
        /// </summary>
        /// <returns>对应的int64值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于long存储大小时，抛出该异常</exception>
        unsafe public long ToInt64()
        {
            if (buffer.Length != sizeof(long))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((long*)pbyte);
            }
        }

        /// <summary>
        /// 转float类型
        /// </summary>
        /// <returns>对应的float值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于float存储大小时，抛出该异常</exception>
        unsafe public float ToSingle()
        {
            if (buffer.Length != sizeof(float))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((float*)pbyte);
            }
        }

        /// <summary>
        /// 转utf8 string类型
        /// </summary>
        /// <returns>对应的utf8 string值</returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 转uint16类型
        /// </summary>
        /// <returns>对应的uint16值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于ushort存储大小时，抛出该异常</exception>
        unsafe public ushort ToUInt16()
        {
            if (buffer.Length != sizeof(ushort))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((ushort*)pbyte);
            }
        }

        /// <summary>
        /// 转uint32类型
        /// </summary>
        /// <returns>对应的uint32值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于uint存储大小时，抛出该异常</exception>
        unsafe public uint ToUInt32(int index = 0)
        {
            if (buffer.Length != sizeof(uint) + index)
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[index])
            {
                return *((uint*)pbyte);
            }
        }

        /// <summary>
        /// 转UInt64类型
        /// </summary>
        /// <returns>对应的UInt64值</returns>
        /// <exception cref="System.InvalidCastException">若切片长度不等于ulong存储大小时，抛出该异常</exception>
        unsafe public ulong ToUInt64()
        {
            if (buffer.Length != sizeof(ulong))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((ulong*)pbyte);
            }
        }

        /// <summary>
        /// 创建byte数组切片
        /// </summary>
        /// <param name="data">字节数组数据</param>
        public static implicit operator Slice(byte[] data)
        {
            return new Slice { buffer = data };
        }

        /// <summary>
        /// 创建bool切片
        /// </summary>
        /// <param name="data">bool数据</param>
        public static implicit operator Slice(bool data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }


        /// <summary>
        /// 创建byte切片
        /// </summary>
        /// <param name="data">字节数据</param>
        public static implicit operator Slice(byte data)
        {
            return new Slice { buffer = new[] { data } };
        }

        /// <summary>
        /// 创建double切片
        /// </summary>
        /// <param name="data">double数据</param>
        public static implicit operator Slice(double data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建short切片
        /// </summary>
        /// <param name="data">short数据</param>
        public static implicit operator Slice(short data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建int切片
        /// </summary>
        /// <param name="data">int数据</param>
        public static implicit operator Slice(int data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建long切片
        /// </summary>
        /// <param name="data">long数据</param>
        public static implicit operator Slice(long data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建float切片
        /// </summary>
        /// <param name="data">float数据</param>
        public static implicit operator Slice(float data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建string切片
        /// </summary>
        /// <param name="data">string数据</param>
        public static implicit operator Slice(string data)
        {
            return new Slice { buffer = Encoding.UTF8.GetBytes(data) };
        }

        /// <summary>
        /// 创建ushort切片
        /// </summary>
        /// <param name="data">ushort数据</param>
        public static implicit operator Slice(ushort data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建uint切片
        /// </summary>
        /// <param name="data">uint数据</param>
        public static implicit operator Slice(uint data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        /// <summary>
        /// 创建ulong切片
        /// </summary>
        /// <param name="data">ulong数据</param>
        public static implicit operator Slice(ulong data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }


        /// <summary>
        /// 切片操作，小于
        /// </summary>
        /// <param name="x">第一个对象</param>
        /// <param name="y">第二个对象</param>
        /// <returns>比较结果，如果小于则返回true，否则返回false</returns>
        public static bool operator <(Slice x, Slice y)
        {
            return x.CompareTo(y) < 0;
        }

        /// <summary>
        /// 切片操作，小于等于
        /// </summary>
        /// <param name="x">第一个对象</param>
        /// <param name="y">第二个对象</param>
        /// <returns>比较结果，如果小于等于则返回true，否则返回false</returns>
        public static bool operator <=(Slice x, Slice y)
        {
            return x.CompareTo(y) <= 0;
        }
        /// <summary>
        /// 切片操作，大于
        /// </summary>
        /// <param name="x">第一个对象</param>
        /// <param name="y">第二个对象</param>
        /// <returns>比较结果，如果大于则返回true，否则返回false</returns>
        public static bool operator >(Slice x, Slice y)
        {
            return x.CompareTo(y) > 0;
        }
        /// <summary>
        /// 切片操作，大于等于
        /// </summary>
        /// <param name="x">第一个对象</param>
        /// <param name="y">第二个对象</param>
        /// <returns>比较结果，如果大于等于则返回true，否则返回false</returns>
        public static bool operator >=(Slice x, Slice y)
        {
            return x.CompareTo(y) >= 0;
        }

        /// <summary>
        /// 切片操作，等于
        /// </summary>
        /// <param name="x">第一个对象</param>
        /// <param name="y">第二个对象</param>
        /// <returns>比较结果，如果等于则返回true，否则返回false</returns>
        public static bool operator ==(Slice x, Slice y)
        {
            return x.Equals(y);
        }


        /// <summary>
        /// 切片操作，不等于
        /// </summary>
        /// <param name="x">第一个对象</param>
        /// <param name="y">第二个对象</param>
        /// <returns>比较结果，如果不等于则返回true，否则返回false</returns>
        public static bool operator !=(Slice x, Slice y)
        {
            return !x.Equals(y);
        }
    }
}
