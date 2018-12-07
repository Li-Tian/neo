using Neo.SmartContract;

namespace Neo.Wallets
{
    /// <summary>
    /// Neo钱包账户的抽象类
    /// </summary>
    public abstract class WalletAccount
    {
        /// <summary>
        /// 脚本的哈希值。可以转化为地址。
        /// </summary>
        public readonly UInt160 ScriptHash;

        /// <summary>
        /// 钱包的标签
        /// </summary>
        public string Label;

        /// <summary>
        /// 是否是默认钱包
        /// </summary>
        public bool IsDefault;
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool Lock;

        /// <summary>
        /// 钱包账户的合约
        /// </summary>
        public Contract Contract;


        /// <summary>
        /// 钱包账户的地址
        /// </summary>
        public string Address => ScriptHash.ToAddress();

        /// <summary>
        /// 是否已经拥有了Key
        /// </summary>
        public abstract bool HasKey { get; }

        /// <summary>
        /// 是否是一个监视钱包账户
        /// </summary>
        public bool WatchOnly => Contract == null;

        /// <summary>
        /// 返回一个KeyPair对象
        /// </summary>
        /// <returns>钱包中的一个KeyPair</returns>
        public abstract KeyPair GetKey();
        /// <summary>
        /// 通过一个地址脚本哈希来构建一个钱包账户。
        /// </summary>
        /// <param name="scriptHash">地址哈希</param>
        protected WalletAccount(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
