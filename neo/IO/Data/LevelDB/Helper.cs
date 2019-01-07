using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.IO.Data.LevelDB
{
    // <summary>
    // Leveldb辅助方法
    // </summary>
    /// <summary>
    /// Leveldb helper method
    /// </summary>
    public static class Helper
    {
        // <summary>
        // 向批处理中添加删除Key的行为
        // </summary>
        // <param name="batch">批量操作</param>
        // <param name="prefix">待删除前缀</param>
        // <param name="key">待删除的Key</param>
        /// <summary>
        /// Add a delete key operation to the batch
        /// </summary>
        /// <param name="batch">Batch operation</param>
        /// <param name="prefix">Prefix to be deleted</param>
        /// <param name="key">Key to be deleted</param>
        public static void Delete(this WriteBatch batch, byte prefix, ISerializable key)
        {
            batch.Delete(SliceBuilder.Begin(prefix).Add(key));
        }

        // <summary>
        // 查询前缀匹配的元素
        // </summary>
        // <typeparam name="T">值泛型</typeparam>
        // <param name="db">待查询db</param>
        // <param name="options">读选项</param>
        // <param name="prefix">待查询前缀</param>
        // <returns>T列表</returns>
        /// <summary>
        /// Query the element that the prefix matches
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="db">Db to be queried</param>
        /// <param name="options">Read Options</param>
        /// <param name="prefix">Prefix to be queried</param>
        /// <returns>T list</returns>
        public static IEnumerable<T> Find<T>(this DB db, ReadOptions options, byte prefix) where T : class, ISerializable, new()
        {
            return Find(db, options, SliceBuilder.Begin(prefix), (k, v) => v.ToArray().AsSerializable<T>());
        }

        // <summary>
        // 查询前缀匹配的元素
        // </summary>
        // <typeparam name="T">值泛型</typeparam>
        // <param name="db">待查询db</param>
        // <param name="options">读选项</param>
        // <param name="prefix">待查询前缀</param>
        // <param name="resultSelector">值处理回调函数</param>
        // <returns>T列表</returns>
        /// <summary>
        /// Query the element that the prefix matches
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="db">Db to be queried</param>
        /// <param name="options">Read Options</param>
        /// <param name="prefix">Prefix to be queried</param>
        /// <param name="resultSelector">Value handling callback function</param>
        /// <returns>T list</returns>
        public static IEnumerable<T> Find<T>(this DB db, ReadOptions options, Slice prefix, Func<Slice, Slice, T> resultSelector)
        {
            using (Iterator it = db.NewIterator(options))
            {
                for (it.Seek(prefix); it.Valid(); it.Next())
                {
                    Slice key = it.Key();
                    byte[] x = key.ToArray();
                    byte[] y = prefix.ToArray();
                    if (x.Length < y.Length) break;
                    if (!x.Take(y.Length).SequenceEqual(y)) break;
                    yield return resultSelector(key, it.Value());
                }
            }
        }

        // <summary>
        // 查询某个Key
        // </summary>
        // <typeparam name="T">值泛型</typeparam>
        // <param name="db">待查询db</param>
        // <param name="options">读选项</param>
        // <param name="prefix">待查询前缀</param>
        // <param name="key">待查询Key</param>
        // <returns>T</returns>
        /// <summary>
        /// Query a Key
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="db">Db to be queried</param>
        /// <param name="options">Read Options</param>
        /// <param name="prefix">Prefix to be queried</param>
        /// <param name="key">Key to be queried</param>
        /// <returns>T</returns>
        public static T Get<T>(this DB db, ReadOptions options, byte prefix, ISerializable key) where T : class, ISerializable, new()
        {
            return db.Get(options, SliceBuilder.Begin(prefix).Add(key)).ToArray().AsSerializable<T>();
        }


        // <summary>
        // 查询某个Key
        // </summary>
        // <typeparam name="T">值泛型</typeparam>
        // <param name="db">待查询db</param>
        // <param name="options">读选项</param>
        // <param name="prefix">待查询前缀</param>
        // <param name="key">待查询Key</param>
        // <param name="resultSelector">值处理回调函数</param>
        // <returns>T</returns>
        /// <summary>
        /// Query a Key
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="db">Db to be queried</param>
        /// <param name="options">Read Options</param>
        /// <param name="prefix">Prefix to be queried</param>
        /// <param name="key">Key to be queried</param>
        /// <param name="resultSelector">Value handling callback function</param>
        /// <returns>T</returns>
        public static T Get<T>(this DB db, ReadOptions options, byte prefix, ISerializable key, Func<Slice, T> resultSelector)
        {
            return resultSelector(db.Get(options, SliceBuilder.Begin(prefix).Add(key)));
        }

        // <summary>
        // 向批处理中添加写入键值的行为
        // </summary>
        // <param name="batch">批量操作</param>
        // <param name="prefix">前缀</param>
        // <param name="key">键</param>
        // <param name="value">值</param>
        /// <summary>
        /// Add a write key-value operation to the batch
        /// </summary>
        /// <param name="batch">Batch operation</param>
        /// <param name="prefix">prefix</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void Put(this WriteBatch batch, byte prefix, ISerializable key, ISerializable value)
        {
            batch.Put(SliceBuilder.Begin(prefix).Add(key), value.ToArray());
        }

        // <summary>
        // 尝试获取Key
        // </summary>
        // <typeparam name="T">值泛型</typeparam>
        // <param name="db">待查询db</param>
        // <param name="options">读选项</param>
        // <param name="prefix">待查询前缀</param>
        // <param name="key">待查询Key</param>
        // <returns>T，若不存在时，返回null</returns>
        /// <summary>
        /// Try to get a Key
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="db">Db to be queried</param>
        /// <param name="options">Read Options</param>
        /// <param name="prefix">Prefix to be queried</param>
        /// <param name="key">Key to be queried</param>
        /// <returns>T, if it does not exist, return null</returns>
        public static T TryGet<T>(this DB db, ReadOptions options, byte prefix, ISerializable key) where T : class, ISerializable, new()
        {
            Slice slice;
            if (!db.TryGet(options, SliceBuilder.Begin(prefix).Add(key), out slice))
                return null;
            return slice.ToArray().AsSerializable<T>();
        }

        // <summary>
        // 尝试获取Key
        // </summary>
        // <typeparam name="T">值泛型</typeparam>
        // <param name="db">待查询db</param>
        // <param name="options">读选项</param>
        // <param name="prefix">待查询前缀</param>
        // <param name="key">待查询Key</param>
        // <param name="resultSelector">值处理回调函数</param>
        // <returns>T，若不存在时，返回null</returns>
        /// <summary>
        /// Try to get a Key
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="db">Db to be queried</param>
        /// <param name="options">Read Options</param>
        /// <param name="prefix">Prefix to be queried</param>
        /// <param name="key">Key to be queried</param>
        /// <param name="resultSelector">Value handling callback function</param>
        /// <returns>T, if it does not exist, return null</returns>
        public static T TryGet<T>(this DB db, ReadOptions options, byte prefix, ISerializable key, Func<Slice, T> resultSelector) where T : class
        {
            Slice slice;
            if (!db.TryGet(options, SliceBuilder.Begin(prefix).Add(key), out slice))
                return null;
            return resultSelector(slice);
        }
    }
}
