using Neo.Persistence;

namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// 库存接口
    /// </summary>
    public interface IInventory : IVerifiable
    {
        /// <summary>
        /// 库存哈希值
        /// </summary>
        UInt256 Hash { get; }
        /// <summary>
        /// 库存类型
        /// </summary>
        InventoryType InventoryType { get; }
        /// <summary>
        /// 库存校验函数，根据快照进行校验
        /// </summary>
        /// <param name="snapshot">快照</param>
        /// <returns>校验成功返回true，否则返回false</returns>
        bool Verify(Snapshot snapshot);
    }
}
