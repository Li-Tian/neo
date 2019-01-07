using System;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // LevelDB封装的DB操作类，提供基本的 Get, Delete, Put, BatchWrite, Snapshot等操作
    // </summary>
    /// <summary>
    /// LevelDB encapsulated DB operation class, providing basic Get, Delete, Put, BatchWrite, Snapshot, etc.
    /// </summary>
    public class DB : IDisposable
    {
        private IntPtr handle;

        // <summary>
        // 若没有获取到合法的句柄时, 返回True
        // </summary>
        /// <summary>
        /// Return True if no legal handle is obtained
        /// </summary>
        public bool IsDisposed => handle == IntPtr.Zero;

        private DB(IntPtr handle)
        {
            this.handle = handle;
        }

        // <summary>
        // 释放资源，包括Leveldb资源以及句柄释放
        // </summary>
        /// <summary>
        /// Release resources, including Leveldb resources and handle release 
        /// </summary>
        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.leveldb_close(handle);
                handle = IntPtr.Zero;
            }
        }

        // <summary>
        // 删除Key
        // </summary>
        // <param name="options">写选项</param>
        // <param name="key">要删除的key</param>
        // <exception cref="Neo.IO.Data.LevelDB.LevelDBException">遇到错误时，统一抛出该类型错误</exception>
        /// <summary>
        /// Delete Key
        /// </summary>
        /// <param name="options">Write Options</param>
        /// <param name="key">The key to be deleted</param>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">This type of exception is thrown when an error is encountered</exception>
        public void Delete(WriteOptions options, Slice key)
        {
            IntPtr error;
            Native.leveldb_delete(handle, options.handle, key.buffer, (UIntPtr)key.buffer.Length, out error);
            NativeHelper.CheckError(error);
        }

        // <summary>
        // 查询某一个Key
        // </summary>
        // <param name="options">读选项</param>
        // <param name="key">待查询的key</param>
        // <returns>value的切片</returns>
        // <exception cref="Neo.IO.Data.LevelDB.LevelDBException">查询不存在，或遇到错误时，统一抛出该异常 </exception>
        /// <summary>
        /// Query a key
        /// </summary>
        /// <param name="options">Read Options</param>
        /// <param name="key">Key to be queried</param>
        /// <returns>Slice of value</returns>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">The query is not presented, or an error is occured, throw the exception</exception>
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

        // <summary>
        // 获取快照
        // </summary>
        // <returns>快照对象</returns>
        /// <summary>
        /// Get a snapshot
        /// </summary>
        /// <returns>Snapshot object</returns>
        public Snapshot GetSnapshot()
        {
            return new Snapshot(handle);
        }

        // <summary>
        // 创建新的迭代器
        // </summary>
        // <param name="options">读选项</param>
        // <returns>迭代器</returns>
        /// <summary>
        /// Create a new iterator
        /// </summary>
        /// <param name="options">Read Options</param>
        /// <returns>iterator</returns>
        public Iterator NewIterator(ReadOptions options)
        {
            return new Iterator(Native.leveldb_create_iterator(handle, options.handle));
        }

        // <summary>
        //  打开数据库
        // </summary>
        // <param name="name">数据库路径</param>
        // <returns>DB实例</returns>
        /// <summary>
        /// Open the database
        /// </summary>
        /// <param name="name">Database path</param>
        /// <returns>DB instance</returns>
        public static DB Open(string name)
        {
            return Open(name, Options.Default);
        }

        // <summary>
        // 打开数据库
        // </summary>
        // <param name="name">数据库路径</param>
        // <param name="options">数据库打开相关设置</param>
        // <returns>DB实例</returns>
        // <exception cref="Neo.IO.Data.LevelDB.LevelDBException">遇到错误时，统一抛出该类型错误</exception>
        /// <summary>
        /// Open the database
        /// </summary>
        /// <param name="name">Database path</param>
        /// <param name="options">Database related settings</param>
        /// <returns>DB instance</returns>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">This type of exception is thrown when an error is encountered</exception>
        public static DB Open(string name, Options options)
        {
            IntPtr error;
            IntPtr handle = Native.leveldb_open(options.handle, name, out error);
            NativeHelper.CheckError(error);
            return new DB(handle);
        }

        // <summary>
        // 存放键值对
        // </summary>
        // <param name="options">写选项</param>
        // <param name="key">键</param>
        // <param name="value">值</param>
        // <exception cref="Neo.IO.Data.LevelDB.LevelDBException">遇到错误时，统一抛出该类型错误</exception>
        /// <summary>
        /// Store key-value pair
        /// </summary>
        /// <param name="options">Write Options</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <exception cref="Neo.IO.Data.LevelDB.LevelDBException">This type of exception is thrown when an error is encountered</exception>
        public void Put(WriteOptions options, Slice key, Slice value)
        {
            IntPtr error;
            Native.leveldb_put(handle, options.handle, key.buffer, (UIntPtr)key.buffer.Length, value.buffer, (UIntPtr)value.buffer.Length, out error);
            NativeHelper.CheckError(error);
        }


        // <summary>
        // 尝试获取某个Key，并返回bool类型
        // </summary>
        // <param name="options">读选项</param>
        // <param name="key">键</param>
        // <param name="value">值</param>
        // <returns>查询到返回true和value， 否则返回false</returns>
        /// <summary>
        /// Try to get a Key and return the bool type
        /// </summary>
        /// <param name="options">Read Options</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>Returns true and value if queried, otherwise return false</returns>
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

        // <summary>
        // 批量写操作
        // </summary>
        // <remarks>
        // 注意，抛出异常之前，内部会进行最多5次尝试写
        // </remarks>
        // <param name="options">写选项</param>
        // <param name="write_batch">批量写</param>
        // <exception cref="LevelDBException">5此重试之后如果仍然失败则抛出异常</exception>
        /// <summary>
        /// Batch write operation
        /// </summary>
        /// <remarks>Note that there will be up to 5 attempts to write internally before throwing an exception.</remarks>
        /// <param name="options">Write Options</param>
        /// <param name="write_batch">write batch</param>
        /// <exception cref="LevelDBException">Throws the exception if it still fails after 5 retries</exception>
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
