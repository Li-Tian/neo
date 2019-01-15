using Neo.Cryptography;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 封装leveldb的切片，可以存放任何基础类型的值，并支持逻辑运算。
    // 与leveldb原生切片不同点是这里存放的是真实值而非引用或指针
    // </summary>
    /// <summary>
    /// Encapsulate leveldb slices to store any underlying type of value and support logical operations. <br/>
    /// Unlike leveldb's native slicing, real values are stored here instead of references or Pointers.
    /// </summary>
    public struct Slice : IComparable<Slice>, IEquatable<Slice>
    {
        internal byte[] buffer;

        internal Slice(IntPtr data, UIntPtr length)
        {
            buffer = new byte[(int)length];
            Marshal.Copy(data, buffer, 0, (int)length);
        }

        // <summary>
        // 比较大小，按照单个字节进行对比（注，当是前缀关系时，长度最长的大）
        // </summary>
        // <param name="other">待对比切片</param>
        // <returns>大于则返回1，等于则返回0，小于则返回-1</returns>
        /// <summary>
        /// Compare size, compare by single byte (note, when it is a prefix relationship, the longest length is large)
        /// </summary>
        /// <param name="other">Slice to be compared</param>
        /// <returns>Returns 1 if it is larger, 0 if it is equal, and returns -1 if it is smaller</returns>
        public int CompareTo(Slice other)
        {
            for (int i = 0; i < buffer.Length && i < other.buffer.Length; i++)
            {
                int r = buffer[i].CompareTo(other.buffer[i]);
                if (r != 0) return r;
            }
            return buffer.Length.CompareTo(other.buffer.Length);
        }

        // <summary>
        // 是否等于该切片
        // </summary>
        // <param name="other">待比较切片</param>
        // <returns>相等则返回true,否则返回false</returns>
        /// <summary>
        /// Is it equal to the slice
        /// </summary>
        /// <param name="other">Slice to be compared</param>
        /// <returns>Return true if it is equal, otherwise return  false</returns>
        public bool Equals(Slice other)
        {
            if (buffer.Length != other.buffer.Length) return false;
            return buffer.SequenceEqual(other.buffer);
        }

        // <summary>
        // 与某对象是否相等
        // </summary>
        // <param name="obj">待对比的对象</param>
        // <returns>注，若obj为null 或不是slice时，返回false</returns>
        /// <summary>
        /// Is it equal to an object
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Return false if obj is null or not slice</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Slice)) return false;
            return Equals((Slice)obj);
        }

        // <summary>
        // 获取hash code
        // </summary>
        // <returns>murmur32 hash code</returns>
        /// <summary>
        /// Get hash code
        /// </summary>
        /// <returns>murmur32 hash code</returns>
        public override int GetHashCode()
        {
            return (int)buffer.Murmur32(0);
        }

        // <summary>
        // 转成byte数组
        // </summary>
        // <returns>返回Slice内置数据的字节数组。若切片为空，返回空的byte数组</returns>
        /// <summary>
        /// Convert to byte array
        /// </summary>
        /// <returns>Return a byte array of the internal data of Slice. If the slice is empty, return an empty byte array</returns>
        public byte[] ToArray()
        {
            return buffer ?? new byte[0];
        }

        // <summary>
        // 转bool类型
        // </summary>
        // <returns>对应的bool值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于bool存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to bool type
        /// </summary>
        /// <returns>Corresponding bool value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the bool storage size</exception>
        unsafe public bool ToBoolean()
        {
            if (buffer.Length != sizeof(bool))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((bool*)pbyte);
            }
        }

        // <summary>
        // 转byte类型
        // </summary>
        // <returns>对应的byte值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于byte存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to byte type
        /// </summary>
        /// <returns>Corresponding byte value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the bool storage size</exception>
        public byte ToByte()
        {
            if (buffer.Length != sizeof(byte))
                throw new InvalidCastException();
            return buffer[0];
        }

        // <summary>
        // 转double类型
        // </summary>
        // <returns>对应的double值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于double存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to double type
        /// </summary>
        /// <returns>Corresponding byte value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the double storage size</exception>
        unsafe public double ToDouble()
        {
            if (buffer.Length != sizeof(double))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((double*)pbyte);
            }
        }

        // <summary>
        // 转int16类型
        // </summary>
        // <returns>对应的int16值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于short存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to int16 type
        /// </summary>
        /// <returns>Corresponding int16 value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the int16 storage size</exception>
        unsafe public short ToInt16()
        {
            if (buffer.Length != sizeof(short))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((short*)pbyte);
            }
        }

        // <summary>
        // 转int32类型
        // </summary>
        // <returns>对应的int32值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于int存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to int32 type
        /// </summary>
        /// <returns>Corresponding int32 value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the int32 storage size</exception>
        unsafe public int ToInt32()
        {
            if (buffer.Length != sizeof(int))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((int*)pbyte);
            }
        }

        // <summary>
        // 转int64类型
        // </summary>
        // <returns>对应的int64值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于long存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to int64 type
        /// </summary>
        /// <returns>Corresponding int64 value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the int64 storage size</exception>
        unsafe public long ToInt64()
        {
            if (buffer.Length != sizeof(long))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((long*)pbyte);
            }
        }

        // <summary>
        // 转float类型
        // </summary>
        // <returns>对应的float值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于float存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to float type
        /// </summary>
        /// <returns>Corresponding float value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the float storage size</exception>
        unsafe public float ToSingle()
        {
            if (buffer.Length != sizeof(float))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((float*)pbyte);
            }
        }

        // <summary>
        // 转utf8 string类型
        // </summary>
        // <returns>对应的utf8 string值</returns>
        /// <summary>
        /// Switch to utf8 string type
        /// </summary>
        /// <returns>Corresponding utf8 string value</returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(buffer);
        }

        // <summary>
        // 转uint16类型
        // </summary>
        // <returns>对应的uint16值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于ushort存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to uint16 type
        /// </summary>
        /// <returns>Corresponding uint16 value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the uint16 storage size</exception>
        unsafe public ushort ToUInt16()
        {
            if (buffer.Length != sizeof(ushort))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((ushort*)pbyte);
            }
        }

        // <summary>
        // 转uint32类型
        // </summary>
        // <returns>对应的uint32值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于uint存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to uint32 type
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>Corresponding uint32 value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the uint32 storage size</exception>
        unsafe public uint ToUInt32(int index = 0)
        {
            if (buffer.Length != sizeof(uint) + index)
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[index])
            {
                return *((uint*)pbyte);
            }
        }

        // <summary>
        // 转UInt64类型
        // </summary>
        // <returns>对应的UInt64值</returns>
        // <exception cref="System.InvalidCastException">若切片长度不等于ulong存储大小时，抛出该异常</exception>
        /// <summary>
        /// Switch to UInt64 type
        /// </summary>
        /// <returns>Corresponding UInt64 value</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the slice length is not equal to the UInt64 storage size</exception>
        unsafe public ulong ToUInt64()
        {
            if (buffer.Length != sizeof(ulong))
                throw new InvalidCastException();
            fixed (byte* pbyte = &buffer[0])
            {
                return *((ulong*)pbyte);
            }
        }

        // <summary>
        // 从byte类型默认转换成Slice数据类型
        // </summary>
        // <param name="data">字节数组数据</param>
        /// <summary>
        /// Convert from byte array type to Slice
        /// </summary>
        /// <param name="data">Byte array data</param>
        public static implicit operator Slice(byte[] data)
        {
            return new Slice { buffer = data };
        }

        // <summary>
        // 从bool类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">bool数据</param>
        /// <summary>
        /// Convert from bool type to array slice
        /// </summary>
        /// <param name="data">bool data</param>
        public static implicit operator Slice(bool data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }


        // <summary>
        // 从byte类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">字节数据</param>
        /// <summary>
        /// Convert from byte type to Slice
        /// </summary>
        /// <param name="data">byte data</param>
        public static implicit operator Slice(byte data)
        {
            return new Slice { buffer = new[] { data } };
        }

        // <summary>
        // 从double类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">double数据</param>
        /// <summary>
        /// Convert from double type to Slice
        /// </summary>
        /// <param name="data">double data</param>
        public static implicit operator Slice(double data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从short类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">short数据</param>
        /// <summary>
        /// Convert from short type to slice
        /// </summary>
        /// <param name="data">short data</param>
        public static implicit operator Slice(short data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从int类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">int数据</param>
        /// <summary>
        /// Convert from int type to Slice 
        /// </summary>
        /// <param name="data">int data</param>
        public static implicit operator Slice(int data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从long类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">long数据</param>
        /// <summary>
        /// Convert from long type to Slice 
        /// </summary>
        /// <param name="data">long data</param>
        public static implicit operator Slice(long data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从float类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">float数据</param>
        /// <summary>
        /// Convert from float type to Slice implicitly
        /// </summary>
        /// <param name="data">float data</param>
        public static implicit operator Slice(float data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从string类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">string数据</param>
        /// <summary>
        /// Convert from string type to Slice
        /// </summary>
        /// <param name="data">string data</param>
        public static implicit operator Slice(string data)
        {
            return new Slice { buffer = Encoding.UTF8.GetBytes(data) };
        }

        // <summary>
        // 从ushort类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">ushort数据</param>
        /// <summary>
        /// Convert from ushort type to Slice 
        /// </summary>
        /// <param name="data">ushort data</param>
        public static implicit operator Slice(ushort data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从uint类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">uint数据</param>
        /// <summary>
        /// Convert from uint type to Slice 
        /// </summary>
        /// <param name="data">uint data</param>
        public static implicit operator Slice(uint data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }

        // <summary>
        // 从ulong类型默认转换为Slice数据类型
        // </summary>
        // <param name="data">ulong数据</param>
        /// <summary>
        /// Convert from ulong type to Slice 
        /// </summary>
        /// <param name="data">ulong data</param>
        public static implicit operator Slice(ulong data)
        {
            return new Slice { buffer = BitConverter.GetBytes(data) };
        }


        // <summary>
        // 切片操作，小于
        // </summary>
        // <param name="x">第一个对象</param>
        // <param name="y">第二个对象</param>
        // <returns>比较结果，如果小于则返回true，否则返回false</returns>
        /// <summary>
        /// Slice operation, less than
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>Compare results, return true if less than, false otherwise</returns>
        public static bool operator <(Slice x, Slice y)
        {
            return x.CompareTo(y) < 0;
        }

        // <summary>
        // 切片操作，小于等于
        // </summary>
        // <param name="x">第一个对象</param>
        // <param name="y">第二个对象</param>
        // <returns>比较结果，如果小于等于则返回true，否则返回false</returns>
        /// <summary>
        /// Slice operation, less than or equal to
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>Compare results, return true if less than or equal to, false otherwise</returns>
        public static bool operator <=(Slice x, Slice y)
        {
            return x.CompareTo(y) <= 0;
        }
        // <summary>
        // 切片操作，大于
        // </summary>
        // <param name="x">第一个对象</param>
        // <param name="y">第二个对象</param>
        // <returns>比较结果，如果大于则返回true，否则返回false</returns>
        /// <summary>
        /// Slice operation, more than
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>Compare results, return true if more than, false otherwise</returns>
        public static bool operator >(Slice x, Slice y)
        {
            return x.CompareTo(y) > 0;
        }
        // <summary>
        // 切片操作，大于等于
        // </summary>
        // <param name="x">第一个对象</param>
        // <param name="y">第二个对象</param>
        // <returns>比较结果，如果大于等于则返回true，否则返回false</returns>
        /// <summary>
        /// Slice operation, more than or equal to
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>Compare results, return true if more than or equal to, false otherwise</returns>
        public static bool operator >=(Slice x, Slice y)
        {
            return x.CompareTo(y) >= 0;
        }

        // <summary>
        // 切片操作，等于
        // </summary>
        // <param name="x">第一个对象</param>
        // <param name="y">第二个对象</param>
        // <returns>比较结果，如果等于则返回true，否则返回false</returns>
        /// <summary>
        /// Slice operation, equal
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>Compare results, return true if equal, false otherwise</returns>
        public static bool operator ==(Slice x, Slice y)
        {
            return x.Equals(y);
        }


        // <summary>
        // 切片操作，不等于
        // </summary>
        // <param name="x">第一个对象</param>
        // <param name="y">第二个对象</param>
        // <returns>比较结果，如果不等于则返回true，否则返回false</returns>
        /// <summary>
        /// Slice operation, not equal
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>Compare results, return true if not equal, false otherwise</returns>
        public static bool operator !=(Slice x, Slice y)
        {
            return !x.Equals(y);
        }
    }
}
