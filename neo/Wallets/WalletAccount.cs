using Neo.SmartContract;

namespace Neo.Wallets
{
    /// <summary>
    /// Neo钱包的抽象类
    /// </summary>
    public abstract class WalletAccount
    {
        public readonly UInt160 ScriptHash;

        /// <summary>
        /// 钱包的标签
        /// </summary>
        public string Label;

        /// <summary>
        /// 是否是默认钱包
        /// </summary>
        public bool IsDefault;
        public bool Lock;

        /// <summary>
        /// 钱包的合约
        /// </summary>
        public Contract Contract;


        /// <summary>
        /// 钱包的地址
        /// </summary>
        public string Address => ScriptHash.ToAddress();

        /// <summary>
        /// 是否已经拥有了Key
        /// </summary>
        public abstract bool HasKey { get; }

        /// <summary>
        /// 是否是一个监视钱包
        /// </summary>
        public bool WatchOnly => Contract == null;

        /// <summary>
        /// 返回一个KeyPair对象
        /// </summary>
        /// <returns>钱包中的一个KeyPair</returns>
        public abstract KeyPair GetKey();

        protected WalletAccount(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
