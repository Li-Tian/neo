using System;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 写选项
    // </summary>
    /// <summary>
    /// Write Options
    /// </summary>
    public class WriteOptions
    {
        // <summary>
        // 默认写选项
        // </summary>
        /// <summary>
        /// Default Write Options
        /// </summary>
        public static readonly WriteOptions Default = new WriteOptions();
        internal readonly IntPtr handle = Native.leveldb_writeoptions_create();

        // <summary>
        // 是否直接同步到磁盘
        // </summary>
        /// <summary>
        /// Whether to sync directly to disk
        /// </summary>
        public bool Sync
        {
            set
            {
                Native.leveldb_writeoptions_set_sync(handle, value);
            }
        }
        // <summary>
        // 析构函数。将关闭leveldb的句柄。
        // </summary>
        /// <summary>
        /// Destructor. The handle of leveldb will be closed.
        /// </summary>
        ~WriteOptions()
        {
            Native.leveldb_writeoptions_destroy(handle);
        }
    }
}
