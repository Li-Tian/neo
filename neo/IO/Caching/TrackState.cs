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
        /// 已增加
        /// </summary>
        Added,
        /// <summary>
        /// 已更改
        /// </summary>
        Changed,
        /// <summary>
        /// 已删除
        /// </summary>
        Deleted
    }
}
