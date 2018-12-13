using Neo.Network.P2P.Payloads;
using System.Collections.Generic;

namespace Neo.Plugins
{

    /// <summary>
    /// 策略插件
    /// </summary>
    public interface IPolicyPlugin
    {

        /// <summary>
        /// 是否过滤内存池交易
        /// </summary>
        /// <param name="tx">内存池交易</param>
        /// <returns>通过过滤则返回true，否则返回false</returns>
        bool FilterForMemoryPool(Transaction tx);

        /// <summary>
        /// 过滤待打包的交易
        /// </summary>
        /// <param name="transactions">待打包的交易</param>
        /// <returns>过滤后的交易</returns>
        IEnumerable<Transaction> FilterForBlock(IEnumerable<Transaction> transactions);
    }
}
