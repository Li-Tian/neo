using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Neo.Cryptography.ECC
{
    /// <summary>
    /// 使用椭圆曲线和产生的公钥拿来做签名的算法类
    /// </summary>
    /// <remarks>原版代码是public，但未被使用，所以不予公开</remarks>
    internal class ECDsa
    {
        private readonly byte[] privateKey;
        private readonly ECPoint publicKey;
        private readonly ECCurve curve;

        /// <summary>
        /// 作为构造函数传入私钥和椭圆曲线
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="curve">椭圆曲线</param>
        public ECDsa(byte[] privateKey, ECCurve curve)
            : this(curve.G * privateKey)
        {
            this.privateKey = privateKey;
        }

        /// <summary>
        /// 作为构造函数传入一个包含了publicKey的ECPoint作为参数，存为这个ECDsa的publicKey， 并且将椭圆曲线类型存为curve
        /// </summary>
        /// <param name="publicKey">包含了publicKey的ECPoint</param>
        public ECDsa(ECPoint publicKey)
        {
            this.publicKey = publicKey;
            this.curve = publicKey.Curve;
        }

        private BigInteger CalculateE(BigInteger n, byte[] message)
        {
            int messageBitLength = message.Length * 8;
            BigInteger trunc = new BigInteger(message.Reverse().Concat(new byte[1]).ToArray());
            if (n.GetBitLength() < messageBitLength)
            {
                trunc >>= messageBitLength - n.GetBitLength();
            }
            return trunc;
        }

        /// <summary>
        /// 产生签名
        /// 签名过程：
        /// 1、选择随机数r，计算点r·G(x, y)。
        /// 2、根据随机数r、消息M的哈希h、私钥k，计算s = (h + k·x)/r。
        /// 3、将消息M、和签名{r·G, s}发给接收方。
        /// </summary>
        /// <param name="message">被签名的信息</param>
        /// <returns>作为签名的大数对r,s</returns>
        /// <exception cref="System.InvalidOperationException">私钥为空的时候抛出</exception>
        public BigInteger[] GenerateSignature(byte[] message)
        {
            if (privateKey == null) throw new InvalidOperationException();
            BigInteger e = CalculateE(curve.N, message);
            BigInteger d = new BigInteger(privateKey.Reverse().Concat(new byte[1]).ToArray());
            BigInteger r, s;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                do
                {
                    BigInteger k;
                    do
                    {
                        do
                        {
                            k = rng.NextBigInteger(curve.N.GetBitLength());
                        }
                        while (k.Sign == 0 || k.CompareTo(curve.N) >= 0);
                        ECPoint p = ECPoint.Multiply(curve.G, k);
                        BigInteger x = p.X.Value;
                        r = x.Mod(curve.N);
                    }
                    while (r.Sign == 0);
                    s = (k.ModInverse(curve.N) * (e + d * r)).Mod(curve.N);
                    if (s > curve.N / 2)
                    {
                        s = curve.N - s;
                    }
                }
                while (s.Sign == 0);
            }
            return new BigInteger[] { r, s };
        }

        private static ECPoint SumOfTwoMultiplies(ECPoint P, BigInteger k, ECPoint Q, BigInteger l)
        {
            int m = Math.Max(k.GetBitLength(), l.GetBitLength());
            ECPoint Z = P + Q;
            ECPoint R = P.Curve.Infinity;
            for (int i = m - 1; i >= 0; --i)
            {
                R = R.Twice();
                if (k.TestBit(i))
                {
                    if (l.TestBit(i))
                        R = R + Z;
                    else
                        R = R + P;
                }
                else
                {
                    if (l.TestBit(i))
                        R = R + Q;
                }
            }
            return R;
        }


        /// <summary>
        /// 验证签名
        /// 验证过程：
        /// 1、接收方收到消息M、以及签名{r·G=(x, y), s}。
        /// 2、根据消息求哈希h。
        /// 3、使用发送方公钥K计算：h·G/s + x·K/s，并与r·G比较，如相等即验签成功。
        /// 推导原理如下：
        /// h⋅G/s+x⋅K/s
        /// =h⋅G/s+x(k⋅G)/s
        /// =(h+x⋅k)G/s
        /// =r(h+x⋅k)G/(h+k⋅x)
        /// =r⋅G
        /// </summary>
        /// <param name="message">等待被验证的消息</param>
        /// <param name="r">作为签名的大数r</param>
        /// <param name="s">作为签名的大数对s</param>
        /// <returns>如果验证通过返回<c>true</c>, 如果不通过返回<c>false</c></returns>
        public bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
        {
            if (r.Sign < 1 || s.Sign < 1 || r.CompareTo(curve.N) >= 0 || s.CompareTo(curve.N) >= 0)
                return false;
            BigInteger e = CalculateE(curve.N, message);
            BigInteger c = s.ModInverse(curve.N);
            BigInteger u1 = (e * c).Mod(curve.N);
            BigInteger u2 = (r * c).Mod(curve.N);
            ECPoint point = SumOfTwoMultiplies(curve.G, u1, publicKey, u2);
            BigInteger v = point.X.Value.Mod(curve.N);
            return v.Equals(r);
        }
    }
}
