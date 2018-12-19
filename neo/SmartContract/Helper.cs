using Neo.Cryptography;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SmartContract
{
    // <summary>
    // 智能合约帮助类，提供了脚本的多签、单签判断等功能
    // </summary>
    /// <summary>
    /// Smart contract helper class, providing script signature judgment and other functions
    /// </summary>
    public static class Helper
    {
        private static readonly Dictionary<string, uint> method_hashes = new Dictionary<string, uint>();

        // <summary>
        // 判断脚本是否为多签脚本,主要是根据多签脚本的结构来判断。
        // </summary>
        // <param name="script">需要判断的脚本</param>
        // <returns>是多签脚本则返回true，否则返回false</returns>
        /// <summary>
        /// It is judged whether the script is a multi-sign script, and is mainly judged according to the structure of the multi-sign script.
        /// </summary>
        /// <param name="script">Script to be judged</param>
        /// <returns>Returns true if the script is multi-sign script, otherwise returns false</returns>
        public static bool IsMultiSigContract(this byte[] script)
        {
            int m, n = 0;
            int i = 0;
            if (script.Length < 37) return false;
            if (script[i] > (byte)OpCode.PUSH16) return false;
            if (script[i] < (byte)OpCode.PUSH1 && script[i] != 1 && script[i] != 2) return false;
            switch (script[i])
            {
                case 1:
                    m = script[++i];
                    ++i;
                    break;
                case 2:
                    m = script.ToUInt16(++i);
                    i += 2;
                    break;
                default:
                    m = script[i++] - 80;
                    break;
            }
            if (m < 1 || m > 1024) return false;
            while (script[i] == 33)
            {
                i += 34;
                if (script.Length <= i) return false;
                ++n;
            }
            if (n < m || n > 1024) return false;
            switch (script[i])
            {
                case 1:
                    if (n != script[++i]) return false;
                    ++i;
                    break;
                case 2:
                    if (script.Length < i + 3 || n != script.ToUInt16(++i)) return false;
                    i += 2;
                    break;
                default:
                    if (n != script[i++] - 80) return false;
                    break;
            }
            if (script[i++] != (byte)OpCode.CHECKMULTISIG) return false;
            if (script.Length != i) return false;
            return true;
        }
        // <summary>
        // 判断脚本是否为签名脚本，根据签名脚本的结构来判断
        // 签名脚本的结构：0x21(PUSH)+公钥+0xac(CHECKSIG)
        // </summary>
        // <param name="script">需要判断的脚本</param>
        // <returns>是签名脚本则返回true，否则返回false</returns>
        /// <summary>
        /// Determine whether the script is a signature script, and judge according to the structure of the signature script.
        /// The structure of the signature script: 0x21 (PUSH) + public key + 0xAC (CHECKSIG)
        /// </summary>
        /// <param name="script">Script to be judged</param>
        /// <returns>Is true for signature scripts, false otherwise</returns>
        public static bool IsSignatureContract(this byte[] script)
        {
            if (script.Length != 35) return false;
            if (script[0] != 33 || script[34] != (byte)OpCode.CHECKSIG)
                return false;
            return true;
        }
        // <summary>
        // 判断脚本是否为标准脚本，如果脚本是签名脚本或者多签脚本，则该脚本为标准脚本
        // </summary>
        // <param name="script">需要判断的脚本</param>
        // <returns>是标准脚本则返回true，否则返回false</returns>
        /// <summary>
        /// Determine whether the script is a standard script. 
        /// If the script is a signature script or a multi-sign script, the script is a standard script.
        /// </summary>
        /// <param name="script">Script to be judged</param>
        /// <returns>Is true for standard scripts, false otherwise</returns>
        public static bool IsStandardContract(this byte[] script)
        {
            return script.IsSignatureContract() || script.IsMultiSigContract();
        }
        // <summary>
        // 取互操作服务名字的哈希
        // </summary>
        // <param name="method">互操作服务名字</param>
        // <returns>互操作服务名字的哈希的前32位</returns>
        /// <summary>
        /// Get the hash of the interoperability service name.
        /// </summary>
        /// <param name="method">Interoperability service name</param>
        /// <returns>The first 32 bits of the hash of the interoperable service name</returns>
        public static uint ToInteropMethodHash(this string method)
        {
            if (method_hashes.TryGetValue(method, out uint hash))
                return hash;
            hash = BitConverter.ToUInt32(Encoding.ASCII.GetBytes(method).Sha256(), 0);
            method_hashes[method] = hash;
            return hash;
        }

        // <summary>
        // 获取脚本的哈希值，对脚本做Hash160
        // </summary>
        // <param name="script">脚本字节数组</param>
        // <returns>脚本的哈希值</returns>
        /// <summary>
        /// Get the hash of the script, do Hash160 on the script
        /// </summary>
        /// <param name="script">Script byte array</param>
        /// <returns>The hash of the script</returns>
        public static UInt160 ToScriptHash(this byte[] script)
        {
            return new UInt160(Crypto.Default.Hash160(script));
        }

        internal static bool VerifyWitnesses(this IVerifiable verifiable, Snapshot snapshot)
        {
            UInt160[] hashes;
            try
            {
                hashes = verifiable.GetScriptHashesForVerifying(snapshot);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            if (hashes.Length != verifiable.Witnesses.Length) return false;
            for (int i = 0; i < hashes.Length; i++)
            {
                byte[] verification = verifiable.Witnesses[i].VerificationScript;
                if (verification.Length == 0)
                {
                    using (ScriptBuilder sb = new ScriptBuilder())
                    {
                        sb.EmitAppCall(hashes[i].ToArray());
                        verification = sb.ToArray();
                    }
                }
                else
                {
                    if (hashes[i] != verifiable.Witnesses[i].ScriptHash) return false;
                }
                using (ApplicationEngine engine = new ApplicationEngine(TriggerType.Verification, verifiable, snapshot, Fixed8.Zero))
                {
                    engine.LoadScript(verification);
                    engine.LoadScript(verifiable.Witnesses[i].InvocationScript);
                    if (!engine.Execute()) return false;
                    if (engine.ResultStack.Count != 1 || !engine.ResultStack.Pop().GetBoolean()) return false;
                }
            }
            return true;
        }
    }
}
