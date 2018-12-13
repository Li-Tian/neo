using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.IO.Caching
{
    /// <summary>
    /// 数据缓存类，抽象类。定义了数据缓存需要实现的相关方法
    /// </summary>
    /// <typeparam name="TKey">泛型键</typeparam>
    /// <typeparam name="TValue">泛型值</typeparam>
    public abstract class DataCache<TKey, TValue>
        where TKey : IEquatable<TKey>, ISerializable
        where TValue : class, ICloneable<TValue>, ISerializable, new()
    {
        /// <summary>
        /// 可追踪类
        /// </summary>
        public class Trackable
        {
            /// <summary>
            /// 泛型键
            /// </summary>
            public TKey Key;
            /// <summary>
            /// 泛型值
            /// </summary>
            public TValue Item;
            /// <summary>
            /// 追踪状态
            /// </summary>
            public TrackState State;
        }

        private readonly Dictionary<TKey, Trackable> dictionary = new Dictionary<TKey, Trackable>();
        /// <summary>
        /// 定义一个索引器，便于直接操作字典中元素
        /// </summary>
        /// <param name="key">索引器参数</param>
        /// <returns>索引对应的TValue</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">若字典中存在索引参数对应的元素，且元素的状态为delete时抛出</exception>
        public TValue this[TKey key]
        {
            get
            {
                lock (dictionary)
                {
                    if (dictionary.TryGetValue(key, out Trackable trackable))
                    {
                        if (trackable.State == TrackState.Deleted)
                            throw new KeyNotFoundException();
                    }
                    else
                    {
                        trackable = new Trackable
                        {
                            Key = key,
                            Item = GetInternal(key),
                            State = TrackState.None
                        };
                        dictionary.Add(key, trackable);
                    }
                    return trackable.Item;
                }
            }
        }
        /// <summary>
        /// 向dictionary中新增一对键值对，如果trackable已存在则会修改状态
        /// </summary>
        /// <param name="key">泛型键</param>
        /// <param name="value">泛型值</param>
        /// <exception cref="System.ArgumentException">字典中存在键所对应的值，且值的状态不为deleted时抛出</exception>
        public void Add(TKey key, TValue value)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out Trackable trackable) && trackable.State != TrackState.Deleted)
                    throw new ArgumentException();
                dictionary[key] = new Trackable
                {
                    Key = key,
                    Item = value,
                    State = trackable == null ? TrackState.Added : TrackState.Changed
                };
            }
        }
        /// <summary>
        /// 把键值对添加至内部
        /// </summary>
        /// <param name="key">泛型键</param>
        /// <param name="value">泛型值</param>
        protected abstract void AddInternal(TKey key, TValue value);
        /// <summary>
        /// 提交所有更改
        /// </summary>
        public void Commit()
        {
            foreach (Trackable trackable in GetChangeSet())
                switch (trackable.State)
                {
                    case TrackState.Added:
                        AddInternal(trackable.Key, trackable.Item);
                        break;
                    case TrackState.Changed:
                        UpdateInternal(trackable.Key, trackable.Item);
                        break;
                    case TrackState.Deleted:
                        DeleteInternal(trackable.Key);
                        break;
                }
        }
        /// <summary>
        /// 创建快照
        /// </summary>
        /// <returns>CloneCache类型快照</returns>
        public DataCache<TKey, TValue> CreateSnapshot()
        {
            return new CloneCache<TKey, TValue>(this);
        }
        /// <summary>
        /// 删除dictionary中指定键对应的项
        /// </summary>
        /// <param name="key">泛型键</param>
        public void Delete(TKey key)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out Trackable trackable))
                {
                    if (trackable.State == TrackState.Added)
                        dictionary.Remove(key);
                    else
                        trackable.State = TrackState.Deleted;
                }
                else
                {
                    TValue item = TryGetInternal(key);
                    if (item == null) return;
                    dictionary.Add(key, new Trackable
                    {
                        Key = key,
                        Item = item,
                        State = TrackState.Deleted
                    });
                }
            }
        }
        /// <summary>
        /// 内部删除
        /// </summary>
        /// <param name="key">泛型键</param>
        public abstract void DeleteInternal(TKey key);
        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <param name="predicate">键值对需要满足的条件</param>
        public void DeleteWhere(Func<TKey, TValue, bool> predicate)
        {
            lock (dictionary)
            {
                foreach (Trackable trackable in dictionary.Where(p => p.Value.State != TrackState.Deleted && predicate(p.Key, p.Value.Item)).Select(p => p.Value))
                    trackable.State = TrackState.Deleted;
            }
        }
        /// <summary>
        /// 根据指定前缀查找键值对
        /// </summary>
        /// <param name="key_prefix">键的前缀</param>
        /// <returns>满足条件的键值对</returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> Find(byte[] key_prefix = null)
        {
            lock (dictionary)
            {
                foreach (var pair in FindInternal(key_prefix ?? new byte[0]))
                    if (!dictionary.ContainsKey(pair.Key))
                        yield return pair;
                foreach (var pair in dictionary)
                    if (pair.Value.State != TrackState.Deleted && (key_prefix == null || pair.Key.ToArray().Take(key_prefix.Length).SequenceEqual(key_prefix)))
                        yield return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Item);
            }
        }
        /// <summary>
        /// 内部查找
        /// </summary>
        /// <param name="key_prefix">key的前缀</param>
        /// <returns>满足前缀的IEnumerable类型键值对</returns>
        protected abstract IEnumerable<KeyValuePair<TKey, TValue>> FindInternal(byte[] key_prefix);
        /// <summary>
        /// 获取dictionary中所有状态不为None的trackable
        /// </summary>
        /// <returns>状态不为None的trackable</returns>
        public IEnumerable<Trackable> GetChangeSet()
        {
            lock (dictionary)
            {
                foreach (Trackable trackable in dictionary.Values.Where(p => p.State != TrackState.None))
                    yield return trackable;
            }
        }
        /// <summary>
        /// 内部查询指定键
        /// </summary>
        /// <param name="key">需要查询的泛型键</param>
        /// <returns>对应的值</returns>
        protected abstract TValue GetInternal(TKey key);
        /// <summary>
        /// 根据键获取对应trackable，并执行相应操作
        /// </summary>
        /// <param name="key">需要查询的键</param>
        /// <param name="factory">需要执行的操作</param>
        /// <returns>对应操作完成的值</returns>
        public TValue GetAndChange(TKey key, Func<TValue> factory = null)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out Trackable trackable))
                {
                    if (trackable.State == TrackState.Deleted)
                    {
                        if (factory == null) throw new KeyNotFoundException();
                        trackable.Item = factory();
                        trackable.State = TrackState.Changed;
                    }
                    else if (trackable.State == TrackState.None)
                    {
                        trackable.State = TrackState.Changed;
                    }
                }
                else
                {
                    trackable = new Trackable
                    {
                        Key = key,
                        Item = TryGetInternal(key)
                    };
                    if (trackable.Item == null)
                    {
                        if (factory == null) throw new KeyNotFoundException();
                        trackable.Item = factory();
                        trackable.State = TrackState.Added;
                    }
                    else
                    {
                        trackable.State = TrackState.Changed;
                    }
                    dictionary.Add(key, trackable);
                }
                return trackable.Item;
            }
        }
        /// <summary>
        /// 根据键获取对应trackable，并执行相应操作。如果对应的trackable不存在，则新建一个trackable并执行相应操作
        /// </summary>
        /// <param name="key">需要查询的键</param>
        /// <param name="factory">需要执行的操作</param>
        /// <returns>对应操作完成的值</returns>
        public TValue GetOrAdd(TKey key, Func<TValue> factory)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out Trackable trackable))
                {
                    if (trackable.State == TrackState.Deleted)
                    {
                        trackable.Item = factory();
                        trackable.State = TrackState.Changed;
                    }
                }
                else
                {
                    trackable = new Trackable
                    {
                        Key = key,
                        Item = TryGetInternal(key)
                    };
                    if (trackable.Item == null)
                    {
                        trackable.Item = factory();
                        trackable.State = TrackState.Added;
                    }
                    else
                    {
                        trackable.State = TrackState.None;
                    }
                    dictionary.Add(key, trackable);
                }
                return trackable.Item;
            }
        }
        /// <summary>
        /// 根据键获取对应trackable，如果dictionary查找不到则会调用TryGetInternal查找
        /// </summary>
        /// <param name="key">需要查询的键</param>
        /// <returns>查询到的值</returns>
        public TValue TryGet(TKey key)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out Trackable trackable))
                {
                    if (trackable.State == TrackState.Deleted) return null;
                    return trackable.Item;
                }
                TValue value = TryGetInternal(key);
                if (value == null) return null;
                dictionary.Add(key, new Trackable
                {
                    Key = key,
                    Item = value,
                    State = TrackState.None
                });
                return value;
            }
        }
        /// <summary>
        /// 尝试从内部查询键对应的值
        /// </summary>
        /// <param name="key">需要查询的键</param>
        /// <returns>查询到的值</returns>
        protected abstract TValue TryGetInternal(TKey key);
        /// <summary>
        /// 更新内部键值对
        /// </summary>
        /// <param name="key">需要更新的键</param>
        /// <param name="value">需要更新的值</param>
        protected abstract void UpdateInternal(TKey key, TValue value);
    }
}
