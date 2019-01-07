#pragma warning disable CS0612

using Neo.IO.Caching;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // 交易枚举类型
    // </summary>
    /// <summary>
    /// The enum class for the transaction type
    /// </summary>
    public enum TransactionType : byte
    {
        // <summary>
        // 挖矿交易
        // </summary>
        /// <summary>
        /// The miner transaction
        /// </summary>
        [ReflectionCache(typeof(MinerTransaction))]
        MinerTransaction = 0x00,

        // <summary>
        // 发行资产交易
        // </summary>
        /// <summary>
        /// The transaction of issuing asset
        /// </summary>
        [ReflectionCache(typeof(IssueTransaction))]
        IssueTransaction = 0x01,

        // <summary>
        // Claim GAS交易
        // </summary>
        /// <summary>
        /// The gas claim transaction
        /// </summary>
        [ReflectionCache(typeof(ClaimTransaction))]
        ClaimTransaction = 0x02,

        // <summary>
        // 注册验证人交易（已经弃用。参考StateTransaction）
        // </summary>
        /// <summary>
        /// Validator entrollment transaction(Deprecated. Reference  StateTransaction)
        /// </summary>
        [ReflectionCache(typeof(EnrollmentTransaction))]
        EnrollmentTransaction = 0x20,

        // <summary>
        // 注册资产交易
        // </summary>
        /// <summary>
        /// The asset registeration transaction
        /// </summary>
        [ReflectionCache(typeof(RegisterTransaction))]
        RegisterTransaction = 0x40,

        // <summary>
        // 普通交易
        // </summary>
        /// <summary>
        /// The normal transaction
        /// </summary>
        [ReflectionCache(typeof(ContractTransaction))]
        ContractTransaction = 0x80,

        // <summary>
        // 投票或申请验证人交易
        // </summary>
        /// <summary>
        /// The transaction of voting or applying for validators
        /// </summary>
        [ReflectionCache(typeof(StateTransaction))]
        StateTransaction = 0x90,

        // <summary>
        // 部署智能合约到区块链。已经弃用。参考 InvocationTransaction
        // </summary>
        /// <summary>
        /// Deploy smart contract to blockchain, which is already deprecated.Reference InvocationTransaction
        /// </summary>
        [ReflectionCache(typeof(PublishTransaction))]
        PublishTransaction = 0xd0,

        // <summary>
        // 执行交易，调用智能合约或执行脚本。或者部署智能合约。
        // </summary>
        /// <summary>
        /// Transaction execution, invoke the smart contract or the script, or deploy smart contract
        /// </summary>
        [ReflectionCache(typeof(InvocationTransaction))]
        InvocationTransaction = 0xd1
    }
}
