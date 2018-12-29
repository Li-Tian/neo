using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;

namespace Neo.Consensus
{
    // <summary>
    // 共识过程上下文，记录当前共识活动信息
    // </summary>
    /// <summary>
    /// Consensus context.Record current consensus activity information
    /// </summary>
    public interface IConsensusContext : IDisposable
    {
        //public const uint Version = 0;
        // <summary>
        // 所处共识过程状态
        // </summary>
        /// <summary>
        /// Consensus state
        /// </summary>
        ConsensusState State { get; set; }
        // <summary>
        // 上一个block的hash
        // </summary>
        /// <summary>
        /// previous block hash
        /// </summary>
        UInt256 PrevHash { get; }
        // <summary>
        // 提案block的区块高度
        // </summary>
        /// <summary>
        /// proposal block height
        /// </summary>
        uint BlockIndex { get; }
        // <summary>
        // 当前视图的编号
        // </summary>
        /// <summary>
        /// current view number
        /// </summary>
        byte ViewNumber { get; }
        // <summary>
        // 本轮共识节点的公钥列表
        // </summary>
        /// <summary>
        /// List of public keys for this round of consensus nodes
        /// </summary>
        ECPoint[] Validators { get; }
        // <summary>
        // 当前节点编号，在Validators数组中序号
        // </summary>        
        /// <summary>
        /// Current node number, index in the Validators array
        /// </summary>        
        int MyIndex { get; }
        // <summary>
        // 本轮共识的议长编号
        // </summary>
        /// <summary>
        /// Speaker's index of this round of consensus
        /// </summary>
        uint PrimaryIndex { get; }
        // <summary>
        // 当前提案block时间戳
        // </summary>
        /// <summary>
        /// current proposal block timestamp
        /// </summary>
        uint Timestamp { get; set; }
        // <summary>
        // 当前提案block的nonce
        // </summary>
        /// <summary>
        /// current proposal block nonce
        /// </summary>
        ulong Nonce { get; set; }
        // <summary>
        // 当前提案block的NextConsensus, 指定下一轮共识节点
        // </summary>
        /// <summary>
        /// NextConsensus of the current proposal block, specifying the next round of consensus nodes
        /// </summary>
        UInt160 NextConsensus { get; set; }
        // <summary>
        // 当前提案block的交易hash列表
        // </summary>
        /// <summary>
        /// List of transaction hashes for the current proposal block
        /// </summary>
        UInt256[] TransactionHashes { get; set; }
        // <summary>
        // 当前提案block的交易
        // </summary>
        /// <summary>
        /// Current proposal block transaction
        /// </summary>
        Dictionary<UInt256, Transaction> Transactions { get; set; }
        // <summary>
        // 存放收到的提案block的签名数组
        // </summary>
        /// <summary>
        /// A array store the signatures of the proposal block
        /// </summary>
        byte[][] Signatures { get; set; }
        // <summary>
        // 收到的各节点期望视图编号，主要用在改变视图过程中。这个数组的每一位对应每个验证人节点期待的视图编号。而验证人是有相应编号的。
        // </summary>
        /// <summary>
        /// The expected view number of each node is mainly used in the process of changing the view.
        /// Each item of this array corresponds to the view number expected by each verifier node.The verifier node is numbered accordingly.        /// </summary>
        /// </summary>
        byte[] ExpectedView { get; set; }
        // <summary>
        // 最低共识节点安全阈值个数，低于该阈值，共识过程将会出错
        // </summary>
        /// <summary>
        /// The minimum number of lowest consensus nodes below which the consensus process will go wrong
        /// </summary>
        int M { get; }
        // <summary>
        // 前区块的区块头
        // </summary>
        /// <summary>
        /// previous block header of previous block
        /// </summary>
        Header PrevHeader { get; }
        // <summary>
        // 判定是否包含指定哈希值的交易
        // </summary>
        // <param name="hash">交易的哈希值</param>
        // <returns>判定是否包含</returns>
        //bool ContainsTransaction(UInt256 hash);
        /// <summary>
        /// determine if it contains a transcation of specified hash
        /// </summary>
        /// <param name="hash">transcation hash</param>
        /// <returns>judgement result</returns>
        bool TransactionExists(UInt256 hash);
        // <summary>
        // 验证指定的交易是否合法
        // </summary>
        // <param name="tx">指定的交易</param>
        // <returns>合法交易返回true</returns>
        /// <summary>
        /// Verify Transaction
        /// </summary>
        /// <param name="tx">specified transaction</param>
        /// <returns>Return true if verification passed</returns>
        bool VerifyTransaction(Transaction tx);
        // <summary>
        // 更换视图
        // </summary>
        // <param name="view_number">新的视图编号</param>
        /// <summary>
        /// Change view
        /// </summary>
        /// <param name="view_number">new view number</param>
        void ChangeView(byte view_number);
        // <summary>
        // 创建区块
        // </summary>
        // <returns>新创建的区块</returns>
        /// <summary>
        /// create block
        /// </summary>
        /// <returns>new block</returns>
        Block CreateBlock();

        //void Dispose();
        // <summary>
        // 计算议长编号
        // </summary>
        // <param name="view_number">当前视图编号</param>
        // <returns>新的议长编号</returns>
        /// <summary>
        /// Calculate the speaker number
        /// </summary>
        /// <param name="view_number">current view number</param>
        /// <returns>new speaker number</returns>
        uint GetPrimaryIndex(byte view_number);
        // <summary>
        // 构建ChangeView消息
        // </summary>
        // <returns>共识消息(ChangeView)</returns>
        /// <summary>
        /// build ChangeView message
        /// </summary>
        /// <returns>ChangeView message</returns>
        ConsensusPayload MakeChangeView();
        // <summary>
        // 构建一个只含有区块头，不含有交易内容的空区块
        // </summary>
        // <returns>只含有区块头的区块</returns>
        /// <summary>
        /// Build an empty block that contains only block headers and no transaction content
        /// </summary>
        /// <returns>a block that contains only block headers</returns>
        Block MakeHeader();
        // <summary>
        // 签名区块头
        // </summary>
        /// <summary>
        /// Sign the block header
        /// </summary>
        void SignHeader();
        // <summary>
        // 构建PrepareRequset的共识消息ConsensusPayload类
        // </summary>
        // <returns>PrepareRequset消息</returns>
        /// <summary>
        /// Build a consensus message for the PrepareRequest ConsensusPayload
        /// </summary>
        /// <returns>PrepareRequset message</returns>
        ConsensusPayload MakePrepareRequest();
        // <summary>
        // 构建PrepareResponse的共识消息ConsensusPayload类
        // </summary>
        // <param name="signature">对提案block的签名</param>
        // <returns>共识消息</returns>
        /// <summary>
        /// Build a consensus message for the PrepareResponse ConsensusPayload
        /// </summary>
        /// <param name="signature">signature of the proposal block</param>
        /// <returns>consensus message</returns>
        ConsensusPayload MakePrepareResponse(byte[] signature);
        // <summary>
        // 重置上下文数据
        // </summary>
        /// <summary>
        /// Reset context data
        /// </summary>
        void Reset();
        // <summary>
        // 填充提案block的数据
        // </summary>
        /// <summary>
        /// Fill the data of the proposal block
        /// </summary>
        void Fill();
        // <summary>
        // 收到PrepareRequest包后，校验PrepareRequest所带的提案block数据
        // </summary>
        // <returns>验证合法以后返回true</returns>
        /// <summary>
        /// 收到PrepareRequest包后，校验PrepareRequest所带的提案block数据
        /// </summary>
        /// <returns>验证合法以后返回true</returns>
        bool VerifyRequest();
    }
}
