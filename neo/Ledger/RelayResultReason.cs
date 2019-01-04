namespace Neo.Ledger
{
    // <summary>
    // 对接受到的转发消息的处理结果描述
    // </summary>
    /// <summary>
    /// The description of result of message relay
    /// </summary>
    public enum RelayResultReason : byte
    {
        // <summary>
        // 处理成功
        // </summary>
        /// <summary>
        /// Handle successfully
        /// </summary>
        Succeed,

        // <summary>
        // 已经存在
        // </summary>
        /// <summary>
        /// Exists already
        /// </summary>
        AlreadyExists,

        // <summary>
        // OOM错误
        // </summary>
        /// <summary>
        /// Out of memory error
        /// </summary>
        OutOfMemory,

        // <summary>
        // 不能进行验证
        // </summary>
        /// <summary>
        /// Can no be verified
        /// </summary>
        UnableToVerify,


        // <summary>
        // 非法数据
        // </summary>
        /// <summary>
        /// Invalid data
        /// </summary>
        Invalid,

        // <summary>
        // 未知
        // </summary>
        /// <summary>
        /// Unknown data
        /// </summary>
        Unknown
    }
}
