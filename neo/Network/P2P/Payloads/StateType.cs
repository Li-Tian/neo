namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // StateTransaction类型
    // </summary>
    /// <summary>
    /// StateTransaction type
    /// </summary>
    public enum StateType : byte
    {
        // <summary>
        // 投票
        // </summary>
        /// <summary>
        /// The acount of votes
        /// </summary>
        Account = 0x40,

        // <summary>
        // 申请验证人
        // </summary>
        /// <summary>
        /// The validator applicant
        /// </summary>
        Validator = 0x48
    }
}
