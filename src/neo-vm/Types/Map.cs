using System;
using System.Collections;
using System.Collections.Generic;

namespace Neo.VM.Types
{
    /// <summary>
    /// 定义了虚拟机Map类型的相关方法
    /// </summary>
    public class Map : StackItem, ICollection, IDictionary<StackItem, StackItem>
    {
        private readonly Dictionary<StackItem, StackItem> dictionary;
        /// <summary>
        /// 用于对字典中指定索引的堆栈项项赋值和取值
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <returns>对应的项</returns>
        public StackItem this[StackItem key]
        {
            get => this.dictionary[key];
            set => this.dictionary[key] = value;
        }
        /// <summary>
        /// Map中所有键的集合
        /// </summary>
        public ICollection<StackItem> Keys => dictionary.Keys;
        /// <summary>
        /// Map中所有值的集合
        /// </summary>
        public ICollection<StackItem> Values => dictionary.Values;
        /// <summary>
        /// Map中键值对的数量
        /// </summary>
        public int Count => dictionary.Count;
        /// <summary>
        /// Map是否为只读
        /// </summary>
        public bool IsReadOnly => false;

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => dictionary;
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public Map() : this(new Dictionary<StackItem, StackItem>()) { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">字典类型，键值均为堆栈项类型</param>
        public Map(Dictionary<StackItem, StackItem> value)
        {
            this.dictionary = value;
        }
        /// <summary>
        /// 向Map中添加一对键值对
        /// </summary>
        /// <param name="key">键，堆栈项类型</param>
        /// <param name="value">值，堆栈项类型</param>
        public void Add(StackItem key, StackItem value)
        {
            dictionary.Add(key, value);
        }

        void ICollection<KeyValuePair<StackItem, StackItem>>.Add(KeyValuePair<StackItem, StackItem> item)
        {
            dictionary.Add(item.Key, item.Value);
        }
        /// <summary>
        /// 清空Map
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        bool ICollection<KeyValuePair<StackItem, StackItem>>.Contains(KeyValuePair<StackItem, StackItem> item)
        {
            return dictionary.ContainsKey(item.Key);
        }
        /// <summary>
        /// 确定Map是否包含指定键
        /// </summary>
        /// <param name="key">指定键，堆栈项类型</param>
        /// <returns>包含则返回true，否则返回false</returns>
        public bool ContainsKey(StackItem key)
        {
            return dictionary.ContainsKey(key);
        }

        void ICollection<KeyValuePair<StackItem, StackItem>>.CopyTo(KeyValuePair<StackItem, StackItem>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<StackItem, StackItem> item in dictionary)
                array[arrayIndex++] = item;
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            foreach (KeyValuePair<StackItem, StackItem> item in dictionary)
                array.SetValue(item, index++);
        }
        /// <summary>
        /// 判断当前Map与指定的堆栈项是否相等
        /// </summary>
        /// <param name="other">指定的堆栈项</param>
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
        /// <exception cref="System.NotSupportedException">默认直接抛出</exception>
        public override byte[] GetByteArray()
        {
            throw new NotSupportedException();
        }

        IEnumerator<KeyValuePair<StackItem, StackItem>> IEnumerable<KeyValuePair<StackItem, StackItem>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
        /// <summary>
        /// 将带有指定键的值从Map中移除
        /// </summary>
        /// <param name="key">要移除元素的键</param>
        /// <returns>如果成功找到并移除该元素，则为true；否则为false。</returns>
        public bool Remove(StackItem key)
        {
            return dictionary.Remove(key);
        }

        bool ICollection<KeyValuePair<StackItem, StackItem>>.Remove(KeyValuePair<StackItem, StackItem> item)
        {
            return dictionary.Remove(item.Key);
        }
        /// <summary>
        /// 获取与指定键关联的值
        /// </summary>
        /// <param name="key">要获取的值的键。</param>
        /// <param name="value">当此方法返回时，如果找到指定键，则包含与该键相关的值；否则包含 value 参数类型的默认值。</param>
        /// <returns>如果包含具有指定键的元素则返回true，否则返回false</returns>
        public bool TryGetValue(StackItem key, out StackItem value)
        {
            return dictionary.TryGetValue(key, out value);
        }
    }
}
