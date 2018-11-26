<center><h2>交易</h2></center>


一个普通的交易的数据结构如下：

| 尺寸 | 字段 | 类型 | 描述 |
|-----|-----|------|-------|
| 1   | Type    | byte | 交易类型 |
| 1 | Version | byte | 交易版本号，目前为0 |
| ? | - | - | 特定交易的数据 |
| ?*? | Attributes | tx_attr[] | 该交易所具备的额外特性 |
| 34*? | Inputs | tx_in[] | 输入 |
| 60 * ? | Outputs | tx_out[] | 输出 |
| ?*? | Scripts | Witness[] | 用于验证该交易的脚本列表 |

### Input

每个交易中可以有多个Input，也可能没有Input。在下面提到的MinerTransaction中Input就为空。Input的数据结构如下： 


| 尺寸 | 字段 | 类型 | 描述 |
|---|-------|------|------|
| 32 | PrevHash | UInt256 | 被引用交易的散列值 |
| 2 | PrevIndex | ushort | 被引用交易输出的索引 | 


### Output

每个交易中最多只能包含 65536 个输出。Output的数据结构如下：


| 尺寸 | 字段 | 类型 | 描述 |
|---|-------|------|------|
| 32 | AssetId | UIntBase | 资产Id |
| ?  | Value | BigDecimal | 转账金额 | 
| 20 | ScriptHash | UInt160 | 地址，即账户地址或合约地址 |


### Attribute


| 尺寸 | 字段 | 类型 | 描述 |
|---|-------|------|------|
| 1 | Usage | byte | 属性类型 |
| 0|1 | length | uint8 | 	数据长度（特定情况下会省略） |
| ? | Data | byte[length] | 特定于用途的外部数据 | 

TransactionAttributeUsage，交易属性使用表数据结构如下：

| 字段 | 值 | 描述 |
|-------|-----|----|
| ContractHash | 0x00 | 外部合同的散列值 |
| ECDH02 | 0x02 | 用于ECDH密钥交换的公钥，该公钥的第一个字节为0x02 |
| ECDH03 | 0x03 | 用于ECDH密钥交换的公钥，该公钥的第一个字节为0x03 |
| Script | 0x20 | 用于对交易进行额外的验证, 如股权类转账，存放收款人的脚本hash |
| Vote | 0x30 |  |
| DescriptionUrl | 0x81 | 外部介绍信息地址 |
| Description | 0x90 | 简短的介绍信息 |
| Hash1 - Hash15 | 0xa1-0xaf | 用于存放自定义的散列值 |
| Remark-Remark15 | 0xf0-0xff | 备注 |

ContractHash、ECDH02-03、Vote和Hash1-15的数据长度固定为 32 字节，所以省略length字段。Script必须明确给出数据长度，且长度不能超过 65535。而DescriptionUrl、Description和Remark1-15必须明确给出数据长度，且长度不能超过 255。


### Witness

见证人，实际上是可执行的验证脚本。`InvocationScript` 脚本传递了`VerificationScript`脚本需要的补充参数。只有当脚本执行返回真时，验证成功。

| 尺寸 | 字段 | 类型 | 描述 |
|--|-------|------|------|
| ?  | InvocationScript | byte[] |调用脚本，补全脚本参数 |
| ?  | VerificationScript | byte[] | 验证脚本  | 


调用脚本进行压栈操作相关的指令，用于向验证脚本传递参数（如签名等）。脚本解释器会先执行栈脚本代码，然后执行验证脚本代码。

`Block.NextConsensus`所代表的多方签名脚本，填充签名参数后的可执行脚本，如下图所示，[`Opt.CHECKMULTISIG`](../neo_vm.md#checkmultisig) 在NVM内部执行时，完成对签名以及公钥之间的多方签名校验。

[![nextconsensus_witness](../../images/blockchain/nextconsensus_witness.jpg)](../../images/blockchain/nextconsensus_witness.jpg)

### **Transaction类型**

Neo中一共定义了9种不同的交易，包括MinerTransaction、RegisterTransaction、IssueTransaction和ContractTransaction等。这9种交易的具体功能请见下图。 


| 编号 | 类型名 | 值  | 系统费用 |用途 |  解释  |
|------|--------|-----|----------|-------|----------|
|  1  | MinerTransaction | 0x00 | 0 | 创建“矿工”交易 | 块的第一条交易，用于分配字节费的交易 |
|  2  | RegisterTransaction | 0x40 | 10000/0 | 注册资产，仅用于NEO和GAS | 已弃用 |
|  3  | IssueTransaction | 0x01 | 500/0 | 分发资产 |
|  4  | ClaimTransaction | 0x02 | 0 | 提取GAS | 每个区块的奖励分发 |
|  5  | StateTransaction | 0x90 | *  | 验证人选举统计选票时使用 | 
|  6  | EnrollmentTransaction | 0x20 | 1000 | 报名成为验证人 | 已弃用 |
|  7  | ContractTransaction | 0x80 | 0 | 转账时用 | 最常用的交易类型 |
|  8  | PublishTransaction | 0xd0 | 500*n |应用合约发布交易 | 已弃用 |
|  9  | InvocationTransaction | 0xd1 | 0 | 合约调用交易 | 用来调用合约，部署合约后或生成新资产之后会使用 | 


<!-- 以上这9种交易并不能完成所有的功能实现，比如部署合约和生成NEO和GAS以外的NEP5新资产。这两种功能是通过系统调用来完成，但最后还是需要使用InvocationTransaction以交易的形式来将这个事情加入到区块链中。 -->


下面给出2个例子。第一个例子是生成创世块，展示了只使用提供的交易类型生成资产；第二个例子是生成NEP5资产，展示了系统调用和合约的方式生成新资产。

### **例1：GenesisBlock**

创世块，是默认已经定义在代码中不可修改的区块链的第一个区块，高度为0。在创世块中生成了NEO和GAS资产。注意，也只有NEO和GAS是使用MinerTransaction和RegisterTransaction生成了，其他的NEP5代币都是通过系统调用的方式生成的。

| 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
|  4  | Version | 区块版本 | uint | `0` |
| 32   | PrevHash | 上一个区块Hash | UInt256 |  `0x0000000000000000000000000000000000000000000000000000000000000000` |
|  32  | MerkleRoot | Merkle树 | uint256 |`0x803ff4abe3ea6533bcc0be574efa02f83ae8fdc651c879056b0d9be336c01bf4`  |
| 4  | Timestamp |  时间戳 | uint | `2016-07-15 | 23:08:21` |
| 4   | Index | 区块高度 | uint |  `0` |
|  8  | ConsensusData | Nonce | ulong | `2083236893`, 比特币创世块nonce值，向比特币致敬  |
| 20  | NextConsensus | 下一个共识地址 | UInt160 | 下一个出块的共识节点的三分之二签名脚本hash   |
| 1  | - | - | uint8 | 	固定为 1   |
|  ?   | Witness | 见证人 |  Witness |  `0x51`, 代表`PUSHT`指令，返回永真 |
|  ?*? | **Transactions** | 交易 |  Transaction[] | 目前存了4笔交易， 见后续表 |


第一笔交易是MinerTransaction，即“挖矿”交易。所有的block的第一笔交易，都必须是MinerTransaction。Neo中没有挖矿的概念。这里的MinerTransaction主要是在Outputs中记下所有的费用都归于哪里。


| 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
| 1   | Type    | uint8 | 交易类型 | `0x00` |
| 1 | Version | uint8 |  交易版本号 | `0` |
| 8 | Nonce | ulong | nonce  | `2083236893` |
| ?*? | Attributes | tx_attr[] | 该交易所具备的额外特性 |    空 |
| 34*? | Inputs | tx_in[] | 输入 | 空 |
| 60 * ? | Outputs | tx_out[] | 输出 | 空 |
| ?*? | Scripts | Witness[] | 用于验证该交易的脚本列表 | 空 |


第二笔交易是RegisterTransaction，用来注册NEO代币

| 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
| 1   | Type    | byte | 交易类型 | `0x40` |
| 1 | Version | byte |  交易版本号 | `0` |
| 1 | AssetType | byte | 资产类型  | `0x00` |
| ? | Name | string | 资产名字  | `NEO` |
| 8 | Amount | Fix8 | 总量  | `100000000` |
| 1 | Precision | byte | 精度  | `0` |
| ? | Owner | ECPoint | 所有者公钥  |  |
| 32 | Admin | UInt160 | 管理者  | `0x51`.toScriptHash |
| ?*? | Attributes | tx_attr[] | 该交易所具备的额外特性 |    空 |
| 34*? | Inputs | tx_in[] | 输入 | 空 |
| 60 * ? | Outputs | tx_out[] | 输出 | 空 |
| ?*? | Scripts | Witness[] | 用于验证该交易的脚本列表 | 空 |

`NEO`名称定义 = `[{"lang":"zh-CN","name":"小蚁股"},{"lang":"en","name":"AntShare"}]`


第三笔交易也是RegisterTransaction，用来注册GAS代币

| 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
| 1   | Type    | byte | 交易类型 | `0x40` |
| 1 | Version | byte |  交易版本号 | `0` |
| 1 | AssetType | byte | 资产类型  | `0x01` |
| ? | Name | string | 资产名字  | `GAS` |
| 8 | Amount | Fix8 | 总量  | `100000000` |
| 1 | Precision | byte | 精度  | `8` |
| ? | Owner | ECPoint | 所有者公钥  | |
| 32 | Admin | UInt160 | 管理者  | `0x00`.toScriptHash, 即 `OpCode.PUSHF`指令脚本 |
| ?*? | Attributes | tx_attr[] | 该交易所具备的额外特性 |    空 |
| 34*? | Inputs | tx_in[] | 输入 | 空 |
| 60 * ? | Outputs | tx_out[] | 输出 | 空 |
| ?*? | Scripts | Witness[] | 用于验证该交易的脚本列表 | 空 |

`GAS`名称定义 =  `[{"lang":"zh-CN","name":"小蚁币"},{"lang":"en","name":"AntCoin"}]`

第四笔交易是IssueTransaction，用来把NEO发放到合约地址

| 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
| 1   | Type    | byte | 交易类型 | `0x01` |
| 1 | Version | byte |  交易版本号 | `0` |
| ?*? | Attributes | tx_attr[] | 该交易所具备的额外特性 |    空 |
| 34*? | Inputs | tx_in[] | 输入 | 空 |
| 60 * ? | Outputs | tx_out[] | 输出 | 有一笔output，见下表 |
| ?*? | Scripts | Witness[] | 用于验证该交易的脚本列表 | `0x51`, 代表 `OpCode.PUSHT` |

其中，Output定义了将所有的NEO代币，转移到共识节点多方签名地址上。而Scripts这里是空的，意思是这个交易因为是创世时做的，不需要验证了。

| 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
| 1   | AssetId    | byte | 资产类型 | `0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b`， NEO代币 |
| 8 | Value | Fix8 |  转账总量 | `100000000` |
| 20 | ScriptHash | UInt160 |  收款脚本hash |  备用共识节点多方签名合约地址 |


### **例2：生成新资产，即NEP5资产**。


<!-- | 尺寸 | 字段 | 名称  | 类型 | 值 |
|----|-----|-------|------|------|
| 1   | Type    | byte | 交易类型 | `0xd1` |
| 1 | Version | byte |  交易版本号 | `0` |
| ? | Script | byte[] | NEP-5合约代码  | todo `0x....` |
| 8 | Gas | Fix8 | 手续费  |  |
| ?*? | Attributes | tx_attr[] | 该交易所具备的额外特性 |    空 |
| 34*? | Inputs | tx_in[] | 输入 | 空 |
| 60 * ? | Outputs | tx_out[] | 输出 | 空 |
| ?*? | Scripts | Witness[] | 用于验证该交易的脚本列表 | 空 | -->

请参考[NEP5协议](https://github.com/neo-project/proposals/blob/master/nep-5.mediawiki) 进 [部署和调用智能合约](http://docs.neo.org/zh-cn/sc/quickstart/deploy-invoke.html)