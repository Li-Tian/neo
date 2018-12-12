using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.IO.Data.LevelDB
{
    /// <summary>
    /// Leveldb辅助方法
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// 批量删除Key
        /// </summary>
        /// <param name="batch">批量操作</param>
        /// <param name="prefix">待删除前缀</param>
        /// <param name="key">待删除的Key</param>
        public static void Delete(this WriteBatch batch, byte prefix, ISerializable key)
        {
            batch.Delete(SliceBuilder.Begin(prefix).Add(key));
        }

        /// <summary>
        /// 查询前缀匹配的元素
        /// </summary>
        /// <typeparam name="T">值泛型</typeparam>
        /// <param name="db">待查询db</param>
        /// <param name="options">读选项</param>
        /// <param name="prefix">待查询前缀</param>
        /// <returns>T列表</returns>
        public static IEnumerable<T> Find<T>(this DB db, ReadOptions options, byte prefix) where T : class, ISerializable, new()
        {
            return Find(db, options, SliceBuilder.Begin(prefix), (k, v) => v.ToArray().AsSerializable<T>());
        }

        /// <summary>
        /// 查询前缀匹配的元素
        /// </summary>
        /// <typeparam name="T">值泛型</typeparam>
        /// <param name="db">待查询db</param>
        /// <param name="options">读选项</param>
        /// <param name="prefix">待查询前缀</param>
        /// <param name="resultSelector">值处理回调函数</param>
        /// <returns>T列表</returns>
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

        /// <summary>
        /// 查询某个Key
        /// </summary>
        /// <typeparam name="T">值泛型</typeparam>
        /// <param name="db">待查询db</param>
        /// <param name="options">读选项</param>
        /// <param name="prefix">待查询前缀</param>
        /// <param name="key">待查询Key</param>
        /// <returns>T</returns>
        public static T Get<T>(this DB db, ReadOptions options, byte prefix, ISerializable key) where T : class, ISerializable, new()
        {
            return db.Get(options, SliceBuilder.Begin(prefix).Add(key)).ToArray().AsSerializable<T>();
        }


        /// <summary>
        /// 查询某个Key
        /// </summary>
        /// <typeparam name="T">值泛型</typeparam>
        /// <param name="db">待查询db</param>
        /// <param name="options">读选项</param>
        /// <param name="prefix">待查询前缀</param>
        /// <param name="key">待查询Key</param>
        /// <param name="resultSelector">值处理回调函数</param>
        /// <returns>T</returns>
        public static T Get<T>(this DB db, ReadOptions options, byte prefix, ISerializable key, Func<Slice, T> resultSelector)
        {
            return resultSelector(db.Get(options, SliceBuilder.Begin(prefix).Add(key)));
        }

        /// <summary>
        /// 批量写入键值
        /// </summary>
        /// <param name="batch">批量操作</param>
        /// <param name="prefix">前缀</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Put(this WriteBatch batch, byte prefix, ISerializable key, ISerializable value)
        {
            batch.Put(SliceBuilder.Begin(prefix).Add(key), value.ToArray());
        }

        /// <summary>
        /// 尝试获取Key
        /// </summary>
        /// <typeparam name="T">值泛型</typeparam>
        /// <param name="db">待查询db</param>
        /// <param name="options">读选项</param>
        /// <param name="prefix">待查询前缀</param>
        /// <param name="key">待查询Key</param>
        /// <returns>T，若不存在时，返回null</returns>
        public static T TryGet<T>(this DB db, ReadOptions options, byte prefix, ISerializable key) where T : class, ISerializable, new()
        {
            Slice slice;
            if (!db.TryGet(options, SliceBuilder.Begin(prefix).Add(key), out slice))
                return null;
            return slice.ToArray().AsSerializable<T>();
        }

        /// <summary>
        /// 尝试获取Key
        /// </summary>
        /// <typeparam name="T">值泛型</typeparam>
        /// <param name="db">待查询db</param>
        /// <param name="options">读选项</param>
        /// <param name="prefix">待查询前缀</param>
        /// <param name="key">待查询Key</param>
        /// <param name="resultSelector">值处理回调函数</param>
        /// <returns>T，若不存在时，返回null</returns>
        public static T TryGet<T>(this DB db, ReadOptions options, byte prefix, ISerializable key, Func<Slice, T> resultSelector) where T : class
        {
            Slice slice;
            if (!db.TryGet(options, SliceBuilder.Begin(prefix).Add(key), out slice))
                return null;
            return resultSelector(slice);
        }
    }
}
