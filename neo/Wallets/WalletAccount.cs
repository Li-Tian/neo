using Neo.SmartContract;

namespace Neo.Wallets
{
    // <summary>
    // Neo钱包账户的抽象类
    // </summary>
    /// <summary>
    /// A abstract class for Neo wallet account
    /// </summary>
    public abstract class WalletAccount
    {
        // <summary>
        // 脚本的哈希值。可以转化为地址。
        // </summary>
        /// <summary>
        /// script hash.It could be converted to address
        /// </summary>
        public readonly UInt160 ScriptHash;

        // <summary>
        // 钱包的标签
        // </summary>
        /// <summary>
        /// wallet label
        /// </summary>
        public string Label;

        // <summary>
        // 是否是默认钱包
        // </summary>
        /// <summary>
        /// A flag to mark if it is the default wallet
        /// </summary>
        public bool IsDefault;
        // <summary>
        // 是否锁定
        // </summary>
        /// <summary>
        /// A flag to mark if it is locked
        /// </summary>
        public bool Lock;

        // <summary>
        // 钱包账户的合约
        // </summary>
        /// <summary>
        /// Contract
        /// </summary>
        public Contract Contract;


        // <summary>
        // 钱包账户的地址
        // </summary>
        /// <summary>
        /// wallet account address
        /// </summary>
        public string Address => ScriptHash.ToAddress();

        // <summary>
        // 是否已经拥有了Key
        // </summary>
        /// <summary>
        /// A flag to mark if it has a key
        /// </summary>
        public abstract bool HasKey { get; }

        // <summary>
        // 是否是一个监视钱包账户
        // </summary>
        /// <summary>
        /// A flag to mark if it is a watch account
        /// </summary>
        public bool WatchOnly => Contract == null;

        // <summary>
        // 返回一个KeyPair对象
        // </summary>
        // <returns>钱包中的一个KeyPair</returns>
        /// <summary>
        /// return a KeyPair object
        /// </summary>
        /// <returns>KeyPair object</returns>
        public abstract KeyPair GetKey();
        // <summary>
        // 通过一个地址脚本哈希来构建一个钱包账户。
        // </summary>
        // <param name="scriptHash">地址哈希</param>
        /// <summary>
        /// Build a wallet account with an script hash。
        /// </summary>
        /// <param name="scriptHash">script hash</param>
        protected WalletAccount(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
