using Neo.Network.P2P.Payloads;
using System.Collections.Generic;

namespace Neo.Plugins
{

    // <summary>
    // 策略插件
    // </summary>
    /// <summary>
    /// Policy plugin
    /// </summary>
    public interface IPolicyPlugin
    {

        // <summary>
        // 交易的合法性过滤
        // </summary>
        // <param name="tx">交易</param>
        // <returns>返回true则将交易加入内存池并转发给其它节点，返回false则抛弃</returns>
        /// <summary>
        /// Legal filtering of transactions
        /// </summary>
        /// <param name="tx">Transaction</param>
        /// <returns>A return of true adds the transaction to the memory pool and relay it to other nodes, while a return of false discards it</returns>
        bool FilterForMemoryPool(Transaction tx);

        // <summary>
        // 打包的交易的过滤器。共识节点出块时调用
        // </summary>
        // <param name="transactions">待打包的交易</param>
        // <returns>过滤后的交易</returns>
        /// <summary>
        /// Filters for packaged transactions. Called when the consensus node makes a block
        /// </summary>
        /// <param name="transactions">Transactions to be packaged</param>
        /// <returns>Filtered transactions</returns>
        IEnumerable<Transaction> FilterForBlock(IEnumerable<Transaction> transactions);
    }
}
