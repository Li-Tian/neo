#pragma warning disable CS0612

using Neo.IO.Caching;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 交易枚举类型
    /// </summary>
    public enum TransactionType : byte
    {
        /// <summary>
        /// 挖矿交易
        /// </summary>
        [ReflectionCache(typeof(MinerTransaction))]
        MinerTransaction = 0x00,

        /// <summary>
        /// 发行资产交易
        /// </summary>
        [ReflectionCache(typeof(IssueTransaction))]
        IssueTransaction = 0x01,

        /// <summary>
        /// Claim GAS交易
        /// </summary>
        [ReflectionCache(typeof(ClaimTransaction))]
        ClaimTransaction = 0x02,

        /// <summary>
        /// 注册验证人交易
        /// </summary>
        [ReflectionCache(typeof(EnrollmentTransaction))]
        EnrollmentTransaction = 0x20,

        /// <summary>
        /// 注册资产交易
        /// </summary>
        [ReflectionCache(typeof(RegisterTransaction))]
        RegisterTransaction = 0x40,

        /// <summary>
        /// 普通交易
        /// </summary>
        [ReflectionCache(typeof(ContractTransaction))]
        ContractTransaction = 0x80,

        /// <summary>
        /// 投票或申请验证人交易
        /// </summary>
        [ReflectionCache(typeof(StateTransaction))]
        StateTransaction = 0x90,

        /// <summary>
        /// Publish scripts to the blockchain for being invoked later.
        /// </summary>
        [ReflectionCache(typeof(PublishTransaction))]
        PublishTransaction = 0xd0,

        /// <summary>
        /// 执行交易，调用智能合约或执行脚本
        /// </summary>
        [ReflectionCache(typeof(InvocationTransaction))]
        InvocationTransaction = 0xd1
    }
}
