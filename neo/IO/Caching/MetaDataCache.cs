﻿using System;

namespace Neo.IO.Caching
{
    /// <summary>
    /// 元数据缓存类，抽象类。定义了元数据缓存需要实现的相关方法
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public abstract class MetaDataCache<T>
        where T : class, ICloneable<T>, ISerializable, new()
    {
        private T Item;
        private TrackState State;
        private readonly Func<T> factory;
        /// <summary>
        /// 向内部添加项
        /// </summary>
        /// <param name="item">需要添加的项</param>
        protected abstract void AddInternal(T item);
        /// <summary>
        /// 尝试从内部获取
        /// </summary>
        /// <returns>获取的项</returns>
        protected abstract T TryGetInternal();
        /// <summary>
        /// 从内部更新
        /// </summary>
        /// <param name="item">需要更新的项</param>
        protected abstract void UpdateInternal(T item);
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="factory">factory对象</param>
        protected MetaDataCache(Func<T> factory)
        {
            this.factory = factory;
        }
        /// <summary>
        /// 提交所有更改。
        /// </summary>
        /// <remarks>
        /// 如果对象被标记为Added或者Changed，将被提交（写回文件）。
        /// </remarks>
        public void Commit()
        {
            switch (State)
            {
                case TrackState.Added:
                    AddInternal(Item);
                    break;
                case TrackState.Changed:
                    UpdateInternal(Item);
                    break;
            }
        }
        /// <summary>
        /// 创建快照CloneMetaCache
        /// </summary>
        /// <returns>创建完成的快照</returns>
        public MetaDataCache<T> CreateSnapshot()
        {
            return new CloneMetaCache<T>(this);
        }
        /// <summary>
        /// 获取项，如果不存在则尝试从内部获取，若仍不存在则新建一个项
        /// </summary>
        /// <returns>获得的项</returns>
        public T Get()
        {
            if (Item == null)
            {
                Item = TryGetInternal();
            }
            if (Item == null)
            {
                Item = factory?.Invoke() ?? new T();
                State = TrackState.Added;
            }
            return Item;
        }
        /// <summary>
        /// 获取项，如果是新建项则将其状态设为Changed。提交时会被写回。
        /// </summary>
        /// <returns>获得的项</returns>
        public T GetAndChange()
        {
            T item = Get();
            if (State == TrackState.None)
                State = TrackState.Changed;
            return item;
        }
    }
}
