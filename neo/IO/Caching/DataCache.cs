using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.IO.Caching
{
    // <summary>
    // 数据缓存类，抽象类。定义了数据缓存需要实现的相关方法
    // </summary>
    // <typeparam name="TKey">键</typeparam>
    // <typeparam name="TValue">值</typeparam>
    /// <summary>
    /// DataCache class, abstract class. Defines the relevant methods that DataCache needs to implement
    /// </summary>
    /// <typeparam name="TKey">key</typeparam>
    /// <typeparam name="TValue">value</typeparam>
    public abstract class DataCache<TKey, TValue>
        where TKey : IEquatable<TKey>, ISerializable
        where TValue : class, ICloneable<TValue>, ISerializable, new()
    {
        // <summary>
        // 可追踪类
        // </summary>
        /// <summary>
        /// Trackable class
        /// </summary>
        public class Trackable
        {
            // <summary>
            // 泛型键
            // </summary>
            /// <summary>
            /// TKey
            /// </summary>
            public TKey Key;
            // <summary>
            // 泛型值
            // </summary>
            /// <summary>
            /// TValue
            /// </summary>
            public TValue Item;
            // <summary>
            // 追踪状态
            // </summary>
            /// <summary>
            /// TrackState
            /// </summary>
            public TrackState State;
        }

        private readonly Dictionary<TKey, Trackable> dictionary = new Dictionary<TKey, Trackable>();
        // <summary>
        // 定义一个索引器，便于直接操作字典中元素
        // </summary>
        // <param name="key">索引器参数</param>
        // <returns>索引对应的TValue</returns>
        // <exception cref="System.Collections.Generic.KeyNotFoundException">若字典中存在索引参数对应的元素，且元素的状态为delete时抛出</exception>
        /// <summary>
        /// Define an indexer
        /// </summary>
        /// <param name="key">Indexer parameter</param>
        /// <returns>The TValue corresponding to the index</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if there is an element corresponding to the index parameter in the dictionary and the state of the element is deleted</exception>
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
        // <summary>
        // 向dictionary中新增一对键值对，如果trackable已存在则会修改状态
        // </summary>
        // <param name="key">键</param>
        // <param name="value">值</param>
        // <exception cref="System.ArgumentException">字典中存在键所对应的值，且值的状态不为deleted时抛出</exception>
        /// <summary>
        /// Add a key-value pair to the dictionary. If the trackable already exists, the status will be modified.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <exception cref="System.ArgumentException">The value corresponding to the key exists in the dictionary, and the state of the value is not deleted.</exception>
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
        // <summary>
        // 把键值对添加至内部
        // </summary>
        // <param name="key">键</param>
        // <param name="value">值</param>
        /// <summary>
        /// Add key-value pairs to the internal
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected abstract void AddInternal(TKey key, TValue value);
        // <summary>
        // 提交所有更改。行为受子类的实现影响。
        // </summary>
        /// <summary>
        /// Commit all changes. Behavior is affected by the implementation of subclasses.
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
        // <summary>
        // 创建快照
        // </summary>
        // <returns>CloneCache类型快照</returns>
        /// <summary>
        /// Create a snapshot
        /// </summary>
        /// <returns>CloneCache type snapshot</returns>
        public DataCache<TKey, TValue> CreateSnapshot()
        {
            return new CloneCache<TKey, TValue>(this);
        }
        // <summary>
        // 删除dictionary中指定键对应的项
        // </summary>
        // <param name="key">键</param>
        /// <summary>
        /// Delete the item corresponding to the specified key in the dictionary
        /// </summary>
        /// <param name="key">key</param>
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
        // <summary>
        // 内部删除
        // </summary>
        // <param name="key">键</param>
        /// <summary>
        /// Internal deletion
        /// </summary>
        /// <param name="key">key</param>
        public abstract void DeleteInternal(TKey key);
        // <summary>
        // 按条件删除
        // </summary>
        // <param name="predicate">键值对需要满足的条件判定函数</param>
        /// <summary>
        /// Delete by condition
        /// </summary>
        /// <param name="predicate">Conditional decision function</param>
        public void DeleteWhere(Func<TKey, TValue, bool> predicate)
        {
            lock (dictionary)
            {
                foreach (Trackable trackable in dictionary.Where(p => p.Value.State != TrackState.Deleted && predicate(p.Key, p.Value.Item)).Select(p => p.Value))
                    trackable.State = TrackState.Deleted;
            }
        }
        // <summary>
        // 根据指定前缀查找键值对
        // </summary>
        // <param name="key_prefix">键的前缀。不指定时返回所有键值对。</param>
        // <returns>满足条件的键值对</returns>
        /// <summary>
        /// Find key-value pairs based on the specified prefix
        /// </summary>
        /// <param name="key_prefix">The prefix of the key. Returns all key-value pairs when not specified.</param>
        /// <returns>Key-value pairs that satisfy the condition</returns>
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
        // <summary>
        // 内部查找
        // </summary>
        // <param name="key_prefix">key的前缀</param>
        // <returns>满足前缀的IEnumerable类型键值对</returns>
        /// <summary>
        /// Find internal
        /// </summary>
        /// <param name="key_prefix">The prefix of the key. </param>
        /// <returns>IEnumerable type key-value pair that satisfies the prefix</returns>
        protected abstract IEnumerable<KeyValuePair<TKey, TValue>> FindInternal(byte[] key_prefix);
        // <summary>
        // 获取dictionary中所有状态不为None的trackable
        // </summary>
        // <returns>状态不为None的trackable</returns>
        /// <summary>
        /// Get all trackable in the dictionary whose status is not None
        /// </summary>
        /// <returns>Trackable with a status of None</returns>
        public IEnumerable<Trackable> GetChangeSet()
        {
            lock (dictionary)
            {
                foreach (Trackable trackable in dictionary.Values.Where(p => p.State != TrackState.None))
                    yield return trackable;
            }
        }
        // <summary>
        // 内部查询指定键
        // </summary>
        // <param name="key">需要查询的键</param>
        // <returns>对应的值</returns>
        /// <summary>
        /// Internal query specified key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        protected abstract TValue GetInternal(TKey key);
        // <summary>
        // 根据 key 获取对应的 value 对象，并将这个键值对标记为有发生更改。
        // </summary>
        // <param name="key">键</param>
        // <param name="factory">如果缓冲区没有指定的值，或者已经被删除时，通过factory重新创建值的实例</param>
        // <returns>指定的键所对应的值的实例</returns>
        // <exception cref="KeyNotFoundException">指定的key不存在或者已经被删除，而且factory未被指定，无法重新创建值对象时抛出此异常。</exception>
        /// <summary>
        /// Gets the corresponding value object based on key and marks this key-value pair as having changed.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="factory">Recreate an instance of a value by factory if the buffer does not have a value specified or has been deleted</param>
        /// <returns>An instance of the value corresponding to the specified key</returns>
        /// <exception cref="KeyNotFoundException">The specified key does not exist or has been deleted, and the factory is not specified, and the value object cannot be recreated.</exception>
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
        // <summary>
        // 根据键获取对应值。如果不存在则重新创建实例。
        // </summary>
        // <param name="key">需要查询的键</param>
        // <param name="factory">不存在时，通过factory创建实例</param>
        // <returns>对应的值</returns>
        /// <summary>
        /// Get the corresponding value according to the key. Recreate the instance if it does not exist.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="factory">Create an instance via factory when value does not exist</param>
        /// <returns>corresponding value</returns>
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
        // <summary>
        // 根据键获取对应的值，如果缓冲区内查找不到则会调用TryGetInternal查找
        // </summary>
        // <param name="key">需要查询的键</param>
        // <returns>查询到的值</returns>
        /// <summary>
        /// Get the corresponding value according to the key, if the buffer does not find it, call TryGetInternal to find
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Queryed value</returns>
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
        // <summary>
        // 尝试从内部查询键对应的值
        // </summary>
        // <param name="key">需要查询的键</param>
        // <returns>查询到的值</returns>
        /// <summary>
        /// Try to get the value corresponding to the key from the internal
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Queryed value</returns>
        protected abstract TValue TryGetInternal(TKey key);
        // <summary>
        // 更新内部键值对
        // </summary>
        // <param name="key">需要更新的键</param>
        // <param name="value">需要更新的值</param>
        /// <summary>
        /// Update internal key-value pair
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        protected abstract void UpdateInternal(TKey key, TValue value);
    }
}
