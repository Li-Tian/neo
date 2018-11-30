using System;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// LevelDB封装的DB操作类，提供基本的 Get, Delete, Put, BatchWrite, Snapshot等操作
    /// </summary>
    public class DB : IDisposable
    {
        private IntPtr handle;

        /// <summary>
        /// 若没有获取到合法的句柄时, 返回Ture
        /// </summary>
        public bool IsDisposed => handle == IntPtr.Zero;

        private DB(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// 释放资源，包括Leveldb资源以及句柄释放
        /// </summary>
        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.leveldb_close(handle);
                handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 删除Key
        /// </summary>
        /// <param name="options">写选项</param>
        /// <param name="key">要删除的前缀key</param>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">遇到错误时，统一抛出该类型错误</exception>
        public void Delete(WriteOptions options, Slice key)
        {
            IntPtr error;
            Native.leveldb_delete(handle, options.handle, key.buffer, (UIntPtr)key.buffer.Length, out error);
            NativeHelper.CheckError(error);
        }

        /// <summary>
        /// 查询某一个Key
        /// </summary>
        /// <param name="options">读选项</param>
        /// <param name="key">待查询的key</param>
        /// <returns></returns>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">查询不存在，或遇到错误时，统一抛出该异常 </exception>
        public Slice Get(ReadOptions options, Slice key)
        {
            UIntPtr length;
            IntPtr error;
            IntPtr value = Native.leveldb_get(handle, options.handle, key.buffer, (UIntPtr)key.buffer.Length, out length, out error);
            try
            {
                NativeHelper.CheckError(error);
                if (value == IntPtr.Zero)
                    throw new LevelDBException("not found");
                return new Slice(value, length);
            }
            finally
            {
                if (value != IntPtr.Zero) Native.leveldb_free(value);
            }
        }

        /// <summary>
        /// 获取快照
        /// </summary>
        /// <returns></returns>
        public Snapshot GetSnapshot()
        {
            return new Snapshot(handle);
        }

        /// <summary>
        /// 创建新的迭代器
        /// </summary>
        /// <param name="options">读选项</param>
        /// <returns>迭代器</returns>
        public Iterator NewIterator(ReadOptions options)
        {
            return new Iterator(Native.leveldb_create_iterator(handle, options.handle));
        }

        /// <summary>
        ///  打开数据库
        /// </summary>
        /// <param name="name">数据库路径</param>
        /// <returns>DB实例</returns>
        public static DB Open(string name)
        {
            return Open(name, Options.Default);
        }

        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <param name="name">数据库路径</param>
        /// <param name="options">数据库打开相关设置</param>
        /// <returns>DB实例</returns>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">遇到错误时，统一抛出该类型错误</exception>
        public static DB Open(string name, Options options)
        {
            IntPtr error;
            IntPtr handle = Native.leveldb_open(options.handle, name, out error);
            NativeHelper.CheckError(error);
            return new DB(handle);
        }

        /// <summary>
        /// 存放键值对
        /// </summary>
        /// <param name="options">写选项</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">遇到错误时，统一抛出该类型错误</exception>
        public void Put(WriteOptions options, Slice key, Slice value)
        {
            IntPtr error;
            Native.leveldb_put(handle, options.handle, key.buffer, (UIntPtr)key.buffer.Length, value.buffer, (UIntPtr)value.buffer.Length, out error);
            NativeHelper.CheckError(error);
        }


        /// <summary>
        /// 尝试获取某个Key，并返回bool类型
        /// </summary>
        /// <param name="options">读选项</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>查询到返回true和value， 否则返回false</returns>
        public bool TryGet(ReadOptions options, Slice key, out Slice value)
        {
            UIntPtr length;
            IntPtr error;
            IntPtr v = Native.leveldb_get(handle, options.handle, key.buffer, (UIntPtr)key.buffer.Length, out length, out error);
            if (error != IntPtr.Zero)
            {
                Native.leveldb_free(error);
                value = default(Slice);
                return false;
            }
            if (v == IntPtr.Zero)
            {
                value = default(Slice);
                return false;
            }
            value = new Slice(v, length);
            Native.leveldb_free(v);
            return true;
        }

        /// <summary>
        /// 批量写操作
        /// </summary>
        /// <remarks>
        /// 注意，这里并不会抛出异常，内部会进行最多5次尝试写
        /// </remarks>
        /// <param name="options">写选项</param>
        /// <param name="write_batch">批量写</param>
        public void Write(WriteOptions options, WriteBatch write_batch)
        {
            // There's a bug in .Net Core.
            // When calling DB.Write(), it will throw LevelDBException sometimes.
            // But when you try to catch the exception, the bug disappears.
            // We shall remove the "try...catch" clause when Microsoft fix the bug.
            byte retry = 0;
            while (true)
            {
                try
                {
                    IntPtr error;
                    Native.leveldb_write(handle, options.handle, write_batch.handle, out error);
                    NativeHelper.CheckError(error);
                    break;
                }
                catch (LevelDBException ex)
                {
                    if (++retry >= 4) throw;
                    System.IO.File.AppendAllText("leveldb.log", ex.Message + "\r\n");
                }
            }
        }
    }
}
