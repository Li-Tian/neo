using Neo.Persistence;

namespace Neo.Plugins
{
    /// <summary>
    /// 持久化插件
    /// </summary>
    public interface IPersistencePlugin
    {
        /// <summary>
        /// 当前快照
        /// </summary>
        /// <param name="snapshot">快照</param>
        void OnPersist(Snapshot snapshot);
    }
}
