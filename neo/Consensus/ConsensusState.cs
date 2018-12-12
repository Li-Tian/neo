using System;

namespace Neo.Consensus
{
    /// <summary>
    /// Consensus state
    /// </summary>
    [Flags]
    public enum ConsensusState : byte
    {
        /// <summary>
        /// Initial
        /// </summary>
        Initial = 0x00,

        /// <summary>
        /// The Speaker
        /// </summary>
        Primary = 0x01,

        /// <summary>
        /// The Delegates
        /// </summary>
        Backup = 0x02,

        /// <summary>
        /// The prepare-request message has been send
        /// </summary>
        RequestSent = 0x04,

        /// <summary>
        /// The prepare-request message has been received
        /// </summary>
        RequestReceived = 0x08,

        /// <summary>
        /// The prepare-response message has been send
        /// </summary>
        SignatureSent = 0x10,

        /// <summary>
        /// The full block has been published before the next round start
        /// </summary>
        BlockSent = 0x20,

        /// <summary>
        /// View changing
        /// </summary>
        ViewChanging = 0x40,
    }
}
