using System;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// 封装的leveldb迭代器
    /// </summary>
    public class Iterator : IDisposable
    {
        private IntPtr handle;

        internal Iterator(IntPtr handle)
        {
            this.handle = handle;
        }

        private void CheckError()
        {
            IntPtr error;
            Native.leveldb_iter_get_error(handle, out error);
            NativeHelper.CheckError(error);
        }

        /// <summary>
        /// 释放资源，包括Leveldbd迭代器以及句柄释放
        /// </summary>
        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.leveldb_iter_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 获取当前Key值
        /// </summary>
        /// <returns>Slice数据</returns>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">若遇到错误，统一抛出该类型错误</exception>
        public Slice Key()
        {
            UIntPtr length;
            IntPtr key = Native.leveldb_iter_key(handle, out length);
            CheckError();
            return new Slice(key, length);
        }

        /// <summary>
        /// 游标下移一位
        /// </summary>
        public void Next()
        {
            Native.leveldb_iter_next(handle);
            CheckError();
        }

        /// <summary>
        /// 游标上移一位
        /// </summary>
        public void Prev()
        {
            Native.leveldb_iter_prev(handle);
            CheckError();
        }

        /// <summary>
        /// 移动游标到某一个Key上
        /// </summary>
        /// <param name="target">目标key</param>
        public void Seek(Slice target)
        {
            Native.leveldb_iter_seek(handle, target.buffer, (UIntPtr)target.buffer.Length);
        }

        /// <summary>
        /// 移动游标到首位置
        /// </summary>
        public void SeekToFirst()
        {
            Native.leveldb_iter_seek_to_first(handle);
        }

        /// <summary>
        /// 移动游标到最后一位位置
        /// </summary>
        public void SeekToLast()
        {
            Native.leveldb_iter_seek_to_last(handle);
        }

        /// <summary>
        /// 检查句柄是否合法
        /// </summary>
        /// <returns>合法则返回true,否则返回false</returns>
        public bool Valid()
        {
            return Native.leveldb_iter_valid(handle);
        }

        /// <summary>
        /// 当前位置的值
        /// </summary>
        /// <returns>Slice数据</returns>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">若遇到错误，统一抛出该类型错误</exception>
        public Slice Value()
        {
            UIntPtr length;
            IntPtr value = Native.leveldb_iter_value(handle, out length);
            CheckError();
            return new Slice(value, length);
        }
    }
}
