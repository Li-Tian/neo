using Neo.Cryptography.ECC;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neo.SmartContract
{
    // <summary>
    // 合约参数上下文类，主要提供了合约参数上下文的相关判断，以及脚本对应见证人等功能
    // </summary>
    /// <summary>
    /// The contract parameter context class, mainly provides the relevant judgment of the contract parameter context, and the function of the script witness.
    /// </summary>
    public class ContractParametersContext
    {
        private class ContextItem
        {
            public byte[] Script;
            public ContractParameter[] Parameters;
            public Dictionary<ECPoint, byte[]> Signatures;

            private ContextItem() { }

            public ContextItem(Contract contract)
            {
                this.Script = contract.Script;
                this.Parameters = contract.ParameterList.Select(p => new ContractParameter { Type = p }).ToArray();
            }

            public static ContextItem FromJson(JObject json)
            {
                return new ContextItem
                {
                    Script = json["script"]?.AsString().HexToBytes(),
                    Parameters = ((JArray)json["parameters"]).Select(p => ContractParameter.FromJson(p)).ToArray(),
                    Signatures = json["signatures"]?.Properties.Select(p => new
                    {
                        PublicKey = ECPoint.Parse(p.Key, ECCurve.Secp256r1),
                        Signature = p.Value.AsString().HexToBytes()
                    }).ToDictionary(p => p.PublicKey, p => p.Signature)
                };
            }

            public JObject ToJson()
            {
                JObject json = new JObject();
                if (Script != null)
                    json["script"] = Script.ToHexString();
                json["parameters"] = new JArray(Parameters.Select(p => p.ToJson()));
                if (Signatures != null)
                {
                    json["signatures"] = new JObject();
                    foreach (var signature in Signatures)
                        json["signatures"][signature.Key.ToString()] = signature.Value.ToHexString();
                }
                return json;
            }
        }
        // <summary>
        // 可验证类型的对象，一般为交易，区块等
        // </summary>
        /// <summary>
        /// Objects of Verifiable types, typically transactions, blocks, etc.
        /// </summary>
        public readonly IVerifiable Verifiable;
        private readonly Dictionary<UInt160, ContextItem> ContextItems;
        // <summary>
        // 合约参数上下文是否已完成，如果所有ContextItem均不为空，且所有ContextItem对应的
        // ContractParameter的值不为空，则为true
        // </summary>
        /// <summary>
        /// Whether the contract parameter context has been completed, 
        /// true if all ContextItems are not empty, and the value of the ContractParameter corresponding to all ContextItems is not empty.
        /// </summary>
        public bool Completed
        {
            get
            {
                if (ContextItems.Count < ScriptHashes.Count)
                    return false;
                return ContextItems.Values.All(p => p != null && p.Parameters.All(q => q.Value != null));
            }
        }

        private UInt160[] _ScriptHashes = null;
        // <summary>
        // 根据当前快照获取所有需要验证的脚本的哈希值
        // </summary>
        /// <summary>
        /// Get the hash value of all scripts that need to be verified based on the current snapshot
        /// </summary>
        public IReadOnlyList<UInt160> ScriptHashes
        {
            get
            {
                if (_ScriptHashes == null)
                    using (Snapshot snapshot = Blockchain.Singleton.GetSnapshot())
                    {
                        _ScriptHashes = Verifiable.GetScriptHashesForVerifying(snapshot);
                    }
                return _ScriptHashes;
            }
        }
        // <summary>
        // 合约参数上下文构造函数
        // </summary>
        // <param name="verifiable">可验证类型的对象</param>
        /// <summary>
        /// Contract parameter context constructor
        /// </summary>
        /// <param name="verifiable">Verifiable object</param>
        public ContractParametersContext(IVerifiable verifiable)
        {
            this.Verifiable = verifiable;
            this.ContextItems = new Dictionary<UInt160, ContextItem>();
        }
        // <summary>
        // 对合约参数列表指定参数赋值
        // </summary>
        // <param name="contract">合约</param>
        // <param name="index">参数值所对应的下标</param>
        // <param name="parameter">参数值对象</param>
        // <returns>是否赋值成功，成功返回true,失败返回false</returns>
        /// <summary>
        /// Assign a value to the parameter of contract parameter list
        /// </summary>
        /// <param name="contract">contract</param>
        /// <param name="index">The index of parameter</param>
        /// <param name="parameter">Parameter object</param>
        /// <returns>True if the assignment is successful, otherwise false</returns>
        public bool Add(Contract contract, int index, object parameter)
        {
            ContextItem item = CreateItem(contract);
            if (item == null) return false;
            item.Parameters[index].Value = parameter;
            return true;
        }
        // <summary>
        // 将签名添加至参数表中，首先判断合约脚本是多签还是单签，<br/>
        // 如果是多签，则首先获取所有需要签名的地址列表，然后检测是否有需要该用户签名的，
        // 如果是，则把签名添加到签名列表中。当所有签名完毕时，对所有签名排序。<br/>
        // 如果是单签，则找到参数列表中签名参数所在的下标，将签名 signature 加入到合约的参数变量列表里面。
        // </summary>
        // <param name="contract">合约对象</param>
        // <param name="pubkey">公钥</param>
        // <param name="signature">签名</param>
        // <returns>签名结果</returns>
        // <exception cref="System.InvalidOperationException">多签合约，向合约添加签名参数失败时抛出</exception>
        // <exception cref="System.NotSupportedException">单签合约，但合约参数的类型没有一个是签名时抛出</exception>
        /// <summary>
        /// Add the signature to the parameter table, first determine whether the contract script is multi-signed or single-sign,<br/>
        /// if it is multi-sign, first obtain all the address lists that need to be signed, <br/>
        /// and then detect whether the user needs to be signed, and if so, then sign the signature Add to the list of signatures. <br/>
        /// Sort all signatures when all signatures are complete. If it is a single sign, <br/>
        /// find the index in which the signature parameter is located in the parameter list, <br/>
        /// and add the signature to the parameter table in the contract.
        /// </summary>
        /// <param name="contract">Contract</param>
        /// <param name="pubkey">Public key</param>
        /// <param name="signature">Signature</param>
        /// <returns>Signature result, true if successful, otherwise false</returns>
        /// <exception cref="System.InvalidOperationException">Multi-sign contract, throw an exception when adding a signature parameter to the contract fails</exception>
        /// <exception cref="System.NotSupportedException">Single sign contract, but none of the contract parameter types are Signature</exception>
        public bool AddSignature(Contract contract, ECPoint pubkey, byte[] signature)
        {
            if (contract.Script.IsMultiSigContract())
            {
                ContextItem item = CreateItem(contract);
                if (item == null) return false;
                if (item.Parameters.All(p => p.Value != null)) return false;
                if (item.Signatures == null)
                    item.Signatures = new Dictionary<ECPoint, byte[]>();
                else if (item.Signatures.ContainsKey(pubkey))
                    return false;
                List<ECPoint> points = new List<ECPoint>();
                {
                    int i = 0;
                    switch (contract.Script[i++])
                    {
                        case 1:
                            ++i;
                            break;
                        case 2:
                            i += 2;
                            break;
                    }
                    while (contract.Script[i++] == 33)
                    {
                        points.Add(ECPoint.DecodePoint(contract.Script.Skip(i).Take(33).ToArray(), ECCurve.Secp256r1));
                        i += 33;
                    }
                }
                if (!points.Contains(pubkey)) return false;
                item.Signatures.Add(pubkey, signature);
                if (item.Signatures.Count == contract.ParameterList.Length)
                {
                    Dictionary<ECPoint, int> dic = points.Select((p, i) => new
                    {
                        PublicKey = p,
                        Index = i
                    }).ToDictionary(p => p.PublicKey, p => p.Index);
                    byte[][] sigs = item.Signatures.Select(p => new
                    {
                        Signature = p.Value,
                        Index = dic[p.Key]
                    }).OrderByDescending(p => p.Index).Select(p => p.Signature).ToArray();
                    for (int i = 0; i < sigs.Length; i++)
                        if (!Add(contract, i, sigs[i]))
                            throw new InvalidOperationException();
                    item.Signatures = null;
                }
                return true;
            }
            else
            {
                int index = -1;
                for (int i = 0; i < contract.ParameterList.Length; i++)
                    if (contract.ParameterList[i] == ContractParameterType.Signature)
                        if (index >= 0)
                            throw new NotSupportedException();
                        else
                            index = i;

                if (index == -1)
                {
                    // unable to find ContractParameterType.Signature in contract.ParameterList 
                    // return now to prevent array index out of bounds exception
                    return false;
                }
                return Add(contract, index, signature);
            }
        }

        private ContextItem CreateItem(Contract contract)
        {
            if (ContextItems.TryGetValue(contract.ScriptHash, out ContextItem item))
                return item;
            if (!ScriptHashes.Contains(contract.ScriptHash))
                return null;
            item = new ContextItem(contract);
            ContextItems.Add(contract.ScriptHash, item);
            return item;
        }
        // <summary>
        // 从Json对象中获取合约参数上下文
        // </summary>
        // <param name="json">需要转换的Json对象</param>
        // <returns>从Json对象转换来的合约参数上下文</returns>
        // <exception cref="System.FormatException">
        // json对象中type属性转换成IVerifiable出的结果为false时抛出
        // </exception>
        /// <summary>
        /// Get the contract parameter context from the Json object
        /// </summary>
        /// <param name="json">Json object</param>
        /// <returns>Contract parameter context</returns>
        /// <exception cref="System.FormatException">
        /// Throws when the result of the type property converted to IVerifiable in the json object is false
        /// </exception>
        public static ContractParametersContext FromJson(JObject json)
        {
            IVerifiable verifiable = typeof(ContractParametersContext).GetTypeInfo().Assembly.CreateInstance(json["type"].AsString()) as IVerifiable;
            if (verifiable == null) throw new FormatException();
            using (MemoryStream ms = new MemoryStream(json["hex"].AsString().HexToBytes(), false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                verifiable.DeserializeUnsigned(reader);
            }
            ContractParametersContext context = new ContractParametersContext(verifiable);
            foreach (var property in json["items"].Properties)
            {
                context.ContextItems.Add(UInt160.Parse(property.Key), ContextItem.FromJson(property.Value));
            }
            return context;
        }
        // <summary>
        // 获取脚本哈希对应脚本的参数中，对应索引的参数
        // </summary>
        // <param name="scriptHash">合约的哈希值</param>
        // <param name="index">参数对应的索引</param>
        // <returns>获取到的合约参数</returns>
        /// <summary>
        /// Get the parameters of the corresponding index in the parameters of the script hash corresponding to the script.
        /// </summary>
        /// <param name="scriptHash">Contract hash</param>
        /// <param name="index">Index of parameter</param>
        /// <returns>Contract parameter</returns>
        public ContractParameter GetParameter(UInt160 scriptHash, int index)
        {
            return GetParameters(scriptHash)?[index];
        }
        // <summary>
        // 根据合约哈希获取合约的所有参数
        // </summary>
        // <param name="scriptHash">合约的哈希值</param>
        // <returns>合约哈希对应的所有参数列表</returns>
        /// <summary>
        /// Get all the parameters of the contract according to the contract hash
        /// </summary>
        /// <param name="scriptHash">Contract hash</param>
        /// <returns>All the parameters of the contract</returns>
        public IReadOnlyList<ContractParameter> GetParameters(UInt160 scriptHash)
        {
            if (!ContextItems.TryGetValue(scriptHash, out ContextItem item))
                return null;
            return item.Parameters;
        }
        // <summary>
        // 获取所有脚本见证人。对每个见证人，分别填充对应的参数和脚本信息。<br/>
        // 见证人，即脚本执行代码， 分为两段脚本: <br/>
        // InvocationScript 执行脚本（补充所需要的参数）<br/>
        // VerificationScript 验证脚本， 具体的执行指令。
        // </summary>
        // <returns>填充完成的所有脚本见证人</returns>
        // <exception cref="System.InvalidOperationException">脚本未完全执行成功时抛出</exception>
        /// <summary>
        /// Get all the script witnesses. For each witness, fill in the corresponding parameters and script information. <br/>
        /// Witness, the script execution code, is divided into two scripts: <br/>
        /// InvocationScript execution script (supplement of required parameters) <br/>
        /// VerificationScript validation script, specific execution instructions.
        /// </summary>
        /// <returns>All the script witnesses</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the script is not fully executed successfully</exception>
        public Witness[] GetWitnesses()
        {
            if (!Completed) throw new InvalidOperationException();
            Witness[] witnesses = new Witness[ScriptHashes.Count];
            for (int i = 0; i < ScriptHashes.Count; i++)
            {
                ContextItem item = ContextItems[ScriptHashes[i]];
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    foreach (ContractParameter parameter in item.Parameters.Reverse())
                    {
                        sb.EmitPush(parameter);
                    }
                    witnesses[i] = new Witness
                    {
                        InvocationScript = sb.ToArray(),
                        VerificationScript = item.Script ?? new byte[0]
                    };
                }
            }
            return witnesses;
        }
        // <summary>
        // 从字符串中按照JSON格式解析合约参数上下文
        // </summary>
        // <param name="value">需要解析的JSON格式的字符串</param>
        // <returns>解析出来的合约参数上下文</returns>
        /// <summary>
        /// Parse the contract parameter context from the string in JSON format
        /// </summary>
        /// <param name="value">String in JSON format that needs to be parsed</param>
        /// <returns>Parsed contract parameter context</returns>
        public static ContractParametersContext Parse(string value)
        {
            return FromJson(JObject.Parse(value));
        }
        // <summary>
        // 将目标转化为Json对象
        // </summary>
        // <returns>转化得到的Json对象</returns>
        /// <summary>
        /// Convert to a Json object
        /// </summary>
        /// <returns>Json object</returns>
        public JObject ToJson()
        {
            JObject json = new JObject();
            json["type"] = Verifiable.GetType().FullName;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                Verifiable.SerializeUnsigned(writer);
                writer.Flush();
                json["hex"] = ms.ToArray().ToHexString();
            }
            json["items"] = new JObject();
            foreach (var item in ContextItems)
                json["items"][item.Key.ToString()] = item.Value.ToJson();
            return json;
        }
        // <summary>
        // 重写ToString方法，用于将JObject对象转化成的json字符串
        // </summary>
        // <returns>输出JObject对象转化成的json字符串</returns>
        /// <summary>
        /// Rewrite the ToString method to convert the JObject object into a json string
        /// </summary>
        /// <returns>Json string</returns>
        public override string ToString()
        {
            return ToJson().ToString();
        }
    }
}
