using Neo.IO.Caching;

namespace Neo.Consensus
{
    /// <summary>
    /// Consensus message type
    /// </summary>
    internal enum ConsensusMessageType : byte
    {
        /// <summary>
        /// ChangeView message
        /// </summary>
        [ReflectionCache(typeof(ChangeView))]
        ChangeView = 0x00,

        /// <summary>
        /// PrepareRequest message
        /// </summary>
        [ReflectionCache(typeof(PrepareRequest))]
        PrepareRequest = 0x20,

        /// <summary>
        /// PrepareResponse message
        /// </summary>
        [ReflectionCache(typeof(PrepareResponse))]
        PrepareResponse = 0x21,
    }
}
