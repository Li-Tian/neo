using System;

namespace Neo.IO.Data.LevelDB
{
    public class ReadOptions
    {
        public static readonly ReadOptions Default = new ReadOptions();
        internal readonly IntPtr handle = Native.leveldb_readoptions_create();

        /// <summary>
        /// 是否进行校验
        /// </summary>
        public bool VerifyChecksums
        {
            set
            {
                Native.leveldb_readoptions_set_verify_checksums(handle, value);
            }
        }

        /// <summary>
        /// 是否将读取的内容存放到缓存
        /// </summary>
        public bool FillCache
        {
            set
            {
                Native.leveldb_readoptions_set_fill_cache(handle, value);
            }
        }

        /// <summary>
        /// 设置从本快照读取
        /// </summary>
        public Snapshot Snapshot
        {
            set
            {
                Native.leveldb_readoptions_set_snapshot(handle, value.handle);
            }
        }

        ~ReadOptions()
        {
            Native.leveldb_readoptions_destroy(handle);
        }
    }
}
