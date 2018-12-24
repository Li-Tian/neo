using System.Data.Common;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // 封装的LeveldbException
    // </summary>
    /// <summary>
    /// Encapsulation LeveldbException
    /// </summary>
    public class LevelDBException : DbException
    {
        internal LevelDBException(string message)
            : base(message)
        {
        }
    }
}
