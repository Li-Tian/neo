using System.Collections;
using System.Linq;

namespace Neo.Cryptography
{
    /// <summary>
    /// 实现了Bloom Filter的类
    /// </summary>
    public class BloomFilter
    {
        private readonly uint[] seeds;
        private readonly BitArray bits;
        /// <summary>
        /// 互相独立的哈希函数的个数
        /// </summary>
        public int K => seeds.Length;

        /// <summary>
        /// 位阵列的长度
        /// </summary>
        public int M => bits.Length;

        /// <summary>
        /// 产生哈希函数种子的微调参数
        /// </summary>
        public uint Tweak { get; private set; }

        /// <summary>
        /// 初始化Bloom filter. 0xFBA4C795 in decimal is 4221880213. 
        /// </summary>
        /// <param name="m">位阵列的长度</param>
        /// <param name="k">互相独立的哈希函数的个数</param>
        /// <param name="nTweak">微调参数</param>
        /// <param name="elements">初始化的位阵列数据</param>
        public BloomFilter(int m, int k, uint nTweak, byte[] elements = null)
        {
            this.seeds = Enumerable.Range(0, k).Select(p => (uint)p * 0xFBA4C795 + nTweak).ToArray();
            this.bits = elements == null ? new BitArray(m) : new BitArray(elements);
            this.bits.Length = m;
            this.Tweak = nTweak;
        }
        /// <summary>
        /// 向过滤器的集合添加一个新的元素
        /// </summary>
        /// <param name="element">被添加的新元素</param>
        public void Add(byte[] element)
        {
            foreach (uint i in seeds.AsParallel().Select(s => element.Murmur32(s)))
                bits.Set((int)(i % (uint)bits.Length), true);
        }

        /// <summary>
        /// 检测一个元素是否在集合内
        /// </summary>
        /// <param name="element">被检测的元素</param>
        /// <returns>如果元素不在集合内返回<c>true</c>,否则返回<c>false</c></returns>
        public bool Check(byte[] element)
        {
            foreach (uint i in seeds.AsParallel().Select(s => element.Murmur32(s)))
                if (!bits.Get((int)(i % (uint)bits.Length)))
                    return false;
            return true;
        }

        /// <summary>
        /// 将过滤器中的位阵列BitArray转换为一个一维的字节数组
        /// </summary>
        /// <param name="newBits">被存入的字节数组</param>
        public void GetBits(byte[] newBits)
        {
            bits.CopyTo(newBits, 0);
        }
    }
}
