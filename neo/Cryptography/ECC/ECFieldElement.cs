using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Neo.Cryptography.ECC
{
    /// <summary>
    /// 作为椭圆曲线上一个坐标点的一个filed, 其中用一个BigInteger来代表在相对应坐标上的值.
    /// </summary>
    internal class ECFieldElement : IComparable<ECFieldElement>, IEquatable<ECFieldElement>
    {
        internal readonly BigInteger Value;
        private readonly ECCurve curve;
        /// <summary>
        /// 构造函数，传入一个坐标值和一个椭圆曲线
        /// </summary>
        /// <param name="value">坐标值</param>
        /// <param name="curve">椭圆曲线</param>
        /// <exception cref="ArgumentException">如果该坐标值太大</exception>
        public ECFieldElement(BigInteger value, ECCurve curve)
        {
            if (value >= curve.Q)
                throw new ArgumentException("x value too large in field element");
            this.Value = value;
            this.curve = curve;
        }

        /// <summary>
        /// 与另一个ECFieldElement对象比较， 如果是相同的引用返回0， 如果不同则返回比较对象中value值的结果
        /// </summary>
        /// <param name="other">另一个被比较的对象</param>
        /// <returns>
        /// 如果两个ECFieldElement为同一个则返回0.
        /// 否则比较两个对象value的值,如果相同返回0, 如果当前ECFieldElement对象的value值大于被比较的value返回1, 否则返回-1
        /// </returns>
        public int CompareTo(ECFieldElement other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// 将这个ECFieldElement对象与另一个Object做比较
        /// </summary>
        /// <param name="obj">另一个待比较的对象</param>
        /// <returns>如果对象是一个引用则返回<c>true</c>.
        /// 如果是另一个对象不是ECFieldElement,返回<c>false</c>
        /// 否则， 调用Equals比较两个ECFieldElement对象的值
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            ECFieldElement other = obj as ECFieldElement;

            if (other == null)
                return false;

            return Equals(other);
        }

        /// <summary>
        /// 比较两个ECFieldElement对象的value值
        /// </summary>
        /// <param name="other">另一个ECFieldElement对象</param>
        /// <returns>如果该ECFieldElement对象的value值等于另一个ECFieldElement对象的value则返回<c>true</c>, 如果不相等则返回<c>false</c></returns>
        public bool Equals(ECFieldElement other)
        {
            return Value.Equals(other.Value);
        }

        /// <summary>
        /// 返回一个卢卡斯数列
        /// </summary>
        /// <param name="p"></param>
        /// <param name="P"></param>
        /// <param name="Q"></param>
        /// <param name="k"></param>
        /// <returns>生成的卢卡斯序列</returns>
        private static BigInteger[] FastLucasSequence(BigInteger p, BigInteger P, BigInteger Q, BigInteger k)
        {
            int n = k.GetBitLength();
            int s = k.GetLowestSetBit();

            Debug.Assert(k.TestBit(s));

            BigInteger Uh = 1;
            BigInteger Vl = 2;
            BigInteger Vh = P;
            BigInteger Ql = 1;
            BigInteger Qh = 1;

            for (int j = n - 1; j >= s + 1; --j)
            {
                Ql = (Ql * Qh).Mod(p);

                if (k.TestBit(j))
                {
                    Qh = (Ql * Q).Mod(p);
                    Uh = (Uh * Vh).Mod(p);
                    Vl = (Vh * Vl - P * Ql).Mod(p);
                    Vh = ((Vh * Vh) - (Qh << 1)).Mod(p);
                }
                else
                {
                    Qh = Ql;
                    Uh = (Uh * Vl - Ql).Mod(p);
                    Vh = (Vh * Vl - P * Ql).Mod(p);
                    Vl = ((Vl * Vl) - (Ql << 1)).Mod(p);
                }
            }

            Ql = (Ql * Qh).Mod(p);
            Qh = (Ql * Q).Mod(p);
            Uh = (Uh * Vl - Ql).Mod(p);
            Vl = (Vh * Vl - P * Ql).Mod(p);
            Ql = (Ql * Qh).Mod(p);

            for (int j = 1; j <= s; ++j)
            {
                Uh = Uh * Vl * p;
                Vl = ((Vl * Vl) - (Ql << 1)).Mod(p);
                Ql = (Ql * Ql).Mod(p);
            }

            return new BigInteger[] { Uh, Vl };
        }

        /// <summary>
        /// 计算这个ECFieldElement对象的HashCode
        /// </summary>
        /// <returns>返回对象中BigInteger的HashCode</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public ECFieldElement Sqrt()
        {
            if (curve.Q.TestBit(1))
            {
                ECFieldElement z = new ECFieldElement(BigInteger.ModPow(Value, (curve.Q >> 2) + 1, curve.Q), curve);
                return z.Square().Equals(this) ? z : null;
            }
            BigInteger qMinusOne = curve.Q - 1;
            BigInteger legendreExponent = qMinusOne >> 1;
            if (BigInteger.ModPow(Value, legendreExponent, curve.Q) != 1)
                return null;
            BigInteger u = qMinusOne >> 2;
            BigInteger k = (u << 1) + 1;
            BigInteger Q = this.Value;
            BigInteger fourQ = (Q << 2).Mod(curve.Q);
            BigInteger U, V;
            do
            {
                Random rand = new Random();
                BigInteger P;
                do
                {
                    P = rand.NextBigInteger(curve.Q.GetBitLength());
                }
                while (P >= curve.Q || BigInteger.ModPow(P * P - fourQ, legendreExponent, curve.Q) != qMinusOne);
                BigInteger[] result = FastLucasSequence(curve.Q, P, Q, k);
                U = result[0];
                V = result[1];
                if ((V * V).Mod(curve.Q) == fourQ)
                {
                    if (V.TestBit(0))
                    {
                        V += curve.Q;
                    }
                    V >>= 1;
                    Debug.Assert((V * V).Mod(curve.Q) == Value);
                    return new ECFieldElement(V, curve);
                }
            }
            while (U.Equals(BigInteger.One) || U.Equals(qMinusOne));
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ECFieldElement Square()
        {
            return new ECFieldElement((Value * Value).Mod(curve.Q), curve);
        }

        /// <summary>
        /// 将这个ECFieldElement的value转换成一个256位的字节数组返回
        /// </summary>
        /// <returns>转换后的字节数组</returns>
        public byte[] ToByteArray()
        {
            byte[] data = Value.ToByteArray();
            if (data.Length == 32)
                return data.Reverse().ToArray();
            if (data.Length > 32)
                return data.Take(32).Reverse().ToArray();
            return Enumerable.Repeat<byte>(0, 32 - data.Length).Concat(data.Reverse()).ToArray();
        }
        /// <summary>
        /// <c>-</c>操作符,求这个ECFieldElement对象的负数
        /// </summary>
        /// <param name="x">被取负数的ECFieldElement对象</param>
        /// <returns>返回value值为原来负数后取模的一个ECFieldElement对象</returns>
        public static ECFieldElement operator -(ECFieldElement x)
        {
            return new ECFieldElement((-x.Value).Mod(x.curve.Q), x.curve);
        }

        /// <summary>
        /// <c>*</c>操作符,求两个ECFieldElement对象相乘的结果
        /// </summary>
        /// <param name="x">作为被乘数的ECFieldElement对象</param>
        /// <param name="y">作为乘数的ECFieldElement对象</param>
        /// <returns>一个value是两个对象乘积之后取模的ECFieldElement对象</returns>
        public static ECFieldElement operator *(ECFieldElement x, ECFieldElement y)
        {
            return new ECFieldElement((x.Value * y.Value).Mod(x.curve.Q), x.curve);
        }

        /// <summary>
        /// <c>/</c>操作符,通过乘法逆元把两个ECFieldElement对象做除法运算.
        /// </summary>
        /// <param name="x">作为除数的ECFieldElement对象</param>
        /// <param name="y">作为被除数的ECFieldElement对象</param>
        /// <returns>一个value是除完之后的值的ECFieldElement对象</returns>
        public static ECFieldElement operator /(ECFieldElement x, ECFieldElement y)
        {
            return new ECFieldElement((x.Value * y.Value.ModInverse(x.curve.Q)).Mod(x.curve.Q), x.curve);
        }

        /// <summary>
        ///  <c>+</c>操作符, 把两个ECFieldElement对象做加法运算.
        /// </summary>
        /// <param name="x">作为加数的ECFieldElement对象</param>
        /// <param name="y">作为被加数的ECFieldElement对象</param>
        /// <returns>一个value是两者之和取模的ECFieldElement对象</returns>
        public static ECFieldElement operator +(ECFieldElement x, ECFieldElement y)
        {
            return new ECFieldElement((x.Value + y.Value).Mod(x.curve.Q), x.curve);
        }

        /// <summary>
        /// <c>-</c>操作符,把两个ECFieldElement对象做减法运算.
        /// </summary>
        /// <param name="x">作为被减数的ECFieldElement对象</param>
        /// <param name="y">作为减数的ECFieldElement对象</param>
        /// <returns>一个value是两者之差取模的ECFieldElement对象</returns>
        public static ECFieldElement operator -(ECFieldElement x, ECFieldElement y)
        {
            return new ECFieldElement((x.Value - y.Value).Mod(x.curve.Q), x.curve);
        }
    }
}
