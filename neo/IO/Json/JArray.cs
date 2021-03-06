﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Neo.IO.Json
{
    /// <summary>
    /// JObject类的子类，重写了JObject中的一部分方法，用于构建json字符中的数组对象
    /// </summary>
    public class JArray : JObject, IList<JObject>
    {
        private List<JObject> items = new List<JObject>();
        /// <summary>
        /// JArray的构造函数，用于构建JArray对象，使用枚举类型的参数
        /// </summary>
        /// <param name="items">需要存储的元素</param>
        public JArray(params JObject[] items) : this((IEnumerable<JObject>)items)
        {
        }
        /// <summary>
        /// JArray的构造函数，用于构建JArray对象，使用数组形式的参数
        /// </summary>
        /// <param name="items">需要存储的元素</param>
        public JArray(IEnumerable<JObject> items)
        {
            this.items.AddRange(items);
        }
        /// <summary>
        /// 对JArray对象set、get方法的扩展，使外部方法可以便捷的操作其内部元素
        /// </summary>
        /// <param name="index">要获取或设置的元素的索引</param>
        /// <returns>对应的元素</returns>
        public JObject this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }
        /// <summary>
        /// JArray对象内部元素的个数
        /// </summary>
        public int Count
        {
            get
            {
                return items.Count;
            }
        }
        /// <summary>
        /// 只读标志位，默认为false
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 向JArray对象内部添加元素
        /// </summary>
        /// <param name="item">需要添加的元素</param>
        public void Add(JObject item)
        {
            items.Add(item);
        }
        /// <summary>
        /// 清空JArray对象内部元素
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }
        /// <summary>
        /// 判断JArray对象内部是否包含某个元素
        /// </summary>
        /// <param name="item">需要检查的元素</param>
        /// <returns>判断结果，包含返回true,否则返回false</returns>
        public bool Contains(JObject item)
        {
            return items.Contains(item);
        }
        /// <summary>
        /// JArray对象拷贝方法
        /// </summary>
        /// <param name="array">需要拷贝到的目标数组</param>
        /// <param name="arrayIndex">目标数组中拷贝的开始位置</param>
        public void CopyTo(JObject[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// JArray对象的枚举器，读取JArray对象的枚举结果
        /// </summary>
        /// <returns>Array对象的枚举结果</returns>
        public IEnumerator<JObject> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// 获取某个元素在JArray对象中的索引
        /// </summary>
        /// <param name="item">待查询的元素</param>
        /// <returns>对应的索引</returns>
        public int IndexOf(JObject item)
        {
            return items.IndexOf(item);
        }
        /// <summary>
        /// 向JArray对象内部指定索引位置添加元素
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <param name="item">待添加元素</param>
        public void Insert(int index, JObject item)
        {
            items.Insert(index, item);
        }
        /// <summary>
        /// 解析文本读取器中的数据，用文本读取器中的数据构建对应的JArray对象
        /// </summary>
        /// <param name="reader">文本读取器</param>
        /// <param name="max_nest">最大嵌套层数</param>
        /// <returns>输出由文本读取器中的数据构建的JArray对象</returns>
        internal new static JArray Parse(TextReader reader, int max_nest)
        {
            if (max_nest < 0) throw new FormatException();
            SkipSpace(reader);
            if (reader.Read() != '[') throw new FormatException();
            SkipSpace(reader);
            JArray array = new JArray();
            while (reader.Peek() != ']')
            {
                if (reader.Peek() == ',') reader.Read();
                JObject obj = JObject.Parse(reader, max_nest - 1);
                array.items.Add(obj);
                SkipSpace(reader);
            }
            reader.Read();
            return array;
        }
        /// <summary>
        /// 移除JArray对象中某个元素
        /// </summary>
        /// <param name="item">待移除的元素</param>
        /// <returns>移除成功返回true,否则返回false</returns>
        public bool Remove(JObject item)
        {
            return items.Remove(item);
        }
        /// <summary>
        /// 移除JArray对象内部指定索引的元素
        /// </summary>
        /// <param name="index">指定索引参数</param>
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        /// <summary>
        /// JArray对象转换成String类型数据
        /// </summary>
        /// <returns>输出Array对象转换成的String类型数据</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (JObject item in items)
            {
                if (item == null)
                    sb.Append("null");
                else
                    sb.Append(item);
                sb.Append(',');
            }
            if (items.Count == 0)
            {
                sb.Append(']');
            }
            else
            {
                sb[sb.Length - 1] = ']';
            }
            return sb.ToString();
        }
    }
}
