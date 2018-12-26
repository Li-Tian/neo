using Neo.Persistence;

namespace Neo.Network.P2P.Payloads
{
    // <summary>
    // Inventory接口
    // </summary>
    /// <summary>
    /// The interface of inventory
    /// </summary>
    public interface IInventory : IVerifiable
    {
        // <summary>
        // Inventory哈希值
        // </summary>
        /// <summary>
        /// The hash value of inventory
        /// </summary>
        UInt256 Hash { get; }
        // <summary>
        // Inventory类型
        // </summary>
        /// <summary>
        /// The inventory type
        /// </summary>
        InventoryType InventoryType { get; }
        // <summary>
        // 校验函数，根据快照进行校验
        // </summary>
        // <param name="snapshot">快照</param>
        // <returns>校验成功返回true，否则返回false</returns>
        /// <summary>
        /// The verify function, which verify according to the snapshop
        /// </summary>
        /// <param name="snapshot">The snapshot of blockchain</param>
        /// <returns>If verify successfully return true otherwise return false</returns>
        bool Verify(Snapshot snapshot);
    }
}
