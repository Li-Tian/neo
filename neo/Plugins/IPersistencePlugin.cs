using Neo.Persistence;

namespace Neo.Plugins
{
    /// <summary>
    /// 持久化插件。定义在收到区块并将其保存到本地数据库时的额外处理。
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
