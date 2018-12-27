using Neo.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Neo.Cryptography.ECC
{
    // <summary>
    // 这个类代表了椭圆曲线（EC Curve）上的一个点
    // </summary>
    /// <summary>
    /// This class represents a Point on the Elliptic curve
    /// </summary>
    public class ECPoint : IComparable<ECPoint>, IEquatable<ECPoint>, ISerializable
    {
        internal ECFieldElement X, Y;
        internal readonly ECCurve Curve;

        // <summary>
        // 判定一个点是否为无穷远点（零元）
        // </summary>
        // <value>
        // 如果是无穷远点返回<c>true</c>,不是的话返回<c>false</c>
        // </value>
        //
        /// <summary>
        /// Judge this point is an Infinite point or not 
        /// </summary>
        /// <value>
        /// If this point is an Infinite Point return <c>true</c>,otherwise return<c>false</c>
        /// </value>
        public bool IsInfinity
        {
            get { return X == null && Y == null; }
        }

        // <summary>
        // 获取一个点的字节大小
        // </summary>
        // <value>
        // 判定一个点是否为无穷远点， 如果是返回1， 不是返回33
        // </value>
        /// <summary>
        /// Get the size of this ECpoint
        /// </summary>
        /// <value>
        /// If this Point is a Infinity point return 1, otherwise return 33.
        /// </value>
        public int Size => IsInfinity ? 1 : 33;

        // <summary>
        // 空构造函数, 作用是构造一个零元，即无穷远点, 其x坐标和y坐标分别为null, 曲线为<c>Secp256r1</c>
        // </summary>
        /// <summary>
        /// Default Constructor, which build a Inifite Point. The X axis value and Y axis value are null, the curve is Secp256r1
        /// </summary>
        public ECPoint()
            : this(null, null, ECCurve.Secp256r1)
        {
        }

        // <summary>
        // 构造函数， 返回一个指定椭圆曲线上的，坐标为x,y的点
        // </summary>
        // <param name="x">代表x坐标的ECField对象</param>
        // <param name="y">代表y坐标的ECField对象</param>
        // <param name="curve">该点所用的椭圆曲线</param>
        // <exception cref="ArgumentException">x坐标或者y坐标不能单独为Null</exception>
        /// <summary>
        /// The constrcut function, which return a ECpoint based on the specified curve and with axis value(X,Y)
        /// </summary>
        /// <param name="x">An ECField object which represents the X axis value</param>
        /// <param name="y">An ECField object which represents the Y axis v</param>
        /// <param name="curve">The Elliptic Curve which this point use</param>
        /// <exception cref="ArgumentException">The X axis value or Y axis value can not be null alone</exception>
        internal ECPoint(ECFieldElement x, ECFieldElement y, ECCurve curve)
        {
            if ((x != null && y == null) || (x == null && y != null))
                throw new ArgumentException("Exactly one of the field elements is null");
            this.X = x;
            this.Y = y;
            this.Curve = curve;
        }

        // <summary>
        // 将这个点和另一个ECPoint点比较
        // </summary>
        // <param name="other">另一个ECPoint点</param>
        // <returns>
        // 如果两个点是同一个对象则返回0.
        // 否则,先比较X坐标值的大小,如果不相等则返回X坐标值的比较结果
        // 如果X坐标值相等， 则比较Y轴坐标值.
        // 如果X坐标值相等， 则比较Y轴坐标值.
        // </returns>
        /// <summary>
        /// Compare this ECPoint with the other ECpoint
        /// </summary>
        /// <param name="other">The other ECpoint point</param>
        /// <returns>
        /// return 0 if these two point is a same object.
        /// Otherwise, Compare the X axis value first. If the this Point's X axis value greater than the other Point's X axis value, return 1.<br/>
        /// If the this Point's X axis value smaller than the other Point's X axis value, return -1. Otherwise, compare the Y axis value.
        /// If the this Point's Y axis value greater than the other Point's Y axis value, return 1.
        /// If the this Point's Y axis value smaller than the other Point's Y axis value, return -1. If both X value and Y value are equal, return 0.
        /// </returns>
        public int CompareTo(ECPoint other)
        {
            if (ReferenceEquals(this, other)) return 0;
            int result = X.CompareTo(other.X);
            if (result != 0) return result;
            return Y.CompareTo(other.Y);
        }

        // <summary>
        // 将一个代表一个ECPoint的字节数组解码为一个ECPoint对象.
        // 如果第一个字节为0x00, 则为零元. 
        // 如果第一个字节为0x02或者0x03, 则解码为压缩过的ECPoint.
        // 如果第一个字节为0x04,0x06,0x07, 则解码为非压缩过的ECPoint
        // 如果都不符合, 则抛出异常.
        // </summary>
        // <exception cref="FormatException">编码格式有问题</exception>
        // <param name="encoded">编码后的字节数组</param>
        // <param name="curve">使用的椭圆曲线</param>
        // <returns>解码后的ECPoint对象</returns>
        /// <summary>
        /// Decode a byte array to a ECPoint object.
        /// If the first byte is 0x00, it is a infinity point.
        /// If the first byte is 0x02 or 0x03, then it is decoded as a compressed ECPoint.
        /// If the first byte is 0x04, 0x06, 0x07, then it is decoded as an uncompressed ECPoint.
        /// If not qualified with both above, then it will throw an exception.
        /// </summary>
        /// <exception cref="FormatException">There is format problem for this byte array</exception>
        /// <param name="encoded">The encoded byte array which stands for ECPoint</param>
        /// <param name="curve">The Elliptic curve which this ECpoint use</param>
        /// <returns>The Decoded ECpoint</returns>
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

        // <summary>
        // 从一个字节流中读出并转换成为一个ECPoint对象, 并且将其X,Y坐标分别赋予当前ECPoint对象的X，Y 
        // </summary>
        // <param name="reader">一个BinaryReader， 从字节流中读入被转换的ECPoint</param>
        /// <summary>
        /// Read data from byte stream from a BinaryReader and transfer it to a ECPoint object, and assign the X, Y value to the current ECPoint object.
        /// </summary>
        /// <param name="reader">A binaryReader which reads the ECPoint from the BinaryReader</param>
        void ISerializable.Deserialize(BinaryReader reader)
        {
            ECPoint p = DeserializeFrom(reader, Curve);
            X = p.X;
            Y = p.Y;
        }

        // <summary>
        // 从一个字节流中读出并转换成为一个ECPoint
        // </summary>
        // <param name="reader">一个BinaryReader， 从字节流中读入被转换的ECPoint</param>
        // <param name="curve">这个ECPoint所在的ECCurve</param>
        // <exception cref="FormatException">如果读出来的字节流第一个字节不符合格式</exception>
        // <returns>反序列化之后的ECPoint</returns>
        /// <summary>
        /// Read data from a bytestream and transfer it to an  ECPoint
        /// </summary>
        /// <param name="reader">The BinaryReader which reads the ECPoint data</param>
        /// <param name="curve">The curve which this point to use</param>
        /// <exception cref="FormatException">If the byte read from BinaryReader is not the correct format</exception>
        /// <returns>The deserialized ECPoint</returns>
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

        // <summary>
        // 将这对象内ECPoint对象编码为一个字节数组
        // </summary>
        // <param name="commpressed">标记返回压缩之后的ECPoint</param>
        // <returns>
        // 如果需要压缩， 则返回经过压缩的算法得来的ECPoint对象的字节数组
        // 如果不压缩，则直接返回ECPoint对象的字节数组
        // </returns>
        /// <summary>
        /// Encode this ECPoint to an byte array
        /// </summary>
        /// <param name="commpressed">Flag to mark if encode this ECPoint to a compressed format or not</param>
        /// <returns>If compressed , return the byte array which is useing compress algorithm to encode
        /// Otherwise, return the byte array encoded from ECPoint
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

        // <summary>
        // 比较方法，与另一个ECPoint进行比较。
        // </summary>
        // <param name="other">另一个拿来比较的ECPoint</param>
        // <returns>
        // 如果是同一个对象，返回<c>true</c>. <BR/>
        // 如果参数other 是null , 返回<c>false</c>.<BR/>
        // 如果是两个都是零元, 返回<c>true</c>.<BR/>
        // 如果一个是零元，则返回<c>false</c>.<BR/>
        // 否则对两个坐标值 X,Y进行比较
        // </returns>
        /// <summary>
        /// Compare method, compare this ECPoint with other ECPoint
        /// </summary>
        /// <param name="other">The other ECPoint object to be compared</param>
        /// <returns>
        /// If these two ECPoint are same reference, return<c>true</c>. <BR/>
        /// If the other point is null, return <c>false</c>.<BR/>
        /// If these two points are both Infinity point, return <c>true</c>.<BR/>
        /// 如果一个是零元，则返回<c>false</c>.<BR/>
        /// Otherwise, compare the X axis value and Y axis value.
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
        /// Compare if two objects are equal
        /// </summary>
        /// <param name="obj">The object which is going to be compared</param>
        /// <returns>If the two objects are equal then return <c>true</c>, otherwise it returns <c>false</c></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ECPoint);
        }

        /// <summary>
        /// 将一个由椭圆曲线生成的公钥解码为一个ECPoint对象
        /// </summary>
        /// <param name="pubkey">
        /// 需要被解码的公钥<br/>
        /// 1. 当pubkey的字节长度是33字节时，表示一个完整的压缩型公钥。<br/>
        /// 2. 当pubkey的字节长度是65字节时，表示一个完整的非压缩型公钥。<br/>
        /// 3. 当pubkey的字节长度是64字节时，将自动添加前缀标识字节(0x04)表示一个完整的非压缩型公钥。<br/>
        /// 4. 当pubkey的字节长度是72字节时，表示扩展格式1(保留)，将省略开头的8个字节，然后自动添加前缀标识字节(0x04)表示一个完整的非压缩型公钥。<br/>
        /// 5. 当pubkey的字节长度是96字节时，表示扩展格式2(保留)，将省略结尾的32个字节，然后自动添加前缀标识字节(0x04)表示一个完整的非压缩型公钥。<br/>
        /// 6. 当pubkey的字节长度是104字节时，表示扩展格式3(保留)，将省略开头的8个字节和结尾的32个字节，然后自动拼接标识字节(0x04)表示一个完整的非压缩型公钥。<br/>
        /// </param>
        /// <param name="curve">椭圆曲线</param>
        /// <returns>
        /// 根据不同类型的公钥解码后的ECPoint对象
        /// </returns>
        /// <exception cref="System.FormatException">(*)</exception>
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

        // <summary>
        // 计算这个对象的HashCode并且返回,这个对象的HashCode为X坐标值的HashCode和Y坐标值的HashCode之和
        // </summary>
        // <returns>返回当前Ecpoint的HashCode</returns>
        /// <summary>
        /// Calculate the HashCode of this ECPoint and return. The HashCode is the sum of hashCode of X axis and hashCode of Y axis
        /// </summary>
        /// <returns>The HashCode of current ECPoint</returns>
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

        // <summary>
        // 解析一个字符串，转换成ECPoint
        // </summary>
        // <param name="value">被解析的字符串（16进制表示）</param>
        // <param name="curve">椭圆曲线类型</param>
        // <returns>被解析的ECpoint</returns>
        /// <summary>
        /// Parse a String and transfer to ECPoint
        /// </summary>
        /// <param name="value">The String(in hex format) to be parsed</param>
        /// <param name="curve">The ECCurve type</param>
        /// <returns>The parsed ECpoint</returns>
        public static ECPoint Parse(string value, ECCurve curve)
        {
            return DecodePoint(value.HexToBytes(), curve);
        }

        // <summary>
        // 序列化这个ECPoint并且写入字节流中（使用压缩格式）
        // </summary>
        // <param name="writer">写入字节流的BianryWriter</param>
        /// <summary>
        /// Serialize this ECPoint and write into the byteStream with compressed format
        /// </summary>
        /// <param name="writer">The BinaryWriter which used to write in the byte stream</param>
        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(EncodePoint(true));
        }

        // <summary>
        // 将这个ECPoint编码后转换为16进制的字符串（使用压缩格式）
        // </summary>
        // <returns>转换之后的字符串</returns>
        /// <summary>
        /// Transfer this ECPoint to a hex string with compressed format
        /// </summary>
        /// <returns>The transfered string</returns>
        public override string ToString()
        {
            return EncodePoint(true).ToHexString();
        }

        // <summary>
        // 解析一个字符串, 将其解析为一个ECPoint
        // </summary>
        // <param name="value">被解析的字符串</param>
        // <param name="curve">椭圆曲线类型</param>
        // <param name="point">将解析过后的ECPoint保存到point</param>
        // <returns>如果能够解析返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Parse a string and transfer it to a ECPoint
        /// </summary>
        /// <param name="value">The string to be parsed</param>
        /// <param name="curve">The type of ECCurve</param>
        /// <param name="point">The object which is used to save the ECPoint</param>
        /// <returns>If this string can be parsed then return <c>true</c>, otherwise return <c>false</c></returns>
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

        // <summary>
        // <c>-</c>操作符,求出这个点的负元, 将其y坐标对称转换
        // </summary>
        // <param name="x">被转换的点</param>
        // <returns>返回一个负元,即这个曲线上y坐标为原来点的负数的点</returns>
        /// <summary>
        /// The negative operator, which get the negative point of this ECPoint, and it's Y axis value is the negative value of origial one
        /// </summary>
        /// <param name="x">The point which is going to be transferd</param>
        /// <returns>return a negative minus point, whose Y axis value is the negative value of original one</returns>
        public static ECPoint operator -(ECPoint x)
        {
            return new ECPoint(x.X, -x.Y, x.Curve);
        }

        // <summary>
        // <c>*</c>操作符, 用来计算曲线上的乘法运算
        // </summary>
        // <param name="p">作为被乘数的坐标点</param>
        // <param name="n">作为乘数的坐标点所存储的字节数组。长度必须为32字节</param>
        // <exception cref="ArgumentNullException">如果两个参数中有一个是null</exception>
        // <exception cref="ArgumentException">如果传入的字节长度不是32</exception>
        // <returns>
        // 求乘法计算后的结果.如果该点为无穷远点, 则返回这个零点
        // 如果乘数是0,返回无穷零点
        // 否则, 调用Multiply方法计算两者乘积。
        // </returns>
        /// <summary>
        /// <c>*</c>operator, which make the multipy operation on this curve 
        /// </summary>
        /// <param name="p">The ECPoint which is used as multiplicand</param>
        /// <param name="n">The ECPoint which is used as multiplier. It is a byte array and the length must be 32 bytes</param>
        /// <exception cref="ArgumentNullException">If any one Point object is null</exception>
        /// <exception cref="ArgumentException">If the length argument which is used as multiplier is not 32</exception> 
        /// <returns>Get the result of the multiplication. If this Point is infinity point, then return this point.
        /// If the multiplier is 0, then return the Infinity Point.
        /// Otherwith, use Multiply() method to calculate these two ECPoint
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

        // <summary>
        //  <c>+</c>操作符, 计算曲线上的加法运算
        // </summary>
        // <param name="x">第一个点</param>
        // <param name="y">第二个点</param>
        // <returns>
        // 如果有一个点为无穷远点（零元），则返回另一个点.<br/>
        // 如果一个点为另一个点的对于x轴对称点，则返回一个无穷点.<br/>
        // 如果两个点完全相同， 则返回一个点的twice之后的结果.<br/>
        // 否则返回在这个曲线上两个点求和后的新点
        // </returns>
        /// <summary>
        /// <c>+</c>+operator, calculates the sum of two points on this curve.
        /// </summary>
        /// <param name="x">The first point</param>
        /// <param name="y">The second point</param>
        /// <returns>
        /// If one point is Infinity point, then return the other point directly.
        /// If first point and sceond point are symmetric for X axis, return an Infinity point
        /// If two points are same, then return the twice() value of one point.
        /// otherwise, return the new ECPoint which is the sum of these two points.
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

        // <summary>
        // <c>-</c>操作符,计算两个椭圆曲线上点的差值
        // </summary>
        // <param name="x">第一个点</param>
        // <param name="y">第二个点</param>
        // <returns>两个点的差.如果第二个点是无穷远点(零元)则返回第一个点x, 否则返回第一个点和第二个点取负数的和</returns>
        /// <summary>
        /// - operator, calculate the difference on the curve of two ECPoints
        /// </summary>
        /// <param name="x">The first ECPoint</param>
        /// <param name="y">The second ECPoint</param>
        /// <returns>return the difference of two ECPoint. If the second point is Infinity Point return the first Point. Otherwise return the sum of the first point and the negative one of second point</returns>
        public static ECPoint operator -(ECPoint x, ECPoint y)
        {
            if (y.IsInfinity)
                return x;
            return x + (-y);
        }
    }
}
