using System;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 批量操作，提供了对Delete，Put的批量操作
    // </summary>
    /// <summary>
    /// Batch operation, providing batch operation on Delete, Put
    /// </summary>
    public class WriteBatch
    {
        internal readonly IntPtr handle = Native.leveldb_writebatch_create();
        // <summary>
        // 析构函数。将关闭leveldb的句柄。
        // </summary>
        /// <summary>
        /// Destructor. The handle of leveldb will be closed.
        /// </summary>
        ~WriteBatch()
        {
            Native.leveldb_writebatch_destroy(handle);
        }

        // <summary>
        // 清空操作
        // </summary>
        /// <summary>
        /// Clear operation
        /// </summary>
        public void Clear()
        {
            Native.leveldb_writebatch_clear(handle);
        }

        // <summary>
        // 删除某个Key
        // </summary>
        // <param name="key">待删除的Key</param>
        /// <summary>
        /// Delete a key
        /// </summary>
        /// <param name="key">The key to be deleted</param>
        public void Delete(Slice key)
        {
            Native.leveldb_writebatch_delete(handle, key.buffer, (UIntPtr)key.buffer.Length);
        }

        // <summary>
        // 添加键值对
        // </summary>
        // <param name="key">键</param>
        // <param name="value">值</param>
        /// <summary>
        /// Add a key-value pair
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Put(Slice key, Slice value)
        {
            Native.leveldb_writebatch_put(handle, key.buffer, (UIntPtr)key.buffer.Length, value.buffer, (UIntPtr)value.buffer.Length);
        }
    }
}
