using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;

namespace Neo.Consensus
{
    /// <summary>
    /// 共识过程上下文，记录当前共识活动信息
    /// </summary>
    public interface IConsensusContext : IDisposable
    {
        //public const uint Version = 0;
        /// <summary>
        /// 所处共识过程状态
        /// </summary>
        ConsensusState State { get; set; }
        /// <summary>
        /// 上一个block的hash
        /// </summary>
        UInt256 PrevHash { get; }
        /// <summary>
        /// 提案block的区块高度
        /// </summary>
        uint BlockIndex { get; }
        /// <summary>
        /// 当前视图的编号
        /// </summary>
        byte ViewNumber { get; }
        /// <summary>
        /// 本轮共识节点的公钥列表
        /// </summary>
        ECPoint[] Validators { get; }
        /// <summary>
        /// 当前节点编号，在Validators数组中序号
        /// </summary>
        int MyIndex { get; }
        /// <summary>
        /// 本轮共识的议长编号
        /// </summary>
        uint PrimaryIndex { get; }
        /// <summary>
        /// 当前提案block时间戳
        /// </summary>
        uint Timestamp { get; set; }
        /// <summary>
        /// 当前提案block的nonce
        /// </summary>
        ulong Nonce { get; set; }
        /// <summary>
        /// 当前提案block的NextConsensus, 指定下一轮共识节点
        /// </summary>
        UInt160 NextConsensus { get; set; }
        /// <summary>
        /// 当前提案block的交易hash列表
        /// </summary>
        UInt256[] TransactionHashes { get; set; }
        /// <summary>
        /// 当前提案block的交易
        /// </summary>
        Dictionary<UInt256, Transaction> Transactions { get; set; }
        /// <summary>
        /// 存放收到的提案block的签名数组
        /// </summary>
        byte[][] Signatures { get; set; }
        /// <summary>
        /// 收到的各节点期望视图编号，主要用在改变视图过程中。这个数组的每一位对应每个验证人节点期待的视图编号。而验证人是有相应编号的。
        /// </summary>
        byte[] ExpectedView { get; set; }
        /// <summary>
        /// 最低共识节点安全阈值个数，低于该阈值，共识过程将会出错
        /// </summary>
        int M { get; }
        /// <summary>
        /// 前区块的区块头
        /// </summary>
        Header PrevHeader { get; }
        // <summary>
        // 判定是否包含指定哈希值的交易
        // </summary>
        // <param name="hash">交易的哈希值</param>
        // <returns>判定是否包含</returns>
        //bool ContainsTransaction(UInt256 hash);
        /// <summary>
        /// 判定指定哈希值的交易是否存在
        /// </summary>
        /// <param name="hash">交易的哈希值</param>
        /// <returns>判定是否存在</returns>
        bool TransactionExists(UInt256 hash);
        /// <summary>
        /// 验证指定的交易是否合法
        /// </summary>
        /// <param name="tx">指定的交易</param>
        /// <returns>合法交易返回true</returns>
        bool VerifyTransaction(Transaction tx);
        /// <summary>
        /// 更换视图
        /// </summary>
        /// <param name="view_number">新的视图编号</param>
        void ChangeView(byte view_number);
        /// <summary>
        /// 创建区块
        /// </summary>
        /// <returns>新创建的区块</returns>
        Block CreateBlock();

        //void Dispose();
        /// <summary>
        /// 计算议长编号
        /// </summary>
        /// <param name="view_number">当前视图编号</param>
        /// <returns>新的议长编号</returns>
        uint GetPrimaryIndex(byte view_number);
        /// <summary>
        /// 构建ChangeView消息
        /// </summary>
        /// <returns>共识消息(ChangeView)</returns>
        ConsensusPayload MakeChangeView();
        /// <summary>
        /// 构建一个只含有区块头，不含有交易内容的空区块
        /// </summary>
        /// <returns>只含有区块头的区块</returns>
        Block MakeHeader();
        /// <summary>
        /// 签名区块头
        /// </summary>
        void SignHeader();
        /// <summary>
        /// 构建PrepareRequset的共识消息ConsensusPayload类
        /// </summary>
        /// <returns>PrepareRequset消息</returns>
        ConsensusPayload MakePrepareRequest();
        /// <summary>
        /// 构建PrepareResponse的共识消息ConsensusPayload类
        /// </summary>
        /// <param name="signature">对提案block的签名</param>
        /// <returns>共识消息</returns>
        ConsensusPayload MakePrepareResponse(byte[] signature);
        /// <summary>
        /// 重置上下文数据
        /// </summary>
        void Reset();
        /// <summary>
        /// 填充提案block的数据
        /// </summary>
        void Fill();
        /// <summary>
        /// 收到PrepareRequest包后，校验PrepareRequest所带的提案block数据
        /// </summary>
        /// <returns>验证合法以后返回true</returns>
        bool VerifyRequest();
    }
}
