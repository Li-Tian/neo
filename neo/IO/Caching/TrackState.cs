namespace Neo.IO.Caching
{
    // <summary>
    // 追踪状态
    // </summary>
    /// <summary>
    /// Track State
    /// </summary>
    public enum TrackState : byte
    {
        // <summary>
        // 新建
        // </summary>
        /// <summary>
        /// None
        /// </summary>
        None,
        // <summary>
        // 已增加
        // </summary>
        /// <summary>
        /// Added
        /// </summary>
        Added,
        // <summary>
        // 已更改
        // </summary>
        /// <summary>
        /// Changed
        /// </summary>
        Changed,
        // <summary>
        // 已删除
        // </summary>
        /// <summary>
        /// Deleted
        /// </summary>
        Deleted
    }
}
