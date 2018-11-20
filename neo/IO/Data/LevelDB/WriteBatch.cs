using System;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// 批量操作，提供了对Delete，Put的批量操作
    /// </summary>
    public class WriteBatch
    {
        internal readonly IntPtr handle = Native.leveldb_writebatch_create();

        ~WriteBatch()
        {
            Native.leveldb_writebatch_destroy(handle);
        }

        /// <summary>
        /// 清空操作
        /// </summary>
        public void Clear()
        {
            Native.leveldb_writebatch_clear(handle);
        }

        /// <summary>
        /// 删除某个Key
        /// </summary>
        /// <param name="key">待删除的Key</param>
        public void Delete(Slice key)
        {
            Native.leveldb_writebatch_delete(handle, key.buffer, (UIntPtr)key.buffer.Length);
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Put(Slice key, Slice value)
        {
            Native.leveldb_writebatch_put(handle, key.buffer, (UIntPtr)key.buffer.Length, value.buffer, (UIntPtr)value.buffer.Length);
        }
    }
}
