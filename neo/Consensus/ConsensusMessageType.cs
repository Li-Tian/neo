using Neo.IO.Caching;

namespace Neo.Consensus
{
    /// <summary>
    /// 共识消息类型
    /// </summary>
    internal enum ConsensusMessageType : byte
    {
        /// <summary>
        /// ChangeView 改变视图，由节点遇到超时或校验失败时发出
        /// </summary>
        [ReflectionCache(typeof(ChangeView))]
        ChangeView = 0x00,

        /// <summary>
        /// PrepareRequest消息，由议长发送
        /// </summary>
        [ReflectionCache(typeof(PrepareRequest))]
        PrepareRequest = 0x20,

        /// <summary>
        /// PrepareResponse消息，由议员发送
        /// </summary>
        [ReflectionCache(typeof(PrepareResponse))]
        PrepareResponse = 0x21,
    }
}
