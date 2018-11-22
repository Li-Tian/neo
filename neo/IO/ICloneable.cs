namespace Neo.IO
{
    /// <summary>
    /// 对象的克隆方法接口
    /// </summary>
    /// <typeparam name="T">指定的对象类型</typeparam>
    public interface ICloneable<T>
    {
        /// <summary>
        /// 对象的克隆方法
        /// </summary>
        /// <returns>克隆出的对象</returns>
        T Clone();
        /// <summary>
        /// 从副本拷贝数据
        /// </summary>
        /// <param name="replica">副本</param>
        void FromReplica(T replica);
    }
}
