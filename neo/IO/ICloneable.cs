namespace Neo.IO
{
    // <summary>
    // 对象的克隆方法接口
    // </summary>
    // <typeparam name="T">指定的对象类型</typeparam>
    /// <summary>
    /// clone method interface
    /// </summary>
    /// <typeparam name="T">specified method</typeparam>
    public interface ICloneable<T>
    {
        // <summary>
        // 对象的克隆方法
        // </summary>
        // <returns>克隆出的对象</returns>
        /// <summary>
        /// clone method
        /// </summary>
        /// <returns>Clone object</returns>
        T Clone();
        /// <summary>
        /// Copy data from a replica
        /// </summary>
        /// <param name="replica">replica</param>
        void FromReplica(T replica);
    }
}
