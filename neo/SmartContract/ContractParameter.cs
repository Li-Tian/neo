using Neo.Cryptography.ECC;
using Neo.IO.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Neo.SmartContract
{
    // <summary>
    // 合约参数类，主要包括对合约参数赋值，以及合约参数的格式转换方法
    // </summary>
    /// <summary>
    /// Contract parameter class, mainly including assignment of contract parameters, and format conversion method of contract parameters
    /// </summary>
    public class ContractParameter
    {
        // <summary>
        // 合约的参数类型
        // </summary>
        /// <summary>
        /// Contract parameter type
        /// </summary>
        public ContractParameterType Type;
        // <summary>
        // 合约参数的值
        // </summary>
        /// <summary>
        /// The value of the contract parameter
        /// </summary>
        public object Value;
        // <summary>
        // 默认构造函数
        // </summary>
        /// <summary>
        /// Default constructor
        /// </summary>
        public ContractParameter() { }
        // <summary>
        // 参数为ContractParameterType的构造函数
        // </summary>
        // <param name="type">合约参数类型</param>
        // <exception cref="System.ArgumentException">输入类型不是规定合约的参数类型时抛出</exception>
        /// <summary>
        /// Constructor with the parameter ContractParameterType
        /// </summary>
        /// <param name="type">Contract parameter type</param>
        /// <exception cref="System.ArgumentException">Thrown when the input type is not a parameter type that specifies the contract</exception>
        public ContractParameter(ContractParameterType type)
        {
            this.Type = type;
            switch (type)
            {
                case ContractParameterType.Signature:
                    this.Value = new byte[64];
                    break;
                case ContractParameterType.Boolean:
                    this.Value = false;
                    break;
                case ContractParameterType.Integer:
                    this.Value = 0;
                    break;
                case ContractParameterType.Hash160:
                    this.Value = new UInt160();
                    break;
                case ContractParameterType.Hash256:
                    this.Value = new UInt256();
                    break;
                case ContractParameterType.ByteArray:
                    this.Value = new byte[0];
                    break;
                case ContractParameterType.PublicKey:
                    this.Value = ECCurve.Secp256r1.G;
                    break;
                case ContractParameterType.String:
                    this.Value = "";
                    break;
                case ContractParameterType.Array:
                    this.Value = new List<ContractParameter>();
                    break;
                case ContractParameterType.Map:
                    this.Value = new List<KeyValuePair<ContractParameter, ContractParameter>>();
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        // <summary>
        // 从Json对象中获取合约参数
        // </summary>
        // <param name="json">包含合约参数类型和参数值的Json对象</param>
        // <returns>获取到的合约参数</returns>
        // <exception cref="System.ArgumentException">从JSON对象中解析出的type类型
        // 不是规定的合约参数类型时抛出</exception>
        /// <summary>
        /// Get contract parameters from Json object
        /// </summary>
        /// <param name="json">Json object containing contract parameter types and parameter values</param>
        /// <returns>Contract parameters</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the type parsed from the JSON object is not the specified contract parameter type</exception>
        public static ContractParameter FromJson(JObject json)
        {
            ContractParameter parameter = new ContractParameter
            {
                Type = json["type"].AsEnum<ContractParameterType>()
            };
            if (json["value"] != null)
                switch (parameter.Type)
                {
                    case ContractParameterType.Signature:
                    case ContractParameterType.ByteArray:
                        parameter.Value = json["value"].AsString().HexToBytes();
                        break;
                    case ContractParameterType.Boolean:
                        parameter.Value = json["value"].AsBoolean();
                        break;
                    case ContractParameterType.Integer:
                        parameter.Value = BigInteger.Parse(json["value"].AsString());
                        break;
                    case ContractParameterType.Hash160:
                        parameter.Value = UInt160.Parse(json["value"].AsString());
                        break;
                    case ContractParameterType.Hash256:
                        parameter.Value = UInt256.Parse(json["value"].AsString());
                        break;
                    case ContractParameterType.PublicKey:
                        parameter.Value = ECPoint.Parse(json["value"].AsString(), ECCurve.Secp256r1);
                        break;
                    case ContractParameterType.String:
                        parameter.Value = json["value"].AsString();
                        break;
                    case ContractParameterType.Array:
                        parameter.Value = ((JArray)json["value"]).Select(p => FromJson(p)).ToList();
                        break;
                    case ContractParameterType.Map:
                        parameter.Value = ((JArray)json["value"]).Select(p => new KeyValuePair<ContractParameter, ContractParameter>(FromJson(p["key"]), FromJson(p["value"]))).ToList();
                        break;
                    default:
                        throw new ArgumentException();
                }
            return parameter;
        }
        // <summary>
        // 给合约参数赋值，函数将根据参数类型做对应的赋值操作
        // </summary>
        // <param name="text">string类型的参数值</param>
        // <exception cref="System.ArgumentException">找不到所赋值的合约参数类型</exception>
        // <exception cref="System.FormatException">合约参数类型为签名类型时，输入的字符串
        // 字节长度不等于64时抛出</exception>
        /// <summary>
        /// Assign a value to the contract parameter, the function will perform the corresponding assignment operation according to the parameter type.
        /// </summary>
        /// <param name="text">Parameter value of string type</param>
        /// <exception cref="System.ArgumentException">Cannot find the contract parameter type of the assigned value</exception>
        /// <exception cref="System.FormatException">When the contract parameter type is Signature type, the input string length is not equal to 64.
        /// </exception>
        public void SetValue(string text)
        {
            switch (Type)
            {
                case ContractParameterType.Signature:
                    byte[] signature = text.HexToBytes();
                    if (signature.Length != 64) throw new FormatException();
                    Value = signature;
                    break;
                case ContractParameterType.Boolean:
                    Value = string.Equals(text, bool.TrueString, StringComparison.OrdinalIgnoreCase);
                    break;
                case ContractParameterType.Integer:
                    Value = BigInteger.Parse(text);
                    break;
                case ContractParameterType.Hash160:
                    Value = UInt160.Parse(text);
                    break;
                case ContractParameterType.Hash256:
                    Value = UInt256.Parse(text);
                    break;
                case ContractParameterType.ByteArray:
                    Value = text.HexToBytes();
                    break;
                case ContractParameterType.PublicKey:
                    Value = ECPoint.Parse(text, ECCurve.Secp256r1);
                    break;
                case ContractParameterType.String:
                    Value = text;
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        // <summary>
        // 将合约参数转化为Json对象类型
        // </summary>
        // <returns>转化完成的Json对象</returns>
        /// <summary>
        /// Convert contract parameters to Json object
        /// </summary>
        /// <returns>Json object</returns>
        public JObject ToJson()
        {
            return ToJson(this, null);
        }

        private static JObject ToJson(ContractParameter parameter, HashSet<ContractParameter> context)
        {
            JObject json = new JObject();
            json["type"] = parameter.Type;
            if (parameter.Value != null)
                switch (parameter.Type)
                {
                    case ContractParameterType.Signature:
                    case ContractParameterType.ByteArray:
                        json["value"] = ((byte[])parameter.Value).ToHexString();
                        break;
                    case ContractParameterType.Boolean:
                        json["value"] = (bool)parameter.Value;
                        break;
                    case ContractParameterType.Integer:
                    case ContractParameterType.Hash160:
                    case ContractParameterType.Hash256:
                    case ContractParameterType.PublicKey:
                    case ContractParameterType.String:
                        json["value"] = parameter.Value.ToString();
                        break;
                    case ContractParameterType.Array:
                        if (context is null)
                            context = new HashSet<ContractParameter>();
                        else if (context.Contains(parameter))
                            throw new InvalidOperationException();
                        context.Add(parameter);
                        json["value"] = new JArray(((IList<ContractParameter>)parameter.Value).Select(p => ToJson(p, context)));
                        break;
                    case ContractParameterType.Map:
                        if (context is null)
                            context = new HashSet<ContractParameter>();
                        else if (context.Contains(parameter))
                            throw new InvalidOperationException();
                        context.Add(parameter);
                        json["value"] = new JArray(((IList<KeyValuePair<ContractParameter, ContractParameter>>)parameter.Value).Select(p =>
                        {
                            JObject item = new JObject();
                            item["key"] = ToJson(p.Key, context);
                            item["value"] = ToJson(p.Value, context);
                            return item;
                        }));
                        break;
                }
            return json;
        }
        // <summary>
        // 将合约参数的值转化为String类型
        // </summary>
        // <returns>转化为String类型的参数值</returns>
        /// <summary>
        /// Convert the value of the contract parameter to a String type
        /// </summary>
        /// <returns>String type</returns>
        public override string ToString()
        {
            return ToString(this, null);
        }

        private static string ToString(ContractParameter parameter, HashSet<ContractParameter> context)
        {
            switch (parameter.Value)
            {
                case null:
                    return "(null)";
                case byte[] data:
                    return data.ToHexString();
                case IList<ContractParameter> data:
                    if (context is null) context = new HashSet<ContractParameter>();
                    if (context.Contains(parameter))
                    {
                        return "(array)";
                    }
                    else
                    {
                        context.Add(parameter);
                        StringBuilder sb = new StringBuilder();
                        sb.Append('[');
                        foreach (ContractParameter item in data)
                        {
                            sb.Append(ToString(item, context));
                            sb.Append(", ");
                        }
                        if (data.Count > 0)
                            sb.Length -= 2;
                        sb.Append(']');
                        return sb.ToString();
                    }
                case IList<KeyValuePair<ContractParameter, ContractParameter>> data:
                    if (context is null) context = new HashSet<ContractParameter>();
                    if (context.Contains(parameter))
                    {
                        return "(map)";
                    }
                    else
                    {
                        context.Add(parameter);
                        StringBuilder sb = new StringBuilder();
                        sb.Append('[');
                        foreach (var item in data)
                        {
                            sb.Append('{');
                            sb.Append(ToString(item.Key, context));
                            sb.Append(',');
                            sb.Append(ToString(item.Value, context));
                            sb.Append('}');
                            sb.Append(", ");
                        }
                        if (data.Count > 0)
                            sb.Length -= 2;
                        sb.Append(']');
                        return sb.ToString();
                    }
                default:
                    return parameter.Value.ToString();
            }
        }
    }
}
