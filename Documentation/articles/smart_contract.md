# 限制条件

关于智能合约的基本类型限制可以参考: http://docs.neo.org/zh-cn/sc/quickstart/limitation.html

同时出于安全因素考虑，防止不同节点从外部获取不相同的数据而导致网络出现问题，NEO暂时不支持包括访问 Internet上那些不能确定可靠性的信息来源,访问其他区块链数据等功能。

# 价格机制

智能合约的每行指令都需要收取一定的Gas作为手续费，GUI在智能合约部署和调用的试运行过程中，会将智能合约编译出来的字节码在本地虚拟机中执行一次，并计算每条指令所需要的手续费。如果合约执行通过，则会显示需要的Gas总和。

每个智能合约在每次执行过程中有10 GAS 的免费额度，无论是开发者部署还是用户调用，因此，单次执行费用在 10 GAS 以下的智能合约是不需要支付手续费的。当单次执行费用超过10 GAS，会减免10 GAS 的手续费。

所有支付的智能合约手续费将作为系统手续费，并在用户提取Gas时按比例重新分配给所有 NEO 的持有人。

### 各指令的手续费标准：

| 指令                           | 手续费(Gas) |
| -------------------------------- | ----------- |
| 所有PUSH指令（常数指令） | 0           |
| OpCode.NOP                       | 0           |
| OpCode.APPCALL                   | 0.01        |
| OpCode.TAILCAL                   | 0.01        |
| OpCode.SHA1                      | 0.01        |
| OpCode.SHA256                    | 0.01        |
| OpCode.HASH160                   | 0.02        |
| OpCode.HASH256                   | 0.02        |
| OpCode.CHECKSIG                  | 0.1         |
| OpCode.CHECKMULTISIG（每个公钥） | 0.1         |
| 其它（每行OpCode）         | 0.001       |

### 系统调用的手续费标准：

| 系统调用                                | 手续费 [Gas] |
| ------------------------------------------- | ------------ |
| Runtime.CheckWitness                        | 0.2          |
| Blockchain.GetHeader                        | 0.1          |
| Blockchain.GetBlock                         | 0.2          |
| Blockchain.GetTransaction                   | 0.1          |
| Blockchain.GetTransactionHeight             | 0.1          |
| Blockchain.GetAccount                       | 0.1          |
| Blockchain.GetValidators                    | 0.2          |
| Blockchain.GetAsset                         | 0.1          |
| Blockchain.GetContract                      | 0.1          |
| Transaction.GetReferences                   | 0.2          |
| Transaction.GetUnspentCoins                 | 0.2          |
| Transaction.GetWitnesses                    | 0.2          |
| Witness.GetVerificationScript               | 0.1          |
| Account.IsStandard                          | 0.1          |
| Asset.Create（系统资产）              | 5000         |
| Asset.Renew（系统资产，每200万个块，约1年） | 5000         |
| Contract.Create*                            | 100~1000     |
| Contract.Migrate*                           | 100~1000     |
| Storage.Get                                 | 0.1          |
| Storage.Put 、Storage.PutEx [每 KB]       | 1            |
| Storage.Delete                              | 0.1          |
| 其它（每行OpCode）                    | 0.001        |

\* 创建智能合约与迁移智能合约目前是根据合约所需功能进行收费。其中基础的费用为 100GAS，需要存储区 +400GAS，需要动态调用 +500GAS。

* 如果部署合约需要存储区、动态调用等时，请务必勾选对应选项，如果智能合约发布后由于未勾选而导致合约不能正常执行，后果由用户自行承担。未来会考虑添加相关检测机制。

# 触发器

触发器是触发智能合约执行的机制，在 NEO 智能合约中，有 4 种触发器，`Verification`, `Application`， `VerificationR`, `ApplicationR`。其中`VerificationR`和`ApplicationR`为2.9版新增的触发器。

一个实现智能合约的区块链应该为其上运行的智能合约提供多种触发器，便其在不同的上下文中起作用。

Verification和Application使智能合约能够验证交易和改变区块链的状态。

VerificationR, ApplicationR则使智能合约能够拒绝一笔转账或者在接收到一笔转账时改变区块链的状态。

相关介绍可以参考：http://docs.neo.org/zh-cn/sc/trigger.html

### VerificationR

验证触发器R的目的在于将该合约作为验证函数进行调用，因为它被指定为交易输出的目标。验证函数不接受参数，并且应返回有效的布尔值，标志着交易的有效性。

如果智能合约被验证触发器R触发了，则调用智能合约入口点:

`main("receiving", new object[0]);`

`receiving`函数应具有以下编程接口:

`public bool receiving()`

当智能合约从转账中收到一笔资产时，`receiving`函数将会自动被调用。

### ApplicationR

应用触发器R指明了当智能合约被调用时的默认函数`received`，因为它被指定为交易输出的目标。`received`函数不接受参数，对区块链的状态进行更改，并返回任意类型的返回值。

如果智能合约被应用触发器R触发了，则调用智能合约入口点:

`main("received", new object[0]);`

`received`函数应具有以下编程接口:

`public byte[] received()`

当智能合约从转账中收到一笔资产时，`receiving`函数将会自动被调用。


# 互操作服务

NeoContract 的 API 扩展了智能合约的功能，使其可以访问区块链账本数据、操作持久化存储区、访问执行环境等。它是NEO虚拟机（NeoVM）互操作服务层的一部分。


## Runtime：运行时相关的 API

### System.Runtime.GetTrigger

| old api：  | "Neo.Runtime.GetTrigger",                                                                            |
|------------|------------------------------------------------------------------------------------------------------|
| 绑定函数： | Runtime_GetTrigger                                                                                   |
| 功能描述： | 获得该智能合约的触发条件（应用合约 or 鉴权合约）                                                     |
| C\#函数：  | TriggerType Trigger;                                                                                 |
| 说明：     | 其中Verification = 0x00, Application = 0x10。关于触发器详见上一节 |

### System.Runtime.CheckWitness

| old api：  | "Neo.Runtime.CheckWitness", "AntShares.Runtime.CheckWitness"                                                                                                                                                                                                                                                                     |
|------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Runtime_CheckWitness                                                                                                                                                                                                                                                                                                             |
| 功能描述： | 验证调用该智能合约的交易/区块是否验证过所需的脚本散列。                                                                                                                                                                                                                                                                          |
| C\#函数：  | bool CheckWitness(byte[] hashOrPubkey);                                                                                                                                                                                                                                                                                          |
| 说明：     | 查找该笔交易需要验证的脚本Hash是否包含该Hash。

### System.Runtime.Notify

| old api：  | "Neo.Runtime.Notify", "AntShares.Runtime.Notify"                                                               |
|------------|----------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Runtime_Notify                                                                                                 |
| 功能描述： | 在智能合约中向执行该智能合约的客户端发送通知                                                                   |
| C\#函数：  | void Notify(params object[] state)                                                                             |
| 说明：     | 从计算栈中获取状态信息，根据状态信息新建一个notification,</br>调用Notify事件，并将notification加入notifications中。 |

### System.Runtime.Log

| old api：  | "Neo.Runtime.Log"， "AntShares.Runtime.Log"       |
|------------|---------------------------------------------------|
| 绑定函数： | Runtime_Log                                       |
| 功能描述： | 在智能合约中向执行该智能合约的客户端发送日志      |
| C\#函数：  | void Log(string message)                          |
| 说明：     | 从计算栈中获取消息，调用log事件向客户端发送日志。 |

### System.Runtime.GetTime

| old api：  | "Neo.Runtime.GetTime"                                                                            |
|------------|--------------------------------------------------------------------------------------------------|
| 绑定函数： | Runtime_GetTime                                                                                  |
| 功能描述： | 获取当前时间                                                                                     |
| C\#函数：  | uint Time                                                                                        |
| 说明：     | 根据当前区块高度获取区块头信息。当前时间为区块头时间戳加上每个区块出块的时间 |

### System.Runtime.Serialize

| old api：  | "Neo.Runtime.Serialize"                                                                   |
|------------|-------------------------------------------------------------------------------------------|
| 绑定函数： | Runtime_Serialize                                                                         |
| 功能描述： | 对数据流进行序列化                                                                        |
| C\#函数：  | byte[] Serialize(this object source)                                                      |


### System.Runtime.Deserialize

| old api：  | "Neo.Runtime.Deserialize"                                             |
|------------|-----------------------------------------------------------------------|
| 绑定函数： | Runtime_Deserialize                                                   |
| 功能描述： | 将数据反序列化                                                        |
| C\#函数：  | object Deserialize(this byte[] source)                                |

## Blockchain：区块链查询数据的 API


### System.Blockchain.GetHeight

| old api：  | "Neo.Blockchain.GetHeight"， "AntShares.Blockchain.GetHeight"                                         |
|------------|-------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetHeight                                                                                  |
| 功能描述： | 获得当前区块高度                                                                                      |
| C\#函数：  | uint GetHeight()                                                                                      |

### System.Blockchain. GetHeader

| old api：  | " Neo.Blockchain.GetHeader"， " AntShares.Blockchain.GetHeader"                                                                                                                                                                    |
|------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetHeader                                                                                                                                                                                                               |
| 功能描述： | 通过区块高度或区块 Hash，查找区块头                                                                                                                                                                                                |
| C\#函数：  | Header GetHeader(uint height); </br> Header GetHeader(byte[] hash)                                                                                                                                                                       |

### System.Blockchain.GetBlock

| old api：  | "Neo.Blockchain.GetBlock"， "AntShares.Blockchain.GetBlock"                                                                                                                                                                      |
|------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetBlock                                                                                                                                                                                                              |
| 功能描述： | 通过区块高度或区块 Hash，查找区块                                                                                                                                                                                                |
| C\#函数：  | Block GetBlock(uint height);</br> Block GetBlock(byte[] hash)                                                                                                                                                                         |

### System.Blockchain.GetTransaction

| old api：  | "Neo.Blockchain.GetTransaction"， "AntShares.Blockchain.GetTransaction"                                  |
|------------|----------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetTransaction                                                                                |
| 功能描述： | 通过交易 ID 查找交易                                                                                     |
| C\#函数：  | Transaction GetTransaction(byte[] hash)                                                                  |

### System.Blockchain.GetTransactionHeight

| old api：  | "Neo.Blockchain.GetTransactionHeight"                                                                              |
|------------|--------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetTransactionHeight                                                                                    |
| 功能描述： | 通过交易hash查找交易高度                                                                                           |

### System.Blockchain.GetContract

| old api：  | "Neo.Blockchain.GetContract", "AntShares.Blockchain.GetContract"                        |
|------------|-----------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetContract                                                                  |
| 功能描述： | 根据合约散列获取合约内容                                                                |
| C\#函数：  | Contract GetContract(byte[] script_hash)                                                |

### Neo.Blockchain.GetAccount

| old api：  | "AntShares.Blockchain.GetAccount"                                                                                                                                         |
|------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetAccount                                                                                                                                                     |
| 功能描述： | 根据合约脚本的散列来获得一个账户                                                                                                                                          |
| C\#函数：  | Account GetAccount(byte[] script_hash)                                                                                                                                    |
| 说明：     | 如果script_hash对应的账户不存在，则由Hash新建一个AccountState account并加入Accounts,</br>最后返回account。 |

### Neo.Blockchain.GetValidators

| old api：  | "AntShares.Blockchain.GetValidators"                                                                      |
|------------|-----------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetValidators                                                                                  |
| 功能描述： | 获得共识人的公钥                                                                                          |
| C\#函数：  | byte[][] GetValidators()                                                                                  |

### Neo.Blockchain.GetAsset

| old api：  | "AntShares.Blockchain.GetAsset"                                                                                           |
|------------|---------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Blockchain_GetAsset                                                                                                       |
| 功能描述： | 根据资产 ID 查找资产                                                                                                      |
| C\#函数：  | Asset GetAsset(byte[] asset_id)                                                                                           |

## Header：区块头相关API

### System.Header.GetIndex

| old api：  | "Neo.Header.GetIndex"                                                                 |
|------------|---------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetIndex                                                                       |
| 功能描述： | 获得该区块的高度                                                                      |
| C\#函数：  | uint Index                                                                            |

### System.Header.GetHash

| old api：  | "Neo.Header.GetHash"， "AntShares.Header.GetHash"                                    |
|------------|--------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetHash                                                                       |
| 功能描述： | 获得该区块的散列                                                                     |
| C\#函数：  | byte[] Hash                                                                          |

### System.Header.GetPrevHash

| old api：  | "Neo.Header.GetPrevHash"， "AntShares.Header.GetPrevHash"                                |
|------------|------------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetPrevHash                                                                       |
| 功能描述： | 获得前一个区块的散列                                                                     |
| C\#函数：  | byte[] PrevHash                                                                          |

### System.Header.GetTimestamp

| old api：  | "Neo.Header.GetTimestamp"，"AntShares.Header.GetTimestamp"                                |
|------------|-------------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetTimestamp                                                                       |
| 功能描述： | 获得区块的时间戳                                                                          |
| C\#函数：  | uint Timestamp                                                                            |

### Neo.Header.GetVersion

| old api：  | "AntShares.Header.GetVersion"                                                           |
|------------|-----------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetVersion                                                                       |
| 功能描述： | 获得区块版本号                                                                          |
| C\#函数：  | uint Version                                                                            |

### Neo.Header.GetMerkleRoot

| old api：  | "AntShares.Header.GetMerkleRoot"                                                           |
|------------|--------------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetMerkleRoot                                                                       |
| 功能描述： | 获得该区块中所有交易的 Merkle Tree 的根                                                    |
| C\#函数：  | byte[] MerkleRoot                                                                          |

### Neo.Header.GetConsensusData

| old api：  | "AntShares.Header.GetConsensusData"                                                           |
|------------|-----------------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetConsensusData                                                                       |
| 功能描述： | 获得该区块的共识数据（共识节点生成的伪随机数）                                                |

### Neo.Header.GetNextConsensus

| old api：  | "AntShares.Header.GetNextConsensus"                                                           |
|------------|-----------------------------------------------------------------------------------------------|
| 绑定函数： | Header_GetNextConsensus                                                                       |
| 功能描述： | 获得下一个记账合约的散列值                                                                    |
| C\#函数：  | ulong ConsensusData                                                                           |

## Block：区块相关API

### System.Block.GetTransactionCount

| old api：  | "Neo.Block.GetTransactionCount"，"AntShares.Block.GetTransactionCount"                        |
|------------|-----------------------------------------------------------------------------------------------|
| 绑定函数： | Block_GetTransactionCount                                                                     |
| 功能描述： | 获得当前区块中交易的数量                                                                      |
| C\#函数：  | int GetTransactionCount()                                                                     |

### System.Block.GetTransactions

| old api：  | "Neo.Block.GetTransactions"                                                                  |
|------------|----------------------------------------------------------------------------------------------|
| 绑定函数： | Block_GetTransactions                                                                        |
| 功能描述： | 获得当前区块中所有的交易                                                                     |
| C\#函数：  | Transaction[] GetTransactions()                                                              |

### System.Block.GetTransaction

| old api：  | "Neo.Block.GetTransaction"， "AntShares.Block.GetTransaction"                                                                     |
|------------|-----------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Block_GetTransaction                                                                                                              |
| 功能描述： | 获得当前区块中指定的交易                                                                                                          |
| C\#函数：  | Transaction GetTransaction(int index)                                                                                             |
| 说明：     | index为交易在区块中的索引。 |

## Transaction：交易相关API

### System.Transaction.GetHash

| old api：  | "Neo.Transaction.GetHash"，"AntShares.Transaction.GetHash"                     |
|------------|--------------------------------------------------------------------------------|
| 绑定函数： | Transaction_GetHash                                                            |
| 功能描述： | 获得当前交易的 Hash                                                            |
| C\#函数：  | byte[] Hash                                                                    |

### Neo.Transaction.GetType

| old api：  | "AntShares.Transaction.GetType"                                                 |
|------------|---------------------------------------------------------------------------------|
| 绑定函数： | Transaction_GetType                                                             |
| 功能描述： | 获得当前交易的类型                                                              |
| C\#函数：  | byte Type                                                                       |

### Neo.Transaction.GetAttributes

| old api：  | "AntShares.Transaction.GetAttributes"                                                      |
|------------|--------------------------------------------------------------------------------------------|
| 绑定函数： | Transaction_GetAttributes                                                                  |
| 功能描述： | 查询当前交易的所有属性                                                                     |
| C\#函数：  | TransactionAttribute[] GetAttributes()                                                     |

### Neo.Transaction.GetInputs

| old api：  | "AntShares.Transaction.GetInputs"                                                      |
|------------|----------------------------------------------------------------------------------------|
| 绑定函数： | Transaction_GetInputs                                                                  |
| 功能描述： | 查询当前交易的所有交易输入                                                             |
| C\#函数：  | TransactionInput[] GetInputs()                                                         |

### Neo.Transaction.GetOutputs

| old api：  | "AntShares.Transaction.GetOutputs"                                                      |
|------------|-----------------------------------------------------------------------------------------|
| 绑定函数： | Transaction_GetOutputs                                                                  |
| 功能描述： | 查询当前交易的所有交易输出                                                              |
| C\#函数：  | TransactionOutput[] GetOutputs()                                                        |

### Neo.Transaction.GetReferences

| old api：  | "AntShares.Transaction.GetReferences"                                                            |
|------------|--------------------------------------------------------------------------------------------------|
| 绑定函数： | Transaction_GetReferences                                                                        |
| 功能描述： | 查询当前交易的所有输入所引用的交易输出                                                           |
| C\#函数：  | TransactionOutput[] GetReferences()                                                              |

### Neo.Transaction.GetUnspentCoins

| 绑定函数： | Transaction_GetUnspentCoins                                                                          |
|------------|------------------------------------------------------------------------------------------------------|
| 功能描述： | 查询当前交易的所有未花费输出                                                                         |
| C\#函数：  | TransactionOutput[] GetUnspentCoins()                                                                |

## Storage：存储相关API

### System.Storage.GetContext

| old api：  | "Neo.Storage.GetContext"， "AntShares.Storage.GetContext"                                                 |
|------------|-----------------------------------------------------------------------------------------------------------|
| 绑定函数： | Storage_GetContext                                                                                        |
| 功能描述： | 获取当前存储区上下文                                                                                      |
| C\#函数：  | StorageContext CurrentContext                                                                             |
| 说明：     |获取的StorageContext中IsReadOnly = false|

### System.Storage.GetReadOnlyContext

| old api：  | "Neo.Storage.GetReadOnlyContext"                                                                         |
|------------|----------------------------------------------------------------------------------------------------------|
| 绑定函数： | Storage_GetReadOnlyContext                                                                               |
| 功能描述： | 获取当前存储区上下文                                                                                     |
| 说明：     | 获取的StorageContext中IsReadOnly = true|

### System.Storage.Get

| old api：  | "Neo.Storage.Get"， "AntShares.Storage.Get"                                                                                                                                                                                       |
|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Storage_Get                                                                                                                                                                                                                       |
| 功能描述： | 查询操作，在持久化存储区中通过 key 查询对应的 value                                                                                                                                                                               |
| C\#函数：  | byte[] Get(StorageContext context, byte[] key); </br> byte[] Get(StorageContext context, string key);                                                                                                                                   |
| 说明：     | 如果结果不存在，则返回空字节数组。 |

### System.Storage.Put

| old api：  | "Neo.Storage.Put"，"AntShares.Storage.Put"                                                                                                                                                                                                                                                                                                                                      |
|------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Storage_Put                                                                                                                                                                                                                                                                                                                                                                     |
| 功能描述： | 插入操作，以 key-value 的形式向持久化存储区中插入数据                                                                                                                                                                                                                                                                                                                           |
| C\#函数：  | void Put(StorageContext context, byte[] key, byte[] value); </br>void Put(StorageContext context, byte[] key, BigInteger value); </br>void Put(StorageContext context, byte[] key, string value);</br> void Put(StorageContext context, string key, byte[] value); </br>void Put(StorageContext context, string key, BigInteger value); </br>void Put(StorageContext context, string key, string value); |
| 说明：     | 首先判断context是否为只读，为只读时返回false;</br>当context对应的合约不存在，或者合约没有存储区时，返回false;</br> key的长度不能大于1024。 |

### System.Storage.Delete

| old api：  | "Neo.Storage.Delete"， "AntShares.Storage.Delete"                                                                                                                                                                                     |
|------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Storage_Delete                                                                                                                                                                                                                        |
| 功能描述： | 删除操作，在持久化存储区中通过 key 删除对应的 value                                                                                                                                                                                   |
| C\#函数：  | void Delete(StorageContext context, byte[] key); void Delete(StorageContext context, string key);                                                                                                                                     |
| 说明：     |首先判断context是否为只读，为只读时返回false;</br>当context对应的合约不存在，或者合约没有存储区时，返回false。|

### Neo.Storage.Find

| 绑定函数： | Storage_Find                                                                                                                                                                                                       |
|------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 功能描述： | 在当前存储器上下文中查找指定前缀的内容                                                                                                                                                                             |
| C\#函数：  | Iterator < byte[], byte[] \> Find(StorageContext context, byte[] prefix); </br> Iterator < string, byte[] \> Find(StorageContext context, string prefix);                                                                    |

## StorageContext：存储上下文相关API

System.StorageContext.AsReadOnly
--------------------------------

| old api：  | "Neo.StorageContext.AsReadOnly"                                                                                                                           |
|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | StorageContext_AsReadOnly                                                                                                                                 |
| 功能描述： | 将当前存储区上下文设为只读                                                                                                                                |
| 说明：     | 将StorageContext的IsReadOnly=true。 |

## InvocationTransaction：合约交易相关API

### Neo.InvocationTransaction.GetScript

| 绑定函数： | InvocationTransaction_GetScript                                                            |
|------------|--------------------------------------------------------------------------------------------|
| 功能描述： | 查询当前合约交易对应的脚本                                                                 |
| C\#函数：  | byte[] Script                                                                              |

## Attribute：交易特性相关API

### Neo.Attribute.GetUsage

| old api：  | "AntShares.Attribute.GetUsage"                                                                              |
|------------|-------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Attribute_GetUsage                                                                                          |
| 功能描述： | 获得该交易特性中的用途                                                                                      |
| C\#函数：  | byte Usage                                                                                                  |

### Neo.Attribute.GetData

| old api：  | "AntShares.Attribute.GetData"                                                               |
|------------|---------------------------------------------------------------------------------------------|
| 绑定函数： | Attribute_GetData                                                                           |
| 功能描述： | 获得该交易特性中用途之外的额外数据                                                          |
| C\#函数：  | byte[] Data                                                                                 |

## Input：交易输入相关API

### Neo.Input.GetHash

| old api：  | "AntShares.Input.GetHash"                                                                            |
|------------|------------------------------------------------------------------------------------------------------|
| 绑定函数： | Input_GetHash                                                                                        |
| 功能描述： | 所引用的交易的交易散列                                                                               |
| C\#函数：  | byte[] PrevHash                                                                                      |

### Neo.Input.GetIndex

| old api：  | "AntShares.Input.GetIndex"                                                                                  |
|------------|-------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Input_GetIndex                                                                                              |
| 功能描述： | 所引用的交易输出在其全部交易输出列表中的索引                                                                |
| C\#函数：  | ushort PrevIndex                                                                                            |

## Output：交易输出相关API

### Neo.Output.GetAssetId

| old api：  | "AntShares.Output.GetAssetId"                                                                   |
|------------|-------------------------------------------------------------------------------------------------|
| 绑定函数： | Output_GetAssetId                                                                               |
| 功能描述： | 获得交易输出的资产 ID                                                                           |
| C\#函数：  | byte[] AssetId                                                                                  |

### Neo.Output.GetValue

| old api：  | "AntShares.Output.GetValue"                                                                   |
|------------|-----------------------------------------------------------------------------------------------|
| 绑定函数： | Output_GetValue                                                                               |
| 功能描述： | 获得交易输出金额                                                                              |
| C\#函数：  | long Value                                                                                    |

### Neo.Output.GetScriptHash

| old api：  | "AntShares.Output.GetScriptHash"                                                                   |
|------------|----------------------------------------------------------------------------------------------------|
| 绑定函数： | Output_GetScriptHash                                                                               |
| 功能描述： | 获得交易输出的脚本散列                                                                             |
| C\#函数：  | byte[] ScriptHash                                                                                  |

## Account：账户相关API

### Neo.Account.GetScriptHash

| old api：  | "AntShares.Account.GetScriptHash"                                                               |
|------------|-------------------------------------------------------------------------------------------------|
| 绑定函数： | Account_GetScriptHash                                                                           |
| 功能描述： | 获得该合约账户的脚本散列                                                                        |
| C\#函数：  | byte[] ScriptHash;                                                                              |

### Neo.Account.GetVotes

| old api：  | "AntShares.Account.GetVotes"                                                                                               |
|------------|----------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Account_GetVotes                                                                                                           |
| 功能描述： | 获得该合约账户投给其它人的的投票信息                                                                                       |
| C\#函数：  | byte[][] Votes;                                                                                                            |

### Neo.Account.GetBalance

| old api：  | "AntShares.Account.GetBalance"                                                                                                                                  |
|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Account_GetBalance                                                                                                                                              |
| 功能描述： | 通过资产 ID 获得该账户中这种资产的余额                                                                                                                          |
| C\#函数：  | long GetBalance(byte[] asset_id);                                                                                                                               |

## Asset：资产相关API

### Neo.Asset.GetAssetId

| old api：  | "AntShares.Asset.GetAssetId"                                                           |
|------------|----------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetAssetId                                                                       |
| 功能描述： | 获得该资产的 ID                                                                        |
| C\#函数：  | byte[] AssetId                                                                         |

### Neo.Asset.GetAssetType

| old api：  | "AntShares.Asset.GetAssetType"                                                                        |
|------------|-------------------------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetAssetType                                                                                    |
| 功能描述： | 获得该资产的类别                                                                                      |
| C\#函数：  | byte AssetType                                                                                        |

### Neo.Asset.GetAmount
-------------------

| old api：  | "AntShares.Asset.GetAmount"                                                           |
|------------|---------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetAmount                                                                       |
| 功能描述： | 获得该资产的总量                                                                      |
| C\#函数：  | long Amount                                                                           |

### Neo.Asset.GetAvailable

| old api：  | "AntShares.Asset.GetAvailable"                                                           |
|------------|------------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetAvailable                                                                       |
| 功能描述： | 获得该资产的已经发行出去的数量                                                           |
| C\#函数：  | long Available                                                                           |

### Neo.Asset.GetPrecision

| old api：  | "AntShares.Asset.GetPrecision"                                                                        |
|------------|-------------------------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetPrecision                                                                                    |
| 功能描述： | 获得该资产的精度（最小分割数量），单位为小数点之后的位数                                              |
| C\#函数：  | byte Precision                                                                                        |

### Neo.Asset.GetOwner

| old api：  | "AntShares.Asset.GetOwner"                                                                           |
|------------|------------------------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetOwner                                                                                       |
| 功能描述： | 获得该资产的所有人（公钥）                                                                           |
| C\#函数：  | byte[] Owner                                                                                         |

### Neo.Asset.GetAdmin

| old api：  | "AntShares.Asset.GetAdmin"                                                           |
|------------|--------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetAdmin                                                                       |
| 功能描述： | 获得该资产的管理员（合约地址），有权对资产的属性（如总量，名称等）进行修改           |
| C\#函数：  | byte[] Admin                                                                         |

### Neo.Asset.GetIssuer

| old api：  | "AntShares.Asset.GetIssuer"                                                            |
|------------|----------------------------------------------------------------------------------------|
| 绑定函数： | Asset_GetIssuer                                                                        |
| 功能描述： | 获得该资产的发行人（合约地址），有权进行资产的发行                                     |
| C\#函数：  | byte[] Issuer                                                                          |

### Neo.Asset.Create
----------------

| old api：  | "AntShares.Asset.Create"                                                                                                                                                             |
|------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Asset_Create                                                                                                                                                                         |
| 功能描述： | 注册一种资产                                                                                                                                                                         |
| C\#函数：  | Asset Create(byte asset_type, string name, long amount, </br> byte precision, byte[] owner, byte[] admin, byte[] issuer);                                                                  |

### Neo.Asset.Renew

| old api：  | "AntShares.Asset.Renew"                                                                                                                                                                                                                        |
|------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Asset_Renew                                                                                                                                                                                                                                    |
| 功能描述： | 为资产续费                                                                                                                                                                                                                                     |
| C\#函数：  | uint Renew(byte years);                                                                                                                                                                                                                        |
| 说明：     |续费按区块数量计算，1year为200万个区块，</br>即新的到期时间为到期时间加上2000000\*years块。 |

## Contract：合约相关API

### Neo.Contract.GetScript

| old api：  | "AntShares.Contract.GetScript"                                                                     |
|------------|----------------------------------------------------------------------------------------------------|
| 绑定函数： | Contract_GetScript                                                                                 |
| 功能描述： | 获得该合约的脚本                                                                                   |
| C\#函数：  | byte[] Script                                                                                      |

### Neo.Contract.IsPayable

| old api：  | "Neo.Contract.IsPayable"                                                                            |
|------------|-----------------------------------------------------------------------------------------------------|
| 绑定函数： | Contract_IsPayable                                                                                  |
| 功能描述： | 获取该合约是否可被支付                                                                                |
| C\#函数：  | bool IsPayable                                                                                      |

### System.Contract.GetStorageContext

| old api：  | "Neo.Contract.GetStorageContext", "AntShares.Contract.GetStorageContext"                                                                                                                                                |
|------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Contract_GetStorageContext                                                                                                                                                                                              |
| 功能描述： | 获得合约的存储上下文                                                                                                                                                                                                    |
| C\#函数：  | StorageContext StorageContext                                                                                                                                                                                           |
| 说明：     | StorageContext 的 IsReadOnly = false |

### System.Contract.Destroy

| old api：  | "Neo.Contract.Destroy", "AntShares.Contract.Destroy"                                                                                                                                                    |
|------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Contract_Destroy                                                                                                                                                                                        |
| 功能描述： | 销毁合约                                                                                                                                                                                                |
| C\#函数：  | void Destroy();                                                                                                                                                                                         |
| 说明：     | 销毁合约，如果合约使用了存储区，则同时将合约的存储区删除。 |

### Neo.Contract.Create

| old api：  | "AntShares.Contract.Create"                                                                                                                                                                           |
|------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Contract_Create                                                                                                                                                                                       |
| 功能描述： | 发布智能合约                                                                                                                                                                                          |
| C\#函数：  | Contract Create(byte[] script, byte[] parameter_list, byte return_type, </br> ContractPropertyState contract_property_state, string name, </br> string version, string author, string email, string description); |

### Neo.Contract.Migrate

| old api：  | "AntShares.Contract.Migrate"                                                                                                                                                                                                      |
|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 绑定函数： | Contract_Migrate                                                                                                                                                                                                                  |
| 功能描述： | 迁移 / 更新智能合约                                                                                                                                                                                                               |
| C\#函数：  | Contract Migrate(byte[] script, byte[] parameter_list, byte return_type, </br> ContractPropertyState contract_property_state, string name, </br> string version, string author, string email, string description);                            |
| 说明：     | 详见本章“升级” 部分|

## Enumerator：迭代器相关API

### Neo.Enumerator.Create

| 绑定函数： | Enumerator_Create                                                                           |
|------------|---------------------------------------------------------------------------------------------|
| 功能描述： | 构建一个迭代器                                                                              |

### Neo.Enumerator.Next

| Aliases：  | "Neo.Iterator.Next"                                                                            |
|------------|------------------------------------------------------------------------------------------------|
| 绑定函数： | Enumerator_Next                                                                                |
| 功能描述： | 获取迭代器的下一个元素                                                                         |
| C\#函数：  | bool Next();                                                                                   |

### Neo.Enumerator.Value

| Aliases：  | "Neo.Iterator.Value"                                                                            |
|------------|-------------------------------------------------------------------------------------------------|
| 绑定函数： | Enumerator_Value                                                                                |
| 功能描述： | 获取迭代器的值                                                                                  |
| C\#函数：  | TValue Value                                                                                    |

### Neo.Enumerator.Concat
---------------------

| 绑定函数： | Enumerator_Concat                                                                                                                                  |
|------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| 功能描述： | 将两个迭代器连接起来                                                                                                                               |

### Neo.Iterator.Create
-------------------

| 绑定函数： | Iterator_Create                                                                                  |
|------------|--------------------------------------------------------------------------------------------------|
| 功能描述： | 构建一个迭代器IIterator                                                                          |

### Neo.Iterator.Key
----------------

| 绑定函数： | Iterator_Key                                                                            |
|------------|-----------------------------------------------------------------------------------------|
| 功能描述： | 获取迭代器的Key值                                                                       |
| C\#函数：  | TKey Key                                                                                |

### Neo.Iterator.Keys
-----------------

| 绑定函数： | Iterator_Keys                                                                                                  |
|------------|----------------------------------------------------------------------------------------------------------------|
| 功能描述： | 获取迭代器的Keys值                                                                                                            |

### Neo.Iterator.Values
-------------------

| 绑定函数： | Iterator_Values                                                                                                  |
|------------|------------------------------------------------------------------------------------------------------------------|
| 功能描述： | 获取迭代器的Values值                                                                                                                 |
| C\#函数：  | TValue Value                                                                                                     |

## ExecutionEngine:智能合约执行引擎相关API

### System.ExecutionEngine.GetScriptContainer

| 绑定函数： | GetScriptContainer                         |
|------------|--------------------------------------------|
| 功能描述： | 获得该智能合约的脚本容器（最开始的触发者） |
| C\#函数：  | IScriptContainer ScriptContainer           |
| 说明：     | 获取engine.ScriptContainer    |

### System.ExecutionEngine.GetExecutingScriptHash

| 绑定函数： | GetExecutingScriptHash                           |
|------------|--------------------------------------------------|
| 功能描述： | 获得该智能合约执行的脚本散列                     |
| C\#函数：  | byte[] ExecutingScriptHash                       |
| 说明：     | 获取engine.CurrentContext.ScriptHash |

### System.ExecutionEngine.GetCallingScriptHash

| 绑定函数： | GetScriptContainer                               |
|------------|--------------------------------------------------|
| 功能描述： | 获得该智能合约的调用者的脚本散列                 |
| C\#函数：  | byte[] CallingScriptHash                         |
| 说明：     | 获取engine.CallingContext.ScriptHash |

### System.ExecutionEngine.GetEntryScriptHash

| 绑定函数： | GetEntryScriptHash                                   |
|------------|------------------------------------------------------|
| 功能描述： | 获得该智能合约的入口点（合约调用链的起点）的脚本散列 |
| C\#函数：  | byte[] EntryScriptHash                               |
| 说明：     | 获取engine.EntryContext.ScriptHash       |


# NEP-5

NEP5协议是NEO补充协议中的第5号协议。其目的是为neo建立标准的token化智能合约通用交互机 制。NEP5资产与UTXO不同，它没有采用UTXO模型，而是在合约存储区内记账，通过对存储区内不同账户 hash所记录余额数值的变化，完成交易。

​参照NEP5协议的要求，在NEP5资产智能合约时必需实现以下方法：

### totalSupply
    public static BigInteger totalSupply()

​Returns 部署在系统内该token的总数。 

### name
    public static string name()

​Returns token的名称. e.g. "MyToken"。
该方法每次被调用时必需返回一样的值。

### symbol

    public static string symbol()

​Returns 合约所管理的token的短字符串符号 . e.g. "MYT"。
该符号需要应该比较短小 (建议3-8个字符),  没有空白字符或换行符 ，并限制为大写拉丁字母 (26个英文字符)。 
该方法每次被调用时必需返回一样的值。

### decimals

    public static byte decimals()

​Returns token使用的小数位数 - e.g. 8，意味着把token数量除以100,000,000来获得它的表示值。
该方法每次被调用时必需返回一样的值。 

### balanceOf
	public static BigInteger balanceOf(byte[] account)
Returns 账户的token金额。
参数账户必需是一个20字节的地址。如果不是，该方法会抛出一个异常。
如果该账户是个未被使用的地址，该方法会返回0。

### transfer

	public static bool transfer(byte[] from, byte[] to, BigInteger amount)

​从一个账户转移一定数量的token到另一个账户. 参数from和to必需是20字节的地址，否则，该方法会报错。
​参数amount必需大于等于0.否则，该方法会报错。
​如果账户没有足够的支付金额，该函数会返回false。
​如果方法执行成功，会触发转移事件，并返回true，即使数量为0或者from和to是同一个地址。
​函数会检查from的地址是否等于调用合约的hash.如果是，则转移会被处理；否则，函数会调用SYSCALL `Neo.Runtime.CheckWitness`来确认转移。
​如果to地址是一个部署合约，函数会检查其payable标志位来决定是否把token转移到该合约。
​如果转移没有被处理，函数会返回false。

### 事件 transfer

	public static event transfer(byte[] from, byte[] to, BigInteger amount)

​会在token被转移时触发，包括零值转移。
​一个创建新token的token合约在创建token时会触发转移事件，并将from的地址设置为null。
​一个销毁token的token合约在销毁token时会触发转移事件，并将to的地址设置为null。

# 升级

### 合约迁移/升级
智能合约支持在发布之后进行升级操作，但需要在旧合约内预留升级接口。

合约升级主要调用了Neo.Contract.Migrate方法:
```
Contract Migrate(byte[] script, byte[] parameter_list, byte return_type, ContractPropertyState contract_property_state, string name, string version, string author, string email, string description);
```
其中script为新合约的脚本，parameter_list为新合约的参数列表，return_type为新合约的返回值类型，contract_property_state为新合约的属性，name为新合约的名称，version为新合约的版本，author为新合约的作者，email为新合约的电子邮件，description为新合约的说明。

当在旧合约中调用升级接口时，方法将会根据传入的参数构建一个新的智能合约。如果旧合约有存储区，则会将旧合约的存储区转移至新合约中。升级完成后，旧合约将会被删除，如果旧合约有存储区，则存储区也将被删除。之后旧合约将不可用，需要使用新合约的Hash值。

### 合约销毁
智能合约支持在发布之后进行销毁操作，但需要在旧合约内预留销毁接口。

合约升级主要调用了Neo.Contract.Destroy方法:
```
void Destroy();
```
Destroy方法不需要参数，调用该方法后，合约将会被删除，如果合约有存储区，则存储区也将被删除。之后合约将不可用。