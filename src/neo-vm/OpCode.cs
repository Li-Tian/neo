namespace Neo.VM
{
    /// <summary>
    /// 枚举类，NVM支持的所有操作码。
    /// </summary>
    public enum OpCode : byte
    {
        // Constants
        // <summary>
        // An empty array of bytes is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 将一个空字节数组压入栈中。
        /// </summary>
        PUSH0 = 0x00,
        /// <summary>
        /// 等于PUSH0，将一个空字节数组压入栈中。
        /// </summary>
        PUSHF = PUSH0,
        // <summary>
        // 0x01-0x4B The next opcode bytes is data to be pushed onto the stack
        // </summary>
        /// <summary>
        /// 0x01-0x4B 本指令后操作码指定的字节数对应的数据将会被压栈。
        /// </summary>
        PUSHBYTES1 = 0x01,
        /// <summary>
        /// 0x01-0x4B 本指令后操作码指定的字节数对应的数据将会被压栈。
        /// </summary>
        PUSHBYTES75 = 0x4B,
        // <summary>
        // The next byte contains the number of bytes to be pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入一个字节数组，其长度由本指令后的1字节指定。
        /// </summary>
        PUSHDATA1 = 0x4C,
        // <summary>
        // The next two bytes contain the number of bytes to be pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入一个字节数组，其长度由本指令后的2字节指定。
        /// </summary>
        PUSHDATA2 = 0x4D,
        // <summary>
        // The next four bytes contain the number of bytes to be pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入一个字节数组，其长度由本指令后的4字节指定。
        /// </summary>
        PUSHDATA4 = 0x4E,
        // <summary>
        // The number -1 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数-1。
        /// </summary>
        PUSHM1 = 0x4F,
        // <summary>
        // The number 1 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数1。
        /// </summary>
        PUSH1 = 0x51,
        /// <summary>
        /// 等于PUSH1,向栈中压入数1。
        /// </summary>
        PUSHT = PUSH1,
        // <summary>
        // The number 2 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数2。
        /// </summary>
        PUSH2 = 0x52,
        // <summary>
        // The number 3 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数3。
        /// </summary>
        PUSH3 = 0x53,
        // <summary>
        // The number 4 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数4。
        /// </summary>
        PUSH4 = 0x54,
        // <summary>
        // The number 5 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数5。
        /// </summary>
        PUSH5 = 0x55,
        // <summary>
        // The number 6 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数6。
        /// </summary>
        PUSH6 = 0x56,
        // <summary>
        // The number 7 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数7。
        /// </summary>
        PUSH7 = 0x57,
        // <summary>
        // The number 8 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数8。
        /// </summary>
        PUSH8 = 0x58,
        // <summary>
        // The number 9 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数9。
        /// </summary>
        PUSH9 = 0x59,
        // <summary>
        // The number 10 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数10。
        /// </summary>
        PUSH10 = 0x5A,
        // <summary>
        // The number 11 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数11。
        /// </summary>
        PUSH11 = 0x5B,
        // <summary>
        // The number 12 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数12。
        /// </summary>
        PUSH12 = 0x5C,
        // <summary>
        // The number 13 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数13。
        /// </summary>
        PUSH13 = 0x5D,
        // <summary>
        // The number 14 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数14。
        /// </summary>
        PUSH14 = 0x5E,
        // <summary>
        // The number 15 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数15。
        /// </summary>
        PUSH15 = 0x5F,
        // <summary>
        // The number 16 is pushed onto the stack.
        // </summary>
        /// <summary>
        /// 向栈中压入数16。
        /// </summary>
        PUSH16 = 0x60,

        // Flow control
        // <summary>
        //  Does nothing.
        // </summary>
        /// <summary>
        /// 无操作
        /// </summary>
        NOP = 0x61,
        // <summary>
        // Reads a 2-byte value n and a jump is performed to relative position n-3.
        // </summary>
        /// <summary>
        /// 读取一个2字节的值n，然后跳转到相对位置n-3。
        /// </summary>
        JMP = 0x62,
        // <summary>
        // A boolean value b is taken from main stack and reads a 2-byte value n, if b is True then a jump is performed to relative position n-3.
        // </summary>
        /// <summary>
        /// 从主栈中获取布尔值b并读取2字节值n，如果b为真，则执行跳转到相对位置n-3。
        /// </summary>
        JMPIF = 0x63,
        // <summary>
        // A boolean value b is taken from main stack and reads a 2-byte value n, if b is False then a jump is performed to relative position n-3.
        // </summary>
        /// <summary>
        /// 从主栈中获取布尔值b并读取2字节值n，如果b为假，则执行跳转到相对位置n-3。
        /// </summary>
        JMPIFNOT = 0x64,
        // <summary>
        // Current context is copied to the invocation stack. Reads a 2-byte value n and a jump is performed to relative position n-3.
        // </summary>
        /// <summary>
        /// 当前上下文被复制到调用栈。读取一个2字节的值n，然后跳转到相对位置n-3。
        /// </summary>
        CALL = 0x65,
        // <summary>
        // Stops the execution if invocation stack is empty.
        // </summary>
        /// <summary>
        /// 当调用栈为空时停止运行
        /// </summary>
        RET = 0x66,
        // <summary>
        // Reads a scripthash and executes the corresponding contract.
        // </summary>
        /// <summary>
        /// 读取脚本哈希并执行对应的合约
        /// </summary>
        APPCALL = 0x67,
        // <summary>
        // Reads a string and executes the corresponding operation.
        // </summary>
        /// <summary>
        /// 读取字符串并执行相应的操作
        /// </summary>
        SYSCALL = 0x68,
        // <summary>
        // Reads a scripthash and executes the corresponding contract. Disposes the top item on invocation stack.
        // </summary>
        /// <summary>
        /// 读取脚本哈希并执行对应的合约。释放调用栈栈顶元素。不再返回调用方。
        /// </summary>
        TAILCALL = 0x69,


        // Stack
        // <summary>
        // Duplicates the item on top of alt stack and put it on top of main stack.
        // </summary>
        /// <summary>
        /// 复制临时栈栈顶的元素，并将其压入主栈上。
        /// </summary>
        DUPFROMALTSTACK = 0x6A,
        // <summary>
        // Puts the input onto the top of the alt stack. Removes it from the main stack.
        // </summary>
        /// <summary>
        /// 将主栈栈顶元素压入临时栈，并从主栈移除该元素。
        /// </summary>
        TOALTSTACK = 0x6B,
        // <summary>
        // Puts the input onto the top of the main stack. Removes it from the alt stack.
        // </summary>
        /// <summary>
        /// 将临时栈栈顶元素压入主栈，并从临时栈移除该元素。
        /// </summary>
        FROMALTSTACK = 0x6C,
        // <summary>
        // The item n back in the main stack is removed.
        // </summary>
        /// <summary>
        /// 移除计算栈栈顶的元素n，并移除剩余的索引为n的元素。
        /// </summary>
        XDROP = 0x6D,
        // <summary>
        // The item n back in the main stack in swapped with top stack item.
        // </summary>
        /// <summary>
        /// 移除计算栈栈顶的元素n，并将剩余的索引为0的元素和索引为n的元素交换位置。
        /// </summary>
        XSWAP = 0x72,
        // <summary>
        // The item on top of the main stack is copied and inserted to the position n in the main stack.
        // </summary>
        /// <summary>
        /// 移除计算栈栈顶的元素n，并将剩余的索引为0的元素复制并插入到索引为n的位置。
        /// </summary>
        XTUCK = 0x73,
        // <summary>
        // Puts the number of stack items onto the stack.
        // </summary>
        /// <summary>
        /// 将当前栈中的元素数量压栈。
        /// </summary>
        DEPTH = 0x74,
        // <summary>
        // Removes the top stack item.
        // </summary>
        /// <summary>
        /// 移除栈顶的元素。
        /// </summary>
        DROP = 0x75,
        // <summary>
        // Duplicates the top stack item.
        // </summary>
        /// <summary>
        /// 复制栈顶的元素。
        /// </summary>
        DUP = 0x76,
        // <summary>
        // Removes the second-to-top stack item.
        // </summary>
        /// <summary>
        /// 移除栈顶的第2个元素。
        /// </summary>
        NIP = 0x77,
        // <summary>
        // Copies the second-to-top stack item to the top.
        // </summary>
        /// <summary>
        /// 复制栈顶的第二个元素，并压入栈顶。
        /// </summary>
        OVER = 0x78,
        // <summary>
        // The item n back in the stack is copied to the top.
        // </summary>
        /// <summary>
        /// 移除栈顶的元素n，并将剩余的索引为n的元素复制到栈顶。
        /// </summary>
        PICK = 0x79,
        // <summary>
        // The item n back in the stack is moved to the top.
        // </summary>
        /// <summary>
        /// 移除栈顶的元素n，并将剩余的索引为n的元素移动到栈顶。
        /// </summary>
        ROLL = 0x7A,
        // <summary>
        // The top three items on the stack are rotated to the left.
        // </summary>
        /// <summary>
        /// 移除栈顶的第3个元素，并将其压入栈顶。
        /// </summary>
        ROT = 0x7B,
        // <summary>
        // The top two items on the stack are swapped.
        // </summary>
        /// <summary>
        /// 交换栈顶两个元素的位置。
        /// </summary>
        SWAP = 0x7C,
        // <summary>
        // The item at the top of the stack is copied and inserted before the second-to-top item.
        // </summary>
        /// <summary>
        /// 复制栈顶的元素到索引为2的位置。
        /// </summary>
        TUCK = 0x7D,


        // Splice
        // <summary>
        // Concatenates two strings.
        // </summary>
        /// <summary>
        /// 移除栈顶的两个元素，并将其拼接后压入栈顶。
        /// </summary>
        CAT = 0x7E,
        // <summary>
        // Returns a section of a string.
        // </summary>
        /// <summary>
        /// 返回字符串的子串。
        /// </summary>
        SUBSTR = 0x7F,
        // <summary>
        // Keeps only characters left of the specified point in a string.
        // </summary>
        /// <summary>
        /// 只保留字符串中指定位置左边的字符。
        /// </summary>
        LEFT = 0x80,
        // <summary>
        // Keeps only characters right of the specified point in a string.
        // </summary>
        /// <summary>
        /// 只保留字符串中指定位置右边的字符。
        /// </summary>
        RIGHT = 0x81,
        // <summary>
        // Returns the length of the input string.
        // </summary>
        /// <summary>
        /// 返回输入字符串的长度。
        /// </summary>
        SIZE = 0x82,


        // Bitwise logic
        // <summary>
        // Flips all of the bits in the input.
        // </summary>
        /// <summary>
        /// 对输入的元素按位取反。
        /// </summary>
        INVERT = 0x83,
        // <summary>
        // Boolean and between each bit in the inputs.
        // </summary>
        /// <summary>
        /// 对输入的元素做按位与运算。
        /// </summary>
        AND = 0x84,
        // <summary>
        // Boolean or between each bit in the inputs.
        // </summary>
        /// <summary>
        /// 对输入的元素做按位或运算。
        /// </summary>
        OR = 0x85,
        // <summary>
        // Boolean exclusive or between each bit in the inputs.
        // </summary>
        /// <summary>
        /// 对输入的元素做按位异或运算。
        /// </summary>
        XOR = 0x86,
        // <summary>
        // Returns 1 if the inputs are exactly equal, 0 otherwise.
        // </summary>
        /// <summary>
        /// 如果两个输入完全相等则返回1，否则返回0
        /// </summary>
        EQUAL = 0x87,
        //OP_EQUALVERIFY = 0x88, // Same as OP_EQUAL, but runs OP_VERIFY afterward.
        //OP_RESERVED1 = 0x89, // Transaction is invalid unless occuring in an unexecuted OP_IF branch
        //OP_RESERVED2 = 0x8A, // Transaction is invalid unless occuring in an unexecuted OP_IF branch


        // Arithmetic
        // Note: Arithmetic inputs are limited to signed 32-bit integers, but may overflow their output.
        // <summary>
        // 1 is added to the input.
        // </summary>
        /// <summary>
        /// 对输入加一
        /// </summary>
        INC = 0x8B,
        // <summary>
        // 1 is subtracted from the input.
        // </summary>
        /// <summary>
        /// 对输入减一
        /// </summary>
        DEC = 0x8C,
        // <summary>
        // Puts the sign of top stack item on top of the main stack. If value is negative, put -1; if positive, put 1; if value is zero, put 0.
        // </summary>
        /// <summary>
        /// 获取栈顶的大整数的符号（负返回-1、正返回1，零返回0）。
        /// </summary>
        SIGN = 0x8D,
        // <summary>
        // The sign of the input is flipped.
        // </summary>
        /// <summary>
        /// 计算输入的相反数。
        /// </summary>
        NEGATE = 0x8F,
        // <summary>
        // The input is made positive.
        // </summary>
        /// <summary>
        /// 计算输入的绝对值。
        /// </summary>
        ABS = 0x90,
        // <summary>
        // If the input is 0 or 1, it is flipped. Otherwise the output will be 0.
        // </summary>
        /// <summary>
        /// 对输入执行逻辑非运算。
        /// </summary>
        NOT = 0x91,
        // <summary>
        // Returns 0 if the input is 0. 1 otherwise.
        // </summary>
        /// <summary>
        /// 如果输入为0则返回0，否则返回1
        /// </summary>
        NZ = 0x92,
        // <summary>
        // a is added to b.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行加法运算。
        /// </summary>
        ADD = 0x93,
        // <summary>
        // b is subtracted from a.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行减法运算。
        /// </summary>
        SUB = 0x94,
        // <summary>
        // a is multiplied by b.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行乘法运算。
        /// </summary>
        MUL = 0x95,
        // <summary>
        // a is divided by b.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行除法运算。
        /// </summary>
        DIV = 0x96,
        // <summary>
        // Returns the remainder after dividing a by b.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行求余运算。
        /// </summary>
        MOD = 0x97,
        // <summary>
        // Shifts a left b bits, preserving sign.
        // </summary>
        /// <summary>
        /// 对栈中的大整数执行左移运算。
        /// </summary>
        SHL = 0x98,
        // <summary>
        // Shifts a right b bits, preserving sign.
        // </summary>
        /// <summary>
        /// 对栈中的大整数执行右移运算。
        /// </summary>
        SHR = 0x99,
        // <summary>
        // If both a and b are not 0, the output is 1. Otherwise 0.
        // </summary>
        /// <summary>
        /// 对栈顶的两个元素执行逻辑与运算。
        /// </summary>
        BOOLAND = 0x9A,
        // <summary>
        // If a or b is not 0, the output is 1. Otherwise 0.
        // </summary>
        /// <summary>
        /// 对栈顶的两个元素执行逻辑或运算。
        /// </summary>
        BOOLOR = 0x9B,
        // <summary>
        // Returns 1 if the numbers are equal, 0 otherwise.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行相等判断，如果相等则返回1，否则返回0。
        /// </summary>
        NUMEQUAL = 0x9C,
        // <summary>
        // Returns 1 if the numbers are not equal, 0 otherwise.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行不相等判断，如果不相等则返回1，否则返回0。
        /// </summary>
        NUMNOTEQUAL = 0x9E,
        // <summary>
        // Returns 1 if a is less than b, 0 otherwise.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行小于判断。
        /// </summary>
        LT = 0x9F,
        // <summary>
        // Returns 1 if a is greater than b, 0 otherwise.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行大于判断。
        /// </summary>
        GT = 0xA0,
        // <summary>
        // Returns 1 if a is less than or equal to b, 0 otherwise.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行小于等于判断。
        /// </summary>
        LTE = 0xA1,
        // <summary>
        // Returns 1 if a is greater than or equal to b, 0 otherwise.
        // </summary>
        /// <summary>
        /// 对栈顶的两个大整数执行大于等于判断。
        /// </summary>
        GTE = 0xA2,
        // <summary>
        // Returns the smaller of a and b.
        // </summary>
        /// <summary>
        /// 返回a和b中更小者
        /// </summary>
        MIN = 0xA3,
        // <summary>
        // Returns the larger of a and b.
        // </summary>
        /// <summary>
        /// 返回a和b中更大者
        /// </summary>
        MAX = 0xA4,
        // <summary>
        // Returns 1 if x is within the specified range (left-inclusive), 0 otherwise.
        // </summary>
        /// <summary>
        /// 判断栈中的大整数是否在指定的数值范围内。如果是则返回1，否则返回0
        /// </summary>
        WITHIN = 0xA5,


        // Crypto
        //RIPEMD160 = 0xA6, // The input is hashed using RIPEMD-160.
        // <summary>
        // The input is hashed using SHA-1.
        // </summary>
        /// <summary>
        /// 对输入执行SHA-1运算。
        /// </summary>
        SHA1 = 0xA7,
        // <summary>
        // The input is hashed using SHA-256.
        // </summary>
        /// <summary>
        /// 对输入执行SHA-256运算。
        /// </summary>
        SHA256 = 0xA8,
        // <summary>
        // The input is hashed using Hash160: first with SHA-256 and then with RIPEMD-160.
        // </summary>
        /// <summary>
        /// 对输入执行Hash160运算：首先做SHA-256运算，再做RIPEMD-160运算。
        /// </summary>
        HASH160 = 0xA9,
        // <summary>
        // The input is hashed using Hash256: twice with SHA-256.
        // </summary>
        /// <summary>
        /// 对输入执行Hash256运算：即两次SHA-256运算。
        /// </summary>
        HASH256 = 0xAA,
        // <summary>
        // The publickey and signature are taken from main stack. Verifies if transaction was signed by given publickey and a boolean output is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 利用栈顶元素中的签名和公钥，对当前验证对象执行内置的非对称签名验证操作。
        /// </summary>
        CHECKSIG = 0xAC,
        // <summary>
        // The publickey, signature and message are taken from main stack. Verifies if given message was signed by given publickey and a boolean output is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 利用栈顶元素中的签名、公钥和验证对象，执行内置的非对称签名验证操作。
        /// </summary>
        VERIFY = 0xAD,
        // <summary>
        // A set of n public keys (an array or value n followed by n pubkeys) is validated against a set of m signatures (an array or value m followed by m signatures). Verify transaction as multisig and a boolean output is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 利用栈顶元素中的多个签名和公钥，对当前验证对象执行内置的非对称多重签名验证操作。
        /// </summary>
        CHECKMULTISIG = 0xAE,


        // Array
        // <summary>
        // An array is removed from top of the main stack. Its size is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 获取栈顶的数组的元素数量。
        /// </summary>
        ARRAYSIZE = 0xC0,
        // <summary>
        // A value n is taken from top of main stack. The next n items on main stack are removed, put inside n-sized array and this array is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 将栈顶的n个元素打包成数组。
        /// </summary>
        PACK = 0xC1,
        // <summary>
        // An array is removed from top of the main stack. Its elements are put on top of the main stack (in reverse order) and the array size is also put on main stack.
        // </summary>
        /// <summary>
        /// 将栈顶的数组拆包成元素序列。
        /// </summary>
        UNPACK = 0xC2,
        // <summary>
        // An input index n (or key) and an array (or map) are taken from main stack. Element array[n] (or map[n]) is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 获取栈顶的数组中的指定位置n的元素。
        /// </summary>
        PICKITEM = 0xC3,
        // <summary>
        // A value v, index n (or key) and an array (or map) are taken from main stack. Attribution array[n]=v (or map[n]=v) is performed.
        // </summary>
        /// <summary>
        /// 对栈顶的数组中的指定位置n的元素赋值。
        /// </summary>
        SETITEM = 0xC4,
        // <summary>
        //用作引用類型  en: A value n is taken from top of main stack. A zero-filled array type with size n is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 在栈顶新建一个大小为n的Array，其元素全部为0。
        /// </summary>
        NEWARRAY = 0xC5,
        // <summary>
        //用作值類型 en: A value n is taken from top of main stack. A zero-filled struct type with size n is put on top of the main stack.
        // </summary>
        /// <summary>
        /// 在栈顶新建一个大小为n的Struct，其元素全部为0.
        /// </summary>
        NEWSTRUCT = 0xC6,
        // <summary>
        // A Map is created and put on top of the main stack.
        // </summary>
        /// <summary>
        /// 在栈顶新建一个Map。
        /// </summary>
        NEWMAP = 0xC7,
        // <summary>
        // The item on top of main stack is removed and appended to the second item on top of the main stack.
        // </summary>
        /// <summary>
        /// 向Array中添加一个新项。
        /// </summary>
        APPEND = 0xC8,
        // <summary>
        // An array is removed from the top of the main stack and its elements are reversed.
        // </summary>
        /// <summary>
        /// 将Array元素倒序排列。
        /// </summary>
        REVERSE = 0xC9,
        // <summary>
        // An input index n (or key) and an array (or map) are removed from the top of the main stack. Element array[n] (or map[n]) is removed.
        // </summary>
        /// <summary>
        /// 从Array或Map中移除指定位置n的元素。
        /// </summary>
        REMOVE = 0xCA,
        // <summary>
        // An input index n (or key) and an array (or map) are removed from the top of the main stack. Puts True on top of main stack if array[n] (or map[n]) exist, and False otherwise.
        // </summary>
        /// <summary>
        /// 判断Array或Map中是否包含Key指定元素。
        /// </summary>
        HASKEY = 0xCB,
        // <summary>
        // A map is taken from top of the main stack. The keys of this map are put on top of the main stack.
        // </summary>
        /// <summary>
        /// 获取Map的所有键，并放入新的Array中。
        /// </summary>
        KEYS = 0xCC,
        // <summary>
        // A map is taken from top of the main stack. The values of this map are put on top of the main stack.
        // </summary>
        /// <summary>
        /// 获取Map所有值，并放入新的Array中。
        /// </summary>
        VALUES = 0xCD,


        // Stack isolation
        /// <summary>
        /// 调用一个新的运行上下文，其脚本为当前上下文的脚本，
        /// pcount指定参数个数，rvcount指定结果个数。
        /// 执行指令跳转至新的运行上下文。
        /// </summary>
        CALL_I = 0xE0,
        /// <summary>
        /// 调用一个新的运行上下文，其脚本由指令后20位Hash指定。
        /// pcount指定参数个数，rvcount指定结果个数。
        /// 执行指令跳转至新的运行上下文。
        /// </summary>
        CALL_E = 0xE1,
        /// <summary>
        /// 调用一个新的运行上下文，其脚本由计算栈栈顶的Hash指定。
        /// pcount指定参数个数，rvcount指定结果个数。
        /// 执行指令跳转至新的运行上下文。
        /// </summary>
        CALL_ED = 0xE2,
        /// <summary>
        /// CALL_E的尾调用形式。
        /// </summary>
        CALL_ET = 0xE3,
        /// <summary>
        /// CALL_ED的尾调用形式。
        /// </summary>
        CALL_EDT = 0xE4,


        // Exceptions
        // <summary>
        // Halts the execution of the vm by setting VMState.FAULT.
        // </summary>
        /// <summary>
        /// 将虚拟机状态置为FAULT。
        /// </summary>
        THROW = 0xF0,
        // <summary>
        // Removes top stack item n, and halts the execution of the vm by setting VMState.FAULT only if n is False.
        // </summary>
        /// <summary>
        /// 从计算栈栈顶读取一个布尔值，如果为False，则将虚拟机状态置为FAULT。
        /// </summary>
        THROWIFNOT = 0xF1
    }
}
