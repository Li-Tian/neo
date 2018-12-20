using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neo.VM
{
    // <summary>
    // 随机访问栈类，定义了随机访问栈的基本结构和一些操作方法
    // 可用于实现虚拟的调用栈、计算栈等
    // </summary>
    // <typeparam name="T">泛型</typeparam>
    /// <summary>
    /// Random access stack class, defines the basic structure of the random access stack and some methods of operation
    /// Can be used to implement virtual InvocationStack, EvaluationStack, etc.
    /// </summary>
    /// <typeparam name="T">Generic</typeparam>
    public class RandomAccessStack<T> : IReadOnlyCollection<T>
    {
        private readonly List<T> list = new List<T>();
        // <summary>
        // 随机访问栈的元素数量
        // </summary>
        /// <summary>
        /// The number of elements in the RandomAccessStack
        /// </summary>
        public int Count => list.Count;
        // <summary>
        // 清空随机访问栈
        // </summary>
        /// <summary>
        /// Clear the RandomAccessStack
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }
        // <summary>
        // 将随机访问栈指定数量的元素复制到目标随机访问栈
        // </summary>
        // <param name="stack">目标随机访问栈</param>
        // <param name="count">需要复制的元素数量，默认值为-1，表示复制全部元素</param>
        /// <summary>
        /// Copy the specified number of elements of the RandomAccessStack to the target RandomAccessStack
        /// </summary>
        /// <param name="stack">target RandomAccessStack</param>
        /// <param name="count">The number of elements that need to be copied. The default value is -1, which means copying all elements.</param>
        public void CopyTo(RandomAccessStack<T> stack, int count = -1)
        {
            if (count == 0) return;
            if (count == -1)
                stack.list.AddRange(list);
            else
                stack.list.AddRange(list.Skip(list.Count - count));
        }
        // <summary>
        // 获取循环访问list&lt;T&gt;的枚举数。
        // </summary>
        // <returns>循环访问list&lt;T&gt;的枚举数。</returns>
        /// <summary>
        /// Get the enumerator of T
        /// </summary>
        /// <returns>the enumerator of T</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }
        // <summary>
        // 获取循环访问枚举数。
        // </summary>
        // <returns>循环访问枚举数。</returns>
        /// <summary>
        /// Get the enumerator
        /// </summary>
        /// <returns>the enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        // <summary>
        // 向随机访问栈中指定位置插入元素
        // </summary>
        // <param name="index">插入位置</param>
        // <param name="item">插入元素</param>
        // <exception cref="System.InvalidOperationException">插入位置大于随机访问栈中元素个数时抛出</exception>
        /// <summary>
        /// Insert an element into the specified location in the RandomAccessStack
        /// </summary>
        /// <param name="index">Insert position</param>
        /// <param name="item">Insert element</param>
        /// <exception cref="System.InvalidOperationException">The insertion position is greater than the number of elements in the RandomAccessStack</exception>
        public void Insert(int index, T item)
        {
            if (index > list.Count) throw new InvalidOperationException();
            list.Insert(list.Count - index, item);
        }
        // <summary>
        // 查看随机访问栈中指定位置的元素
        // </summary>
        // <param name="index">需要查看的位置，默认为0，表示栈顶元素</param>
        // <returns>指定位置的元素</returns>
        // <exception cref="System.InvalidOperationException">插入位置大于随机访问栈中元素个数或插入位置小于0时抛出</exception>
        /// <summary>
        /// View the elements of the specified location in the RandomAccessStack
        /// </summary>
        /// <param name="index">The location to view, the default is 0, indicating the top element of the stack</param>
        /// <returns>The element at the specified location</returns>
        /// <exception cref="System.InvalidOperationException">The insertion position is greater than the number of elements in the random access stack or the insertion position is less than 0</exception>
        public T Peek(int index = 0)
        {
            if (index >= list.Count) throw new InvalidOperationException();
            if (index < 0) index += list.Count;
            if (index < 0) throw new InvalidOperationException();
            index = list.Count - index - 1;
            return list[index];
        }
        // <summary>
        // 将栈顶元素出栈
        // </summary>
        // <returns>栈顶元素</returns>
        /// <summary>
        /// Pop the top element of the stack
        /// </summary>
        /// <returns>top element of the stack</returns>
        public T Pop()
        {
            return Remove(0);
        }
        // <summary>
        // 向随机访问栈中压入元素
        // </summary>
        // <param name="item">需要压栈的元素</param>
        /// <summary>
        /// Push elements into the RandomAccessStack
        /// </summary>
        /// <param name="item">Elements that need to be pushed</param>
        public void Push(T item)
        {
            list.Add(item);
        }
        // <summary>
        // 获取随机访问栈指定位置的元素，并从栈中移除这个元素
        // </summary>
        // <param name="index">元素所在位置</param>
        // <returns>获取到的元素</returns>
        // <exception cref="System.InvalidOperationException">指定位置大于等于随机访问栈中元素个数或小于0时抛出</exception>
        /// <summary>
        /// Get the element at the specified location of the RandomAccessStack and remove the element from the stack
        /// </summary>
        /// <param name="index">The index of element</param>
        /// <returns>Result element</returns>
        /// <exception cref="System.InvalidOperationException">The specified position is greater than or equal to the number of elements in the RandomAccessStack or less than 0.</exception>
        public T Remove(int index)
        {
            if (index >= list.Count) throw new InvalidOperationException();
            if (index < 0) index += list.Count;
            if (index < 0) throw new InvalidOperationException();
            index = list.Count - index - 1;
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }
        // <summary>
        // 对随机访问栈指定位置的元素赋值
        // </summary>
        // <param name="index">需要赋值的元素位置</param>
        // <param name="item">需要赋的值</param>
        // <exception cref="System.InvalidOperationException">指定位置大于等于随机访问栈中元素个数或小于0时抛出</exception>
        /// <summary>
        /// Assign values to elements of the specified location of the RandomAccessStack
        /// </summary>
        /// <param name="index">The position of the element that needs to be assigned</param>
        /// <param name="item">Value to be assigned</param>
        /// <exception cref="System.InvalidOperationException">The specified position is greater than or equal to the number of elements in the random access stack or less than 0.</exception>
        public void Set(int index, T item)
        {
            if (index >= list.Count) throw new InvalidOperationException();
            if (index < 0) index += list.Count;
            if (index < 0) throw new InvalidOperationException();
            index = list.Count - index - 1;
            list[index] = item;
        }
    }
}
