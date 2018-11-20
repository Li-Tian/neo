using System;

namespace Neo.Consensus
{
    /// <summary>
    /// 共识活动状态
    /// </summary>
    [Flags]
    internal enum ConsensusState : byte
    {
        /// <summary>
        /// 初始化
        /// </summary>
        Initial = 0x00,

        /// <summary>
        /// 议长
        /// </summary>
        Primary = 0x01,

        /// <summary>
        /// 议员
        /// </summary>
        Backup = 0x02,

        /// <summary>
        /// Repare-Request 消息已发送
        /// </summary>
        RequestSent = 0x04,

        /// <summary>
        /// Prepare-Request 消息已收过
        /// </summary>
        RequestReceived = 0x08,

        /// <summary>
        /// 提案block签名已发出
        /// </summary>
        /// <remarks>
        /// PrepareRequest消息中会附带议长的签名，PrepareResponse会附带议员签名
        /// </remarks>
        SignatureSent = 0x10,

        /// <summary>
        /// 提案block通过，完整的块已发出
        /// </summary>
        BlockSent = 0x20,

        /// <summary>
        /// 改变图中
        /// </summary>
        ViewChanging = 0x40,
    }
}
