using System;

namespace Neo.IO.Data.LevelDB
{
    public class WriteOptions
    {
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

        ~WriteOptions()
        {
            Native.leveldb_writeoptions_destroy(handle);
        }
    }
}
