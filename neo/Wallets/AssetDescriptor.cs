using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;
using System;

namespace Neo.Wallets
{
    /// <summary>
    /// 资产的描述
    /// </summary>
    public class AssetDescriptor
    {
        /// <summary>
        /// 资产的ID
        /// </summary>
        public UIntBase AssetId;

        /// <summary>
        /// 资产的名字
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 资产的精度
        /// </summary>
        public byte Decimals;

        /// <summary>
        /// 构造函数, 传入一个assetId, 创建一个AssetDescriptor
        /// </summary>
        /// <param name="asset_id">资产的id标识</param>
        public AssetDescriptor(UIntBase asset_id)
        {
            if (asset_id is UInt160 asset_id_160)
            {
                byte[] script; 
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    sb.EmitAppCall(asset_id_160, "decimals");
                    sb.EmitAppCall(asset_id_160, "name");
                    script = sb.ToArray();
                }
                ApplicationEngine engine = ApplicationEngine.Run(script);
                if (engine.State.HasFlag(VMState.FAULT)) throw new ArgumentException();
                this.AssetId = asset_id;
                this.AssetName = engine.ResultStack.Pop().GetString();
                this.Decimals = (byte)engine.ResultStack.Pop().GetBigInteger();
            }
            else
            {
                AssetState state = Blockchain.Singleton.Store.GetAssets()[(UInt256)asset_id];
                this.AssetId = state.AssetId;
                this.AssetName = state.GetName();
                this.Decimals = state.Precision;
            }
        }

        /// <summary>
        /// 使用AssetName作为字符串返回
        /// </summary>
        /// <returns>返回这个AssetDescriptor的AssetName</returns>
        public override string ToString()
        {
            return AssetName;
        }
    }
}
