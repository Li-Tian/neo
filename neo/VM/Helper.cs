using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.SmartContract;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VMArray = Neo.VM.Types.Array;
using VMBoolean = Neo.VM.Types.Boolean;

namespace Neo.VM
{
    // <summary>
    // VM的辅助操作类
    // </summary>
    /// <summary>
    /// VM helper class
    /// </summary>
    public static class Helper
    {
        // <summary>
        // 向脚本生成器中依次写入操作码
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="ops">操作码</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write the opcode in turn to the script builder
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="ops">opcodes</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder Emit(this ScriptBuilder sb, params OpCode[] ops)
        {
            foreach (OpCode op in ops)
                sb.Emit(op);
            return sb;
        }
        // <summary>
        // 向脚本生成器中写入函数调用，函数由脚本哈希指定
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="scriptHash">调用函数的脚本哈希</param>
        // <param name="useTailCall">是否使用尾调用形式，默认为false</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write a appcall to the script builder, which is specified by the script hash
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="useTailCall">Whether to use the tail call form, the default is false</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitAppCall(this ScriptBuilder sb, UInt160 scriptHash, bool useTailCall = false)
        {
            return sb.EmitAppCall(scriptHash.ToArray(), useTailCall);
        }
        // <summary>
        // 向脚本生成器中写入带ContractParameter类型参数的函数调用，函数由脚本哈希指定
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="scriptHash">调用函数的脚本哈希</param>
        // <param name="parameters">合约参数列表</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write a appcall with parameters of the ContractParameter type to the script builder, which is specified by the script hash
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="parameters">Contract parameter list</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitAppCall(this ScriptBuilder sb, UInt160 scriptHash, params ContractParameter[] parameters)
        {
            for (int i = parameters.Length - 1; i >= 0; i--)
                sb.EmitPush(parameters[i]);
            return sb.EmitAppCall(scriptHash);
        }
        // <summary>
        // 向脚本生成器中写入带指定操作的函数调用，函数由脚本哈希指定
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="scriptHash">调用函数的脚本哈希</param>
        // <param name="operation">指定操作，字符串类型</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write a appcall with the specified operation to the script builder, which is specified by the script hash
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="operation">Specified operation, string type</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitAppCall(this ScriptBuilder sb, UInt160 scriptHash, string operation)
        {
            sb.EmitPush(false);
            sb.EmitPush(operation);
            sb.EmitAppCall(scriptHash);
            return sb;
        }
        // <summary>
        // 向脚本生成器中写入带指定操作和ContractParameter类型参数的函数调用，函数由脚本哈希指定
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="scriptHash">调用函数的脚本哈希</param>
        // <param name="operation">指定操作，字符串类型</param>
        // <param name="args">ContractParameter类型合约参数列表</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write a appcall with the specified operation and the ContractParameter type parameter to the script builder, which is specified by the script hash
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="operation">Specified operation, string type</param>
        /// <param name="args">Contract parameter list</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitAppCall(this ScriptBuilder sb, UInt160 scriptHash, string operation, params ContractParameter[] args)
        {
            for (int i = args.Length - 1; i >= 0; i--)
                sb.EmitPush(args[i]);
            sb.EmitPush(args.Length);
            sb.Emit(OpCode.PACK);
            sb.EmitPush(operation);
            sb.EmitAppCall(scriptHash);
            return sb;
        }
        // <summary>
        // 向脚本生成器中写入带指定操作和object类型参数的函数调用，函数由脚本哈希指定
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="scriptHash">调用函数的脚本哈希</param>
        // <param name="operation">指定操作，字符串类型</param>
        // <param name="args">object类型合约参数列表</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write a appcall with the specified operation and the object type parameter to the script builder, which is specified by the script hash
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="operation">Specified operation, string type</param>
        /// <param name="args">Parameter list</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitAppCall(this ScriptBuilder sb, UInt160 scriptHash, string operation, params object[] args)
        {
            for (int i = args.Length - 1; i >= 0; i--)
                sb.EmitPush(args[i]);
            sb.EmitPush(args.Length);
            sb.Emit(OpCode.PACK);
            sb.EmitPush(operation);
            sb.EmitAppCall(scriptHash);
            return sb;
        }
        // <summary>
        // 向脚本生成器中写入可序列化数据，包括数据对应的压栈指令和数据本身
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="data">可序列化数据</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write serializable data to the script builder, including the push instruction corresponding to the data and the data itself
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="data">Serializable data</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitPush(this ScriptBuilder sb, ISerializable data)
        {
            return sb.EmitPush(data.ToArray());
        }
        // <summary>
        // 向脚本生成器中写入合约参数，根据参数类型的不同，会调用不同的写入方法
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="parameter">合约参数</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write contract parameters to the script builder, depending on the type of the parameter, different write methods will be called
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="parameter">Contract parameter</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitPush(this ScriptBuilder sb, ContractParameter parameter)
        {
            switch (parameter.Type)
            {
                case ContractParameterType.Signature:
                case ContractParameterType.ByteArray:
                    sb.EmitPush((byte[])parameter.Value);
                    break;
                case ContractParameterType.Boolean:
                    sb.EmitPush((bool)parameter.Value);
                    break;
                case ContractParameterType.Integer:
                    if (parameter.Value is BigInteger bi)
                        sb.EmitPush(bi);
                    else
                        sb.EmitPush((BigInteger)typeof(BigInteger).GetConstructor(new[] { parameter.Value.GetType() }).Invoke(new[] { parameter.Value }));
                    break;
                case ContractParameterType.Hash160:
                    sb.EmitPush((UInt160)parameter.Value);
                    break;
                case ContractParameterType.Hash256:
                    sb.EmitPush((UInt256)parameter.Value);
                    break;
                case ContractParameterType.PublicKey:
                    sb.EmitPush((ECPoint)parameter.Value);
                    break;
                case ContractParameterType.String:
                    sb.EmitPush((string)parameter.Value);
                    break;
                case ContractParameterType.Array:
                    {
                        IList<ContractParameter> parameters = (IList<ContractParameter>)parameter.Value;
                        for (int i = parameters.Count - 1; i >= 0; i--)
                            sb.EmitPush(parameters[i]);
                        sb.EmitPush(parameters.Count);
                        sb.Emit(OpCode.PACK);
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
            return sb;
        }
        // <summary>
        //  向脚本生成器中写入object类型数据
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="obj">object类型数据</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write object type data to the script builder
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="obj">Object type data</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitPush(this ScriptBuilder sb, object obj)
        {
            switch (obj)
            {
                case bool data:
                    sb.EmitPush(data);
                    break;
                case byte[] data:
                    sb.EmitPush(data);
                    break;
                case string data:
                    sb.EmitPush(data);
                    break;
                case BigInteger data:
                    sb.EmitPush(data);
                    break;
                case ISerializable data:
                    sb.EmitPush(data);
                    break;
                case sbyte data:
                    sb.EmitPush(data);
                    break;
                case byte data:
                    sb.EmitPush(data);
                    break;
                case short data:
                    sb.EmitPush(data);
                    break;
                case ushort data:
                    sb.EmitPush(data);
                    break;
                case int data:
                    sb.EmitPush(data);
                    break;
                case uint data:
                    sb.EmitPush(data);
                    break;
                case long data:
                    sb.EmitPush(data);
                    break;
                case ulong data:
                    sb.EmitPush(data);
                    break;
                case Enum data:
                    sb.EmitPush(BigInteger.Parse(data.ToString("d")));
                    break;
                default:
                    throw new ArgumentException();
            }
            return sb;
        }
        // <summary>
        // 向脚本生成器中写入指定的系统互操作服务调用，以及参数
        // </summary>
        // <param name="sb">待操作的脚本生成器</param>
        // <param name="api">系统互操作服务api字符串</param>
        // <param name="args">参数</param>
        // <returns>写入完成后的脚本生成器</returns>
        /// <summary>
        /// Write the specified system interop service call and parameters to the script builder
        /// </summary>
        /// <param name="sb">Script builder</param>
        /// <param name="api">System interop service api string</param>
        /// <param name="args">parameters</param>
        /// <returns>Script builder after writing</returns>
        public static ScriptBuilder EmitSysCall(this ScriptBuilder sb, string api, params object[] args)
        {
            for (int i = args.Length - 1; i >= 0; i--)
                EmitPush(sb, args[i]);
            return sb.EmitSysCall(api);
        }
        // <summary>
        // 将StackItem类型的值转化为ContractParameter类型
        // </summary>
        // <param name="item">需要转化的StackItem类型的值</param>
        // <returns>转化完成的ContractParameter类型的值</returns>
        /// <summary>
        /// Convert the value of the StackItem type to the ContractParameter type
        /// </summary>
        /// <param name="item">The value of the StackItem type that needs to be converted</param>
        /// <returns>The value of the converted ConvertParameter type</returns>
        public static ContractParameter ToParameter(this StackItem item)
        {
            return ToParameter(item, null);
        }

        private static ContractParameter ToParameter(StackItem item, List<Tuple<StackItem, ContractParameter>> context)
        {
            ContractParameter parameter = null;
            switch (item)
            {
                case VMArray array:
                    if (context is null)
                        context = new List<Tuple<StackItem, ContractParameter>>();
                    else
                        parameter = context.FirstOrDefault(p => ReferenceEquals(p.Item1, item))?.Item2;
                    if (parameter is null)
                    {
                        parameter = new ContractParameter { Type = ContractParameterType.Array };
                        context.Add(new Tuple<StackItem, ContractParameter>(item, parameter));
                        parameter.Value = array.Select(p => ToParameter(p, context)).ToList();
                    }
                    break;
                case Map map:
                    if (context is null)
                        context = new List<Tuple<StackItem, ContractParameter>>();
                    else
                        parameter = context.FirstOrDefault(p => ReferenceEquals(p.Item1, item))?.Item2;
                    if (parameter is null)
                    {
                        parameter = new ContractParameter { Type = ContractParameterType.Map };
                        context.Add(new Tuple<StackItem, ContractParameter>(item, parameter));
                        parameter.Value = map.Select(p => new KeyValuePair<ContractParameter, ContractParameter>(ToParameter(p.Key, context), ToParameter(p.Value, context))).ToList();
                    }
                    break;
                case VMBoolean _:
                    parameter = new ContractParameter
                    {
                        Type = ContractParameterType.Boolean,
                        Value = item.GetBoolean()
                    };
                    break;
                case ByteArray _:
                    parameter = new ContractParameter
                    {
                        Type = ContractParameterType.ByteArray,
                        Value = item.GetByteArray()
                    };
                    break;
                case Integer _:
                    parameter = new ContractParameter
                    {
                        Type = ContractParameterType.Integer,
                        Value = item.GetBigInteger()
                    };
                    break;
                case InteropInterface _:
                    parameter = new ContractParameter
                    {
                        Type = ContractParameterType.InteropInterface
                    };
                    break;
                default:
                    throw new ArgumentException();
            }
            return parameter;
        }
    }
}
