using System;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 快照
    // </summary>
    /// <summary>
    /// Snapshot
    /// </summary>
    public class Snapshot : IDisposable
    {
        internal IntPtr db, handle;

        internal Snapshot(IntPtr db)
        {
            this.db = db;
            this.handle = Native.leveldb_create_snapshot(db);
        }


        // <summary>
        // 释放快照资源
        // </summary>
        /// <summary>
        /// Release snapshot resources
        /// </summary>
        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.leveldb_release_snapshot(db, handle);
                handle = IntPtr.Zero;
            }
        }
    }
}
