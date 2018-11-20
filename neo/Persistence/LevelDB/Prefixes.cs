namespace Neo.Persistence.LevelDB
{
    /// <summary>
    /// Leveldb表前缀
    /// </summary>
    internal static class Prefixes
    {
        /// <summary>
        /// 区块前缀
        /// </summary>
        public const byte DATA_Block = 0x01;


        /// <summary>
        /// 交易前缀
        /// </summary>
        public const byte DATA_Transaction = 0x02;


        /// <summary>
        /// 账户前缀
        /// </summary>
        public const byte ST_Account = 0x40;

        /// <summary>
        /// UTXO前缀
        /// </summary>
        public const byte ST_Coin = 0x44;

        /// <summary>
        /// 已花费交易前缀
        /// </summary>
        public const byte ST_SpentCoin = 0x45;

        /// <summary>
        /// 验证人前缀
        /// </summary>
        public const byte ST_Validator = 0x48;

        /// <summary>
        /// 资产前缀
        /// </summary>
        public const byte ST_Asset = 0x4c;

        /// <summary>
        /// 合约前缀
        /// </summary>
        public const byte ST_Contract = 0x50;

        /// <summary>
        /// 合约存储前缀
        /// </summary>
        public const byte ST_Storage = 0x70;

        /// <summary>
        /// 区块头hash列表索引前缀
        /// </summary>
        public const byte IX_HeaderHashList = 0x80;

        /// <summary>
        /// 验证人个数的投票前缀
        /// </summary>
        public const byte IX_ValidatorsCount = 0x90;

        /// <summary>
        /// 当前区块前缀
        /// </summary>
        public const byte IX_CurrentBlock = 0xc0;

        /// <summary>
        /// 当前区块头前缀
        /// </summary>
        public const byte IX_CurrentHeader = 0xc1;


        /// <summary>
        /// 系统版本号前缀
        /// </summary>
        public const byte SYS_Version = 0xf0;
    }
}
