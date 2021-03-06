﻿namespace Neo.Network.P2P.Payloads
{
    /// <summary>
    /// StateTransaction类型
    /// </summary>
    public enum StateType : byte
    {
        /// <summary>
        /// 投票
        /// </summary>
        Account = 0x40,

        /// <summary>
        /// 申请验证人
        /// </summary>
        Validator = 0x48
    }
}
