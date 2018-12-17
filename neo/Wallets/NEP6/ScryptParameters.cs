using Neo.IO.Json;

namespace Neo.Wallets.NEP6
{
    /// <summary>
    /// NEP6钱包使用Scrypt算法加密解密NEP2密文所需的参数对象类
    /// </summary>
    public class ScryptParameters
    {
        /// <summary>
        /// 默认值，默认N=16384，R=8，P=8
        /// </summary>
        public static ScryptParameters Default { get; } = new ScryptParameters(16384, 8, 8);
        /// <summary>
        /// ScryptParameters内部的三个参数<br/>
        /// ​ N（CPU/内存消耗指数，一般取值为2的若干次方）：16384<br/>
        ///  R（表块大小，理论取值范围为1-255，同样越大越依赖内存与带宽 ）： 8<br/>
        ///  P（并行计算参数，理论上取值范围为1-255，参数值越大越依赖于并发计算）：8<br/>
        /// </summary>
        public readonly int N, R, P;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="n">CPU/内存消耗指数，一般取值为2的若干次方</param>
        /// <param name="r">表块大小，理论取值范围为1-255，同样越大越依赖内存与带宽 </param>
        /// <param name="p">并行计算参数，理论上取值范围为1-255，参数值越大越依赖于并发计算</param>
        public ScryptParameters(int n, int r, int p)
        {
            this.N = n;
            this.R = r;
            this.P = p;
        }
        /// <summary>
        /// 通过Json对象构建ScryptParameters对象
        /// </summary>
        /// <param name="json">Json对象</param>
        /// <returns>转换成的ScryptParameters对象</returns>
        public static ScryptParameters FromJson(JObject json)
        {
            return new ScryptParameters((int)json["n"].AsNumber(), (int)json["r"].AsNumber(), (int)json["p"].AsNumber());
        }
        /// <summary>
        /// ScryptParameters对象转Json对象的序列化方法
        /// </summary>
        /// <returns>转换成的JObject对象</returns>
        public JObject ToJson()
        {
            JObject json = new JObject();
            json["n"] = N;
            json["r"] = R;
            json["p"] = P;
            return json;
        }
    }
}
