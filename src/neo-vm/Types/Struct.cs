using System.Collections.Generic;

namespace Neo.VM.Types
{
    /// <summary>
    /// 继承Array类，定义了虚拟机Struct类型的相关方法
    /// </summary>
    public class Struct : Array
    {
        /// <summary>
        /// 无参构造函数，新建一个堆栈项列表实例
        /// </summary>
        public Struct() : this(new List<StackItem>()) { }
        /// <summary>
        /// 构造函数，调用Array的构造函数
        /// </summary>
        /// <param name="value">堆栈项集合</param>
        public Struct(IEnumerable<StackItem> value) : base(value)
        {
        }
        /// <summary>
        /// 复制Struct的数据给另一个Struct，深拷贝
        /// </summary>
        /// <returns>复制得到的Struct</returns>
        public Struct Clone()
        {
            Struct @struct = new Struct();
            Queue<Struct> queue = new Queue<Struct>();
            queue.Enqueue(@struct);
            queue.Enqueue(this);
            while (queue.Count > 0)
            {
                Struct a = queue.Dequeue();
                Struct b = queue.Dequeue();
                foreach (StackItem item in b)
                {
                    if (item is Struct sb)
                    {
                        Struct sa = new Struct();
                        a.Add(sa);
                        queue.Enqueue(sa);
                        queue.Enqueue(sb);
                    }
                    else
                    {
                        a.Add(item);
                    }
                }
            }
            return @struct;
        }
        /// <summary>
        /// 判断当前Struct与指定的堆栈项是否相等
        /// </summary>
        /// <param name="other">指定的堆栈项</param>
        /// <returns>相等则返回true，否则返回false</returns>
        public override bool Equals(StackItem other)
        {
            if (other is null) return false;
            Stack<StackItem> stack1 = new Stack<StackItem>();
            Stack<StackItem> stack2 = new Stack<StackItem>();
            stack1.Push(this);
            stack2.Push(other);
            while (stack1.Count > 0)
            {
                StackItem a = stack1.Pop();
                StackItem b = stack2.Pop();
                if (a is Struct sa)
                {
                    if (ReferenceEquals(a, b)) continue;
                    if (!(b is Struct sb)) return false;
                    if (sa.Count != sb.Count) return false;
                    foreach (StackItem item in sa)
                        stack1.Push(item);
                    foreach (StackItem item in sb)
                        stack2.Push(item);
                }
                else
                {
                    if (!a.Equals(b)) return false;
                }
            }
            return true;
        }
    }
}
