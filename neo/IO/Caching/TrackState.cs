namespace Neo.IO.Caching
{
    /// <summary>
    /// 追踪状态
    /// </summary>
    public enum TrackState : byte
    {
        /// <summary>
        /// 新建
        /// </summary>
        None,
        /// <summary>
        /// 被增加
        /// </summary>
        Added,
        /// <summary>
        /// 被更改
        /// </summary>
        Changed,
        /// <summary>
        /// 被删除
        /// </summary>
        Deleted
    }
}
