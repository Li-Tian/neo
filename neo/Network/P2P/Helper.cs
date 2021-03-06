﻿using Neo.Network.P2P.Payloads;
using System.IO;

namespace Neo.Network.P2P
{
    /// <summary>
    /// 辅助工具类
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// 获取指定对象序列化后的数据
        /// </summary>
        /// <param name="verifiable">指定对象</param>
        /// <returns>序列化后的原始数据</returns>
        public static byte[] GetHashData(this IVerifiable verifiable)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                verifiable.SerializeUnsigned(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
