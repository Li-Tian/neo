using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neo.VM.Types
{
    /// <summary>
    /// 定义了虚拟机Array类型的相关方法
    /// </summary>
    public class Array : StackItem, ICollection, IList<StackItem>
    {
        /// <summary>
        /// 一个堆栈项列表
        /// </summary>
        protected readonly List<StackItem> _array;
        /// <summary>
        /// 索引器，用于对列表中指定索引的堆栈项项赋值和取值
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>对应的项</returns>
        public StackItem this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }
        /// <summary>
        /// Array的成员数量
        /// </summary>
        public int Count => _array.Count;
        /// <summary>
        /// Array是否为只读
        /// </summary>
        public bool IsReadOnly => false;

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => _array;
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public Array() : this(new List<StackItem>()) { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">堆栈项集合</param>
        public Array(IEnumerable<StackItem> value)
        {
            this._array = value as List<StackItem> ?? value.ToList();
        }
        /// <summary>
        /// 向Array中添加项
        /// </summary>
        /// <param name="item">需要添加的堆栈项类型对象</param>
        public void Add(StackItem item)
        {
            _array.Add(item);
        }
        /// <summary>
        /// 清空Array
        /// </summary>
        public void Clear()
        {
            _array.Clear();
        }
        /// <summary>
        /// 确定某项是否在Array中
        /// </summary>
        /// <param name="item">需要判断的项</param>
        /// <returns>是包含在Array中则返回true,否则返回false</returns>
        public bool Contains(StackItem item)
        {
            return _array.Contains(item);
        }

        void ICollection<StackItem>.CopyTo(StackItem[] array, int arrayIndex)
        {
            _array.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            foreach (StackItem item in _array)
                array.SetValue(item, index++);
        }
        /// <summary>
        /// 判断当前Array与指定对象是否相等
        /// </summary>
        /// <param name="other">指定对象，为堆栈项类型</param>
        /// <returns>相等则返回true，否则返回false</returns>
        public override bool Equals(StackItem other)
        {
            return ReferenceEquals(this, other);
        }
        /// <summary>
        /// 获取对应的布尔值
        /// </summary>
        /// <returns>返回true</returns>
        public override bool GetBoolean()
        {
            return true;
        }
        /// <summary>
        /// 获取对应的字节数组
        /// </summary>
        /// <returns>抛出异常</returns>
        public override byte[] GetByteArray()
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// 返回一个实现了IEnumerator接口的Array对象
        /// </summary>
        /// <returns>实现了IEnumerator接口的Array对象</returns>
        public IEnumerator<StackItem> GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        int IList<StackItem>.IndexOf(StackItem item)
        {
            return _array.IndexOf(item);
        }
        /// <summary>
        /// 将一个堆栈项插入Array指定索引处
        /// </summary>
        /// <param name="index">插入位置索引</param>
        /// <param name="item">插入堆栈项</param>
        public void Insert(int index, StackItem item)
        {
            _array.Insert(index, item);
        }

        bool ICollection<StackItem>.Remove(StackItem item)
        {
            return _array.Remove(item);
        }
        /// <summary>
        /// 移除Array指定索引处的元素
        /// </summary>
        /// <param name="index">位置索引</param>
        public void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }
        /// <summary>
        /// 将Array中元素的顺序反转
        /// </summary>
        public void Reverse()
        {
            _array.Reverse();
        }
    }
}
