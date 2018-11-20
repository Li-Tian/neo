namespace Neo.Ledger
{
    /// <summary>
    /// 对消息转发的处理结果描述
    /// </summary>
    public enum RelayResultReason : byte
    {
        /// <summary>
        /// 处理成功
        /// </summary>
        Succeed,

        /// <summary>
        /// 已经存在
        /// </summary>
        AlreadyExists,

        /// <summary>
        /// OOM错误
        /// </summary>
        OutOfMemory,

        /// <summary>
        /// 不能进行验证
        /// </summary>
        UnableToVerify,

        /// <summary>
        /// 非法数据
        /// </summary>
        Invalid,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown
    }
}
