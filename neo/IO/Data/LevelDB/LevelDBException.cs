using System.Data.Common;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 封装的LeveldbException
    // </summary>
    /// <summary>
    /// Encapsulated Leveldb Exception
    /// </summary>
    public class LevelDBException : DbException
    {
        internal LevelDBException(string message)
            : base(message)
        {
        }
    }
}
