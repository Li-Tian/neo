using Neo.IO.Caching;

namespace Neo.Consensus
{
    /// <summary>
    /// Consensus message type
    /// </summary>
    internal enum ConsensusMessageType : byte
    {
        /// <summary>
        /// Change view message
        /// </summary>
        [ReflectionCache(typeof(ChangeView))]
        ChangeView = 0x00,

        /// <summary>
        /// Prepare-request message
        /// </summary>
        [ReflectionCache(typeof(PrepareRequest))]
        PrepareRequest = 0x20,

        /// <summary>
        /// Prepare-response message
        /// </summary>
        [ReflectionCache(typeof(PrepareResponse))]
        PrepareResponse = 0x21,
    }
}
