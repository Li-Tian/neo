using Neo.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Neo.Cryptography.ECC
{
    /// <summary>
    /// 这个类代表了椭圆曲线（EC Curve）上的一个点
    /// </summary>
    public class ECPoint : IComparable<ECPoint>, IEquatable<ECPoint>, ISerializable
    {
        internal ECFieldElement X, Y;
        internal readonly ECCurve Curve;

        /// <summary>
        /// 判定一个点是否为无穷远点（零元）
        /// </summary>
        /// <value>
        /// 如果是无穷远点返回<c>true</c>,不是的话返回<c>false</c>
        /// </value>
        public bool IsInfinity
        {
            get { return X == null && Y == null; }
        }

        /// <summary>
        /// 判定一个点的大小
        /// </summary>
        /// <value>
        /// 判定一个点是否为无穷远点， 如果是的化返回1， 不是的话返回33
        /// </value>
        public int Size => IsInfinity ? 1 : 33;

        /// <summary>
        /// 空构造函数, 作用是构造一个零元，即无穷远点, 其x坐标和y坐标分别为null, 曲线为<c>Secp256r1</c>
        /// </summary>
        public ECPoint()
            : this(null, null, ECCurve.Secp256r1)
        {
        }

        /// <summary>
        /// 构造函数， 返回一个指定椭圆曲线上的，坐标为x,y的点
        /// </summary>
        /// <param name="x">代表x坐标的ECField对象</param>
        /// <param name="y">代表y坐标的ECField对象</param>
        /// <param name="curve">改点所用的椭圆曲线</param>
        /// <exception cref="ArgumentException">x坐标或者y坐标不能单独为Null</exception>
        internal ECPoint(ECFieldElement x, ECFieldElement y, ECCurve curve)
        {
            if ((x != null && y == null) || (x == null && y != null))
                throw new ArgumentException("Exactly one of the field elements is null");
            this.X = x;
            this.Y = y;
            this.Curve = curve;
        }

        /// <summary>
        /// 将这个点和另一个ECPoint点比较
        /// </summary>
        /// <param name="other">另一个ECPoint点</param>
        /// <returns>
        /// 如果两个点是一个引用返回0.
        /// 否则,先比较X坐标值的大小,如果不相等则返回1或者-1
        /// 如果X坐标值相等， 则比较Y轴坐标值.
        /// </returns>
        public int CompareTo(ECPoint other)
        {
            if (ReferenceEquals(this, other)) return 0;
            int result = X.CompareTo(other.X);
            if (result != 0) return result;
            return Y.CompareTo(other.Y);
        }

        /// <summary>
        /// 将一个代表一个ECPoint的字节数组解码为一个ECPoint对象.
        /// 如果第一个字节为0x00, 则为零元. 
        /// 如果第一个字节为0x02或者0x03, 则解码为压缩过的ECPoint.
        /// 如果第一个字节为0x04,0x06,0x07, 则解码为非压缩过的ECPoint
        /// 如果都不符合, 则抛出异常.
        /// </summary>
        /// <exception cref="FormatException">编码格式有问题</exception>
        /// <param name="encoded">编码后的字节数组</param>
        /// <param name="curve">使用的椭圆曲线</param>
        /// <returns>解码后的ECPoint对象</returns>
        public static ECPoint DecodePoint(byte[] encoded, ECCurve curve)
        {
            ECPoint p = null;
            int expectedLength = (curve.Q.GetBitLength() + 7) / 8;
            switch (encoded[0])
            {
                case 0x00: // infinity
                    {
                        if (encoded.Length != 1)
                            throw new FormatException("Incorrect length for infinity encoding");
                        p = curve.Infinity;
                        break;
                    }
                case 0x02: // compressed
                case 0x03: // compressed
                    {
                        if (encoded.Length != (expectedLength + 1))
                            throw new FormatException("Incorrect length for compressed encoding");
                        int yTilde = encoded[0] & 1;
                        BigInteger X1 = new BigInteger(encoded.Skip(1).Reverse().Concat(new byte[1]).ToArray());
                        p = DecompressPoint(yTilde, X1, curve);
                        break;
                    }
                case 0x04: // uncompressed
                case 0x06: // hybrid
                case 0x07: // hybrid
                    {
                        if (encoded.Length != (2 * expectedLength + 1))
                            throw new FormatException("Incorrect length for uncompressed/hybrid encoding");
                        BigInteger X1 = new BigInteger(encoded.Skip(1).Take(expectedLength).Reverse().Concat(new byte[1]).ToArray());
                        BigInteger Y1 = new BigInteger(encoded.Skip(1 + expectedLength).Reverse().Concat(new byte[1]).ToArray());
                        p = new ECPoint(new ECFieldElement(X1, curve), new ECFieldElement(Y1, curve), curve);
                        break;
                    }
                default:
                    throw new FormatException("Invalid point encoding " + encoded[0]);
            }
            return p;
        }

        private static ECPoint DecompressPoint(int yTilde, BigInteger X1, ECCurve curve)
        {
            ECFieldElement x = new ECFieldElement(X1, curve);
            ECFieldElement alpha = x * (x.Square() + curve.A) + curve.B;
            ECFieldElement beta = alpha.Sqrt();

            //
            // if we can't find a sqrt we haven't got a point on the
            // curve - run!
            //
            if (beta == null)
                throw new ArithmeticException("Invalid point compression");

            BigInteger betaValue = beta.Value;
            int bit0 = betaValue.IsEven ? 0 : 1;

            if (bit0 != yTilde)
            {
                // Use the other root
                beta = new ECFieldElement(curve.Q - betaValue, curve);
            }

            return new ECPoint(x, beta, curve);
        }

        /// <summary>
        /// 从一个字节流中读出并转换成为一个ECPoint对象, 并且将其X,Y坐标分别赋予当前ECPoint对象的X，Y 
        /// </summary>
        /// <param name="reader">一个BinaryReader， 从字节流中读入被转换的ECPoint</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            ECPoint p = DeserializeFrom(reader, Curve);
            X = p.X;
            Y = p.Y;
        }

        /// <summary>
        /// 从一个字节流中读出并转换成为一个ECPoint
        /// </summary>
        /// <param name="reader">一个BinaryReader， 从字节流中读入被转换的ECPoint</param>
        /// <param name="curve">这个ECPoint所在的ECCurve</param>
        /// <exception cref="FormatException">如果读出来的字节流第一个字节不符合格式</exception>
        /// <returns>反序列化之后的ECPoint</returns>
        public static ECPoint DeserializeFrom(BinaryReader reader, ECCurve curve)
        {
            int expectedLength = (curve.Q.GetBitLength() + 7) / 8;
            byte[] buffer = new byte[1 + expectedLength * 2];
            buffer[0] = reader.ReadByte();
            switch (buffer[0])
            {
                case 0x00:
                    return curve.Infinity;
                case 0x02:
                case 0x03:
                    reader.Read(buffer, 1, expectedLength);
                    return DecodePoint(buffer.Take(1 + expectedLength).ToArray(), curve);
                case 0x04:
                case 0x06:
                case 0x07:
                    reader.Read(buffer, 1, expectedLength * 2);
                    return DecodePoint(buffer, curve);
                default:
                    throw new FormatException("Invalid point encoding " + buffer[0]);
            }
        }

        /// <summary>
        /// 将这对象内ECPoint对象编码为一个字符串
        /// </summary>
        /// <param name="commpressed">判断是否返回压缩之后的ECPoint</param>
        /// <returns>
        /// 如果需要压缩， 则返回经过压缩的算法得来的ECPoint对象的字节数组
        /// 如果不压缩，则直接返回ECPoint对象的字节数组
        /// </returns>

        public byte[] EncodePoint(bool commpressed)
        {
            if (IsInfinity) return new byte[1];
            byte[] data;
            if (commpressed)
            {
                data = new byte[33];
            }
            else
            {
                data = new byte[65];
                byte[] yBytes = Y.Value.ToByteArray().Reverse().ToArray();
                Buffer.BlockCopy(yBytes, 0, data, 65 - yBytes.Length, yBytes.Length);
            }
            byte[] xBytes = X.Value.ToByteArray().Reverse().ToArray();
            Buffer.BlockCopy(xBytes, 0, data, 33 - xBytes.Length, xBytes.Length);
            data[0] = commpressed ? Y.Value.IsEven ? (byte)0x02 : (byte)0x03 : (byte)0x04;
            return data;
        }

        /// <summary>
        /// 比较方法，与另一个ECPoint进行比较。
        /// </summary>
        /// <param name="other">另一个拿来比较的ECPoint</param>
        /// <returns>
        /// 如果是一个引用，返回<c>true</c>. 
        /// 如果一个是null , 返回<c>false</c>.
        /// 如果是两个都是零元, 返回<c>true</c>
        /// 如果一个是零元，则返回<c>false</c>.
        /// 否则对两个坐标值 X,Y进行比较
        /// </returns>
        public bool Equals(ECPoint other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            if (IsInfinity && other.IsInfinity) return true;
            if (IsInfinity || other.IsInfinity) return false;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <summary>
        /// 比较两个Object是否相等
        /// </summary>
        /// <param name="obj">待比较的object对象</param>
        /// <returns>如果两个相等则返回<c>true</c>, 如果不等则返回<c>false</c></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ECPoint);
        }

        /// <summary>
        /// 将一个由椭圆曲线生成的公钥解码为一个ECPoint对象
        /// </summary>
        /// <param name="pubkey">需要被解码的公钥</param>
        /// <param name="curve">椭圆曲线</param>
        /// <returns>
        /// 根据不同类型的公钥解码后的ECPoint对象
        /// </returns>
        public static ECPoint FromBytes(byte[] pubkey, ECCurve curve)
        {
            switch (pubkey.Length)
            {
                case 33:
                case 65:
                    return DecodePoint(pubkey, curve);
                case 64:
                case 72:
                    return DecodePoint(new byte[] { 0x04 }.Concat(pubkey.Skip(pubkey.Length - 64)).ToArray(), curve);
                case 96:
                case 104:
                    return DecodePoint(new byte[] { 0x04 }.Concat(pubkey.Skip(pubkey.Length - 96).Take(64)).ToArray(), curve);
                default:
                    throw new FormatException();
            }
        }

        /// <summary>
        /// 计算这个对象的HashCode并且返回,这个对象的HashCode为X坐标值的HashCode和Y坐标值的HashCode之和
        /// </summary>
        /// <returns>返回当前Ecpoint的HashCode</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }
     
        internal static ECPoint Multiply(ECPoint p, BigInteger k)
        {
            // floor(log2(k))
            int m = k.GetBitLength();

            // width of the Window NAF
            sbyte width;

            // Required length of precomputation array
            int reqPreCompLen;

            // Determine optimal width and corresponding length of precomputation
            // array based on literature values
            if (m < 13)
            {
                width = 2;
                reqPreCompLen = 1;
            }
            else if (m < 41)
            {
                width = 3;
                reqPreCompLen = 2;
            }
            else if (m < 121)
            {
                width = 4;
                reqPreCompLen = 4;
            }
            else if (m < 337)
            {
                width = 5;
                reqPreCompLen = 8;
            }
            else if (m < 897)
            {
                width = 6;
                reqPreCompLen = 16;
            }
            else if (m < 2305)
            {
                width = 7;
                reqPreCompLen = 32;
            }
            else
            {
                width = 8;
                reqPreCompLen = 127;
            }

            // The length of the precomputation array
            int preCompLen = 1;

            ECPoint[] preComp = preComp = new ECPoint[] { p };
            ECPoint twiceP = p.Twice();

            if (preCompLen < reqPreCompLen)
            {
                // Precomputation array must be made bigger, copy existing preComp
                // array into the larger new preComp array
                ECPoint[] oldPreComp = preComp;
                preComp = new ECPoint[reqPreCompLen];
                Array.Copy(oldPreComp, 0, preComp, 0, preCompLen);

                for (int i = preCompLen; i < reqPreCompLen; i++)
                {
                    // Compute the new ECPoints for the precomputation array.
                    // The values 1, 3, 5, ..., 2^(width-1)-1 times p are
                    // computed
                    preComp[i] = twiceP + preComp[i - 1];
                }
            }

            // Compute the Window NAF of the desired width
            sbyte[] wnaf = WindowNaf(width, k);
            int l = wnaf.Length;

            // Apply the Window NAF to p using the precomputed ECPoint values.
            ECPoint q = p.Curve.Infinity;
            for (int i = l - 1; i >= 0; i--)
            {
                q = q.Twice();

                if (wnaf[i] != 0)
                {
                    if (wnaf[i] > 0)
                    {
                        q += preComp[(wnaf[i] - 1) / 2];
                    }
                    else
                    {
                        // wnaf[i] < 0
                        q -= preComp[(-wnaf[i] - 1) / 2];
                    }
                }
            }

            return q;
        }

        /// <summary>
        /// 解析一个字符串，转换成ECPoint
        /// </summary>
        /// <param name="value">被解析的字符串</param>
        /// <param name="curve">椭圆曲线类型</param>
        /// <returns>被解析的ECpoint</returns>
        public static ECPoint Parse(string value, ECCurve curve)
        {
            return DecodePoint(value.HexToBytes(), curve);
        }

        /// <summary>
        /// 序列化这个ECPoint并且写入字节流中
        /// </summary>
        /// <param name="writer">写入字节流的BianryWriter</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(EncodePoint(true));
        }

        /// <summary>
        /// 将这个ECPoint编码后转换为16进制的字符串
        /// </summary>
        /// <returns>转换之后的字符串</returns>
        public override string ToString()
        {
            return EncodePoint(true).ToHexString();
        }

        /// <summary>
        /// 解析一个字符串, 将其解析为一个ECCpoint
        /// </summary>
        /// <param name="value">被解析的字符串</param>
        /// <param name="curve">椭圆曲线类型</param>
        /// <param name="point">将解析过后的ECCpoint保存到point</param>
        /// <returns>如果能够解析返回<c>true</c>, 否则返回<c>false</c></returns>
        public static bool TryParse(string value, ECCurve curve, out ECPoint point)
        {
            try
            {
                point = Parse(value, curve);
                return true;
            }
            catch (FormatException)
            {
                point = null;
                return false;
            }
        }

        internal ECPoint Twice()
        {
            if (this.IsInfinity)
                return this;
            if (this.Y.Value.Sign == 0)
                return Curve.Infinity;
            ECFieldElement TWO = new ECFieldElement(2, Curve);
            ECFieldElement THREE = new ECFieldElement(3, Curve);
            ECFieldElement gamma = (this.X.Square() * THREE + Curve.A) / (Y * TWO);
            ECFieldElement x3 = gamma.Square() - this.X * TWO;
            ECFieldElement y3 = gamma * (this.X - x3) - this.Y;
            return new ECPoint(x3, y3, Curve);
        }

        private static sbyte[] WindowNaf(sbyte width, BigInteger k)
        {
            sbyte[] wnaf = new sbyte[k.GetBitLength() + 1];
            short pow2wB = (short)(1 << width);
            int i = 0;
            int length = 0;
            while (k.Sign > 0)
            {
                if (!k.IsEven)
                {
                    BigInteger remainder = k % pow2wB;
                    if (remainder.TestBit(width - 1))
                    {
                        wnaf[i] = (sbyte)(remainder - pow2wB);
                    }
                    else
                    {
                        wnaf[i] = (sbyte)remainder;
                    }
                    k -= wnaf[i];
                    length = i;
                }
                else
                {
                    wnaf[i] = 0;
                }
                k >>= 1;
                i++;
            }
            length++;
            sbyte[] wnafShort = new sbyte[length];
            Array.Copy(wnaf, 0, wnafShort, 0, length);
            return wnafShort;
        }

        /// <summary>
        /// <c>-</c>操作符,求出这个点的负元, 将其y坐标对称转换
        /// </summary>
        /// <param name="x">被转换的点</param>
        /// <returns>返回一个负元,即这个曲线上y坐标为原来点的负数的点</returns>
        public static ECPoint operator -(ECPoint x)
        {
            return new ECPoint(x.X, -x.Y, x.Curve);
        }

        /// <summary>
        /// <c>*</c>操作符, 用来计算曲线上的乘法运算
        /// </summary>
        /// <param name="p">作为被乘数的坐标点</param>
        /// <param name="n">作为乘数的坐标点所存储的字节数组</param>
        /// <exception cref="ArgumentNullException">如果两个参数中有一个是null</exception>
        /// <exception cref="ArgumentException">如果传入的字节长度不是32</exception>
        /// <returns>
        /// 求乘法计算后的结果.如果该点为无穷远点, 则返回这个零点
        /// 如果乘数是0,返回无穷零点
        /// 否则, 调用Multiply方法计算两者乘积。
        /// </returns>
        public static ECPoint operator *(ECPoint p, byte[] n)
        {
            if (p == null || n == null)
                throw new ArgumentNullException();
            if (n.Length != 32)
                throw new ArgumentException();
            if (p.IsInfinity)
                return p;
            BigInteger k = new BigInteger(n.Reverse().Concat(new byte[1]).ToArray());
            if (k.Sign == 0)
                return p.Curve.Infinity;
            return Multiply(p, k);
        }

        /// <summary>
        ///  <c>+</c>操作符, 计算曲线上的加法运算
        /// </summary>
        /// <param name="x">第一个点</param>
        /// <param name="y">第二个点</param>
        /// <returns>
        /// 如果有一个点为无穷远点（零元），则返回另一个点.
        /// 如果一个点为另一个点的对于x轴对称点，则返回一个无穷点.
        /// 如果两个点完全相同， 则返回一个点的twice之后的结果.
        /// 否则返回在这个曲线上两个点求和后的新点
        /// </returns>
        public static ECPoint operator +(ECPoint x, ECPoint y)
        {
            if (x.IsInfinity)
                return y;
            if (y.IsInfinity)
                return x;
            if (x.X.Equals(y.X))
            {
                if (x.Y.Equals(y.Y))
                    return x.Twice();
                Debug.Assert(x.Y.Equals(-y.Y));
                return x.Curve.Infinity;
            }
            ECFieldElement gamma = (y.Y - x.Y) / (y.X - x.X);
            ECFieldElement x3 = gamma.Square() - x.X - y.X;
            ECFieldElement y3 = gamma * (x.X - x3) - x.Y;
            return new ECPoint(x3, y3, x.Curve);
        }

        /// <summary>
        /// <c>-</c>操作符,计算两个椭圆曲线上点的差值
        /// </summary>
        /// <param name="x">第一个点</param>
        /// <param name="y">第二个点</param>
        /// <returns>两个点的差.如果第二个点是无穷远点(零元)则返回第一个点x, 否则返回第一个点和第二个点取负数的和</returns>
        public static ECPoint operator -(ECPoint x, ECPoint y)
        {
            if (y.IsInfinity)
                return x;
            return x + (-y);
        }
    }
}
