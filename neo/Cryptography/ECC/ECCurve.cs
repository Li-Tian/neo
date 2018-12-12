using System.Globalization;
using System.Numerics;

namespace Neo.Cryptography.ECC
{
    /// <summary>
    /// 定义椭圆曲线的类, 根据参数的不同, 定义了两条曲线Secp256k1和Secp256r1
    /// </summary>
    public class ECCurve
    {
        internal readonly BigInteger Q;
        internal readonly ECFieldElement A;
        internal readonly ECFieldElement B;
        internal readonly BigInteger N;
        /// <summary>
        /// 这条椭圆曲线的零元
        /// </summary>
        /// <value>
        /// 返回这条椭圆曲线的零元
        /// </value>
        public readonly ECPoint Infinity;

        /// <summary>
        /// 这条椭圆曲线的基点
        /// </summary>
        /// <value>
        /// 返回这条椭圆曲线的基点
        /// </value>
        public readonly ECPoint G;

        private ECCurve(BigInteger Q, BigInteger A, BigInteger B, BigInteger N, byte[] G)
        {
            this.Q = Q;
            this.A = new ECFieldElement(A, this);
            this.B = new ECFieldElement(B, this);
            this.N = N;
            this.Infinity = new ECPoint(null, null, this);
            this.G = ECPoint.DecodePoint(G, this);
        }

        /// <summary>
        /// 返回一个符合ECG标准的Koblitz椭圆曲线. <para>   </para>
        /// 曲线为定义在一个有限域F<sub>p</sub>的方程：y<sup>2</sup> = x<sup>3</sup> +ax + b. 其中a=0, b=7.<para>   </para>
        /// 其中有限域P的定义为： p = FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE FFFFFC2F<para>   </para>
        /// 基点G = 04 79BE667E F9DCBBAC 55A06295 CE870B07 029BFCDB 2DCE28D9 59F2815B 16F81798 483ADA77 26A3C465 5DA4FBFC 0E1108A8 FD17B448 A6855419 9C47D08F FB10D4B8<para>   </para>
        /// 基点G的阶数n = FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE BAAEDCE6 AF48A03B BFD25E8C D0364141
        /// </summary>
        /// <value>
        /// 返回一个Koblitz椭圆曲线
        /// </value>
        public static readonly ECCurve Secp256k1 = new ECCurve
        (
            BigInteger.Parse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F", NumberStyles.AllowHexSpecifier),
            BigInteger.Zero,
            7,
            BigInteger.Parse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141", NumberStyles.AllowHexSpecifier),
            ("04" + "79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798" + "483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8").HexToBytes()
        );

        /// <summary>
        ///  返回一个Random的椭圆曲线.
        /// 曲线为定义在一个有限域F<sub>p</sub>的方程E：y<sup>2</sup> = x<sup>3</sup> +ax + b. 
        /// 其中a=FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFC, b=5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B.<para>   </para>
        /// 其中有限域P的定义为： p = FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE FFFFFC2F<para>   </para>
        /// E 是通过随机数生成的曲线. 随机数的种子为: C49D3608 86E70493 6A6678E1 139D26B7 819F7E90 <para>   </para>
        /// 基点G = 04 6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296 4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5<para>   </para>
        /// 基点G的阶数n = FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551
        /// </summary>
        /// <value>
        /// 返回一个Random类型的椭圆曲线
        /// </value>
        public static readonly ECCurve Secp256r1 = new ECCurve
        (
            BigInteger.Parse("00FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.AllowHexSpecifier),
            BigInteger.Parse("00FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFC", NumberStyles.AllowHexSpecifier),
            BigInteger.Parse("005AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B", NumberStyles.AllowHexSpecifier),
            BigInteger.Parse("00FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551", NumberStyles.AllowHexSpecifier),
            ("04" + "6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296" + "4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5").HexToBytes()
        );
    }
}
