using System;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // Leveldb的相关选项
    // </summary>
    /// <summary>
    /// Leveldb related options
    /// </summary>
    public class Options
    {
        // <summary>
        // 默认设置
        // </summary>
        /// <summary>
        /// default Options
        /// </summary>
        public static readonly Options Default = new Options();

        // <summary>
        // db句柄
        // </summary>
        /// <summary>
        /// dn handle
        /// </summary>
        internal readonly IntPtr handle = Native.leveldb_options_create();

        // <summary>
        // 若db不存在则创建
        // </summary>
        /// <summary>
        /// Create if db does not exist
        /// </summary>
        public bool CreateIfMissing
        {
            set
            {
                Native.leveldb_options_set_create_if_missing(handle, value);
            }
        }

        // <summary>
        // 若db已存在时返回错误 
        // </summary>
        /// <summary>
        /// Return error if db already exists
        /// </summary>
        public bool ErrorIfExists
        {
            set
            {
                Native.leveldb_options_set_error_if_exists(handle, value);
            }
        }

        // <summary>
        // 是否进行数据损坏检查
        // </summary>
        /// <summary>
        /// Whether to perform data corruption check
        /// </summary>
        public bool ParanoidChecks
        {
            set
            {
                Native.leveldb_options_set_paranoid_checks(handle, value);
            }
        }

        // <summary>
        // 写缓存大小
        // </summary>
        /// <summary>
        /// Size of write buffer
        /// </summary>
        public int WriteBufferSize
        {
            set
            {
                Native.leveldb_options_set_write_buffer_size(handle, (UIntPtr)value);
            }
        }

        // <summary>
        // 最大打开文件数
        // </summary>
        /// <summary>
        /// Maximum number of open files
        /// </summary>
        public int MaxOpenFiles
        {
            set
            {
                Native.leveldb_options_set_max_open_files(handle, value);
            }
        }

        // <summary>
        // Leveldb存储的Block大小
        // </summary>
        /// <summary>
        /// Block size of Leveldb storage
        /// </summary>
        public int BlockSize
        {
            set
            {
                Native.leveldb_options_set_block_size(handle, (UIntPtr)value);
            }
        }

        // <summary>
        // 每隔几个key就直接存储一个重启点key(为了兼顾查找效率，
        // 每隔K个key，leveldb就不使用前缀压缩，而是存储整个key，
        // 这就是重启点（restartpoint）) 
        // </summary>
        /// <summary>
        /// a restartpoint key is stored directly every few keys
        /// </summary>
        public int BlockRestartInterval
        {
            set
            {
                Native.leveldb_options_set_block_restart_interval(handle, value);
            }
        }

        // <summary>
        // 是否压缩
        // </summary>
        /// <summary>
        /// Whether to compress
        /// </summary>
        public CompressionType Compression
        {
            set
            {
                Native.leveldb_options_set_compression(handle, value);
            }
        }

        // <summary>
        // 过滤策略
        // </summary>
        /// <summary>
        /// Filter Policy
        /// </summary>
        public IntPtr FilterPolicy
        {
            set
            {
                Native.leveldb_options_set_filter_policy(handle, value);
            }
        }

        // <summary>
        // 析构函数。将关闭leveldb的句柄。
        // </summary>
        /// <summary>
        /// Destructor. The handle of leveldb will be closed.
        /// </summary>
        ~Options()
        {
            Native.leveldb_options_destroy(handle);
        }
    }
}
