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
        /// 交易的合法性过滤
        /// </summary>
        /// <param name="tx">交易</param>
        /// <returns>返回true则将交易加入内存池并转发给其它节点，返回false则抛弃</returns>
        bool FilterForMemoryPool(Transaction tx);

        /// <summary>
        /// 打包的交易的过滤器。共识节点出块时调用
        /// </summary>
        /// <param name="transactions">待打包的交易</param>
        /// <returns>过滤后的交易</returns>
        IEnumerable<Transaction> FilterForBlock(IEnumerable<Transaction> transactions);
    }
}
