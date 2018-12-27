using System;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 读选项
    // </summary>
    /// <summary>
    /// Read Options
    /// </summary>
    public class ReadOptions
    {
        // <summary>
        // 默认读选项
        // </summary>
        /// <summary>
        /// Default ReadOptions
        /// </summary>
        public static readonly ReadOptions Default = new ReadOptions();
        internal readonly IntPtr handle = Native.leveldb_readoptions_create();

        // <summary>
        // 是否进行校验
        // </summary>
        /// <summary>
        /// Whether to check
        /// </summary>
        public bool VerifyChecksums
        {
            set
            {
                Native.leveldb_readoptions_set_verify_checksums(handle, value);
            }
        }

        // <summary>
        // 是否将读取的内容存放到缓存
        // </summary>
        /// <summary>
        /// Whether to store the read content in the cache
        /// </summary>
        public bool FillCache
        {
            set
            {
                Native.leveldb_readoptions_set_fill_cache(handle, value);
            }
        }

        // <summary>
        // 设置从本快照读取
        // </summary>
        /// <summary>
        /// Set to read from this snapshot
        /// </summary>
        public Snapshot Snapshot
        {
            set
            {
                Native.leveldb_readoptions_set_snapshot(handle, value.handle);
            }
        }
        // <summary>
        // 析构函数。将关闭leveldb的句柄。
        // </summary>
        /// <summary>
        /// Destructor. The handle of leveldb will be closed.
        /// </summary>
        ~ReadOptions()
        {
            Native.leveldb_readoptions_destroy(handle);
        }
    }
}
