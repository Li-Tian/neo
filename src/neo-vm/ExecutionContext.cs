using System;
using System.IO;

namespace Neo.VM
{
    // <summary>
    // 运行上下文类，定义了运行上下文的结构等
    // </summary>
    /// <summary>
    /// Execution context class, define the structure of the execution context, etc.
    /// </summary>
    public class ExecutionContext : IDisposable
    {
        // <summary>
        // 运行上下文的脚本，为只读数组
        // </summary>
        /// <summary>
        /// Execution context script, read-only array
        /// </summary>
        public readonly byte[] Script;
        internal readonly int RVCount;
        internal readonly BinaryReader OpReader;
        private readonly ICrypto crypto;
        // <summary>
        // 计算栈，主要用来根据指令执行相应的操作
        // </summary>
        /// <summary>
        /// Mainly used to perform corresponding operations according to instructions
        /// </summary>
        public RandomAccessStack<StackItem> EvaluationStack { get; } = new RandomAccessStack<StackItem>();
        // <summary>
        // 临时栈，用于保存计算过程中的临时数据
        // </summary>
        /// <summary>
        /// Used to save temporary data during the execution process
        /// </summary>
        public RandomAccessStack<StackItem> AltStack { get; } = new RandomAccessStack<StackItem>();
        // <summary>
        // 指令指针，指向正在读取的脚本位置
        // </summary>
        /// <summary>
        /// Point to the location of the script being read
        /// </summary>
        public int InstructionPointer
        {
            get
            {
                return (int)OpReader.BaseStream.Position;
            }
            set
            {
                OpReader.BaseStream.Seek(value, SeekOrigin.Begin);
            }
        }

        // <summary>
        // 下一条指令对应的操作码。
        // </summary>
        /// <summary>
        /// The opcode corresponding to the next instruction.
        /// </summary>
        public OpCode NextInstruction
        {
            get
            {
                var position = OpReader.BaseStream.Position;
                if (position >= Script.Length) return OpCode.RET;
                
                return (OpCode)Script[position];
            }
        }

        private byte[] _script_hash = null;
        // <summary>
        // 脚本哈希
        // </summary>
        /// <summary>
        /// Script Hash
        /// </summary>
        public byte[] ScriptHash
        {
            get
            {
                if (_script_hash == null)
                    _script_hash = crypto.Hash160(Script);
                return _script_hash;
            }
        }

        internal ExecutionContext(ExecutionEngine engine, byte[] script, int rvcount)
        {
            this.Script = script;
            this.RVCount = rvcount;
            this.OpReader = new BinaryReader(new MemoryStream(script, false));
            this.crypto = engine.Crypto;
        }
        // <summary>
        // 用于释放脚本资源
        // </summary>
        /// <summary>
        /// Used to release script resources
        /// </summary>
        public void Dispose()
        {
            OpReader.Dispose();
        }
    }
}
