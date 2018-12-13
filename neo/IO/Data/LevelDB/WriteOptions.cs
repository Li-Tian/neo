using System;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// 写选项
    /// </summary>
    public class WriteOptions
    {
        /// <summary>
        /// 默认写选项
        /// </summary>
        public static readonly WriteOptions Default = new WriteOptions();
        internal readonly IntPtr handle = Native.leveldb_writeoptions_create();

        /// <summary>
        /// 是否直接同步到磁盘
        /// </summary>
        public bool Sync
        {
            set
            {
                Native.leveldb_writeoptions_set_sync(handle, value);
            }
        }
        /// <summary>
        /// 析构函数。将关闭leveldb的句柄。
        /// </summary>
        ~WriteOptions()
        {
            Native.leveldb_writeoptions_destroy(handle);
        }
    }
}
