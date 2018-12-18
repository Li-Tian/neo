using Neo.Cryptography.ECC;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Linq;

namespace Neo.SmartContract
{
    // <summary>
    // 合约类，提供了合约的构造方法，以及创建多方签名和单签合约的方法
    // </summary>
    /// <summary>
    /// Contract class, provides a constructor for the contract, and the function to create multi-party signatures and single-sign contracts
    /// </summary>
    public class Contract
    {
        // <summary>
        // 合约脚本的字节码
        // </summary>
        /// <summary>
        /// Script bytecode
        /// </summary>
        public byte[] Script;
        // <summary>
        // 合约的参数类型列表
        // </summary>
        /// <summary>
        /// List of parameter types for contract
        /// </summary>
        public ContractParameterType[] ParameterList;

        private string _address;
        // <summary>
        // 脚本哈希对应的地址
        // </summary>
        /// <summary>
        /// The address corresponding to the script hash
        /// </summary>
        public string Address
        {
            get
            {
                if (_address == null)
                {
                    _address = ScriptHash.ToAddress();
                }
                return _address;
            }
        }

        private UInt160 _scriptHash;
        // <summary>
        // 脚本哈希值
        // </summary>
        /// <summary>
        /// Script hash
        /// </summary>
        public virtual UInt160 ScriptHash
        {
            get
            {
                if (_scriptHash == null)
                {
                    _scriptHash = Script.ToScriptHash();
                }
                return _scriptHash;
            }
        }
        // <summary>
        // 创建一个合约
        // </summary>
        // <param name="parameterList">合约参数类型列表</param>
        // <param name="redeemScript">合约脚本字节码</param>
        // <returns>构建好的合约实例</returns>
        /// <summary>
        /// Create a contract
        /// </summary>
        /// <param name="parameterList">List of parameter types for contract</param>
        /// <param name="redeemScript">Script bytecode</param>
        /// <returns>Contract instance</returns>
        public static Contract Create(ContractParameterType[] parameterList, byte[] redeemScript)
        {
            return new Contract
            {
                Script = redeemScript,
                ParameterList = parameterList
            };
        }
        // <summary>
        // 创建一个多方签名合约
        // </summary>
        // <param name="m">多方签名能通过所需要的最小公钥个数</param>
        // <param name="publicKeys">多方签名的所有公钥</param>
        // <returns>构建好的多方签名合约实例</returns>
        /// <summary>
        /// Create a multi-party signature contract
        /// </summary>
        /// <param name="m">The minimum number of public keys required that multi-party signature can pass</param>
        /// <param name="publicKeys">All public keys of multi-party signature</param>
        /// <returns>multi-party signature contract instance</returns>
        public static Contract CreateMultiSigContract(int m, params ECPoint[] publicKeys)
        {
            return new Contract
            {
                Script = CreateMultiSigRedeemScript(m, publicKeys),
                ParameterList = Enumerable.Repeat(ContractParameterType.Signature, m).ToArray()
            };
        }
        // <summary>
        // 创建一个多方签名脚本，这里会依次向栈中压入m，按顺序排列的公钥，公钥个数，CHECKMULTISIG
        // </summary>
        // <param name="m">多方签名能通过所需要的最小公钥个数</param>
        // <param name="publicKeys">多方签名的所有公钥</param>
        // <returns>多方签名脚本对应的字节码</returns>
        /// <summary>
        /// Create a multi-party signature script, which will in turn push into the stack, m,the public key in order, the number of public keys, CHECKMULTISIG
        /// </summary>
        /// <param name="m">The minimum number of public keys required that multi-party signature can pass</param>
        /// <param name="publicKeys">All public keys of multi-party signature</param>
        /// <returns>The bytecode corresponding to the multi-party signature script</returns>
        public static byte[] CreateMultiSigRedeemScript(int m, params ECPoint[] publicKeys)
        {
            if (!(1 <= m && m <= publicKeys.Length && publicKeys.Length <= 1024))
                throw new ArgumentException();
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitPush(m);
                foreach (ECPoint publicKey in publicKeys.OrderBy(p => p))
                {
                    sb.EmitPush(publicKey.EncodePoint(true));
                }
                sb.EmitPush(publicKeys.Length);
                sb.Emit(OpCode.CHECKMULTISIG);
                return sb.ToArray();
            }
        }
        // <summary>
        // 创建一个单签合约
        // </summary>
        // <param name="publicKey">单签合约的公钥</param>
        // <returns>构建好的单签合约实例</returns>
        /// <summary>
        /// Create a single sign contract
        /// </summary>
        /// <param name="publicKey">Public key for single sign contract</param>
        /// <returns>Single sign contract instance</returns>
        public static Contract CreateSignatureContract(ECPoint publicKey)
        {
            return new Contract
            {
                Script = CreateSignatureRedeemScript(publicKey),
                ParameterList = new[] { ContractParameterType.Signature }
            };
        }
        // <summary>
        // 创建一个单签脚本，单签脚本格式：0x21(PUSH)+公钥+0xac(CHECKSIG)
        // </summary>
        // <param name="publicKey">单签脚本的公钥</param>
        // <returns>构建好的单签脚本实例</returns>
        /// <summary>
        /// Create a single sign script, single sign script format: 0x21 (PUSH) + public key + 0xAC (CHECKSIG)
        /// </summary>
        /// <param name="publicKey">Public key for single sign contract</param>
        /// <returns>Single sign script instance</returns>
        public static byte[] CreateSignatureRedeemScript(ECPoint publicKey)
        {
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitPush(publicKey.EncodePoint(true));
                sb.Emit(OpCode.CHECKSIG);
                return sb.ToArray();
            }
        }
    }
}
