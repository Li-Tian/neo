# 交易介绍

交易，是区块链网络的交互操作的唯一方式，包括发布资产，转账，发布智能合约，合约调用等等，都是基于交易的方式进行操作处理。
NEO中的交易，也是采用比特币类似交易的设计，每一笔交易都包含三个重要部分：input, output, scripts。 input代表资金来源，output代表资金流出，scripts是对input所引用交易的output的验证解锁，通过这种 input-output 组合方式，将资产的每一笔流动都形成了一条链条结构。


## 交易类型

NEO中定义的交易类型如下所示：

| 名称 | 系统费用 | 描述 |
| --------   | :-----:   | :----: |
| MinerTransaction | 0 | 用于分配字节费的交易 |
| RegisterTransaction | 10000/0 | (已弃用) 用于资产登记的交易
| IssueTransaction | 500/0 | 用于分发资产的交易
| ClaimTransaction | 0 | 用于分配 NeoGas 的交易
| EnrollmentTransaction | 1000 | (已弃用) 用于报名成为共识候选人的特殊交易
| StateTransaction | 1000/0 | 申请见证人或共识节点投票
| ContractTransaction | 0 | 合约交易，这是最常用的一种交易
| PublishTransaction | 500*n | (已弃用) 智能合约发布的特殊交易
| InvocationTransaction | 0 | 调用智能合约的特殊交易



## 交易流程

### 1 合约交易
合约交易是最常用的一种交易，用于在不同账户之间转移资产。进行合约交易有三种方式：

#### 通过CLI指令

send <id|alias> <address> <value>|all [fee=0]
参数说明：
<id|alias>：资产ID或名称
<address>：对方地址
<value>转账金额，也可以用all表示所有资产
[fee]：手续费，缺省值为0


#### 通过RPC调用

sendfrom：从指定地址，向指定地址转账。

sendtoaddress：向指定地址转账。

sendmany：批量转账命令，并且可以指定找零地址。

具体用法请参照[NEO Documentation: API 参考](http://docs.neo.org/zh-cn/node/cli/apigen.html)。 


#### 通过GUI界面

具体操作请参照[NEO Documentation: NEO GUI 交易](http://docs.neo.org/zh-cn/node/gui/transc.html)。


#### 合约交易的步骤

#### （1）创建交易

函数：Neo.Wallets.Wallet:: public Transaction MakeTransaction(List<TransactionAttribute> attributes, IEnumerable<TransferOutput> outputs, UInt160 from = null, UInt160 change_address = null, Fixed8 fee = default(Fixed8));

参数：
List<TransactionAttribute> attributes：表示交易特性，这里的输入值为空。
IEnumerable<TransferOutput> outputs：表示要处理的转账信息。
UInt160 from：表示转账的付款账户。
UInt160 change_address：找零收款地址。
Fixed8 fee：系统费。

函数逻辑：根据输入的参数创建相应的交易对象。
生成交易步骤：
（1）统计交易：将转账信息列表各对象按照其每个成员对象的（资产ID，地址）组合进行分组，每个分组即为一个独立的收款方；并求出每个分组的成员的Value之和。
（2）账户信息：获取钱包中，非Lock且非WatchOnly状态的账户的余额信息
（3）生成交易：对于每个收款方，若其数额小于当前钱包各账户余额之和，返回；
                         否则，按照“使用账户数目最少”规则生成交易：
                               若存在余额大于转账金额的账户，则从满足条件的账户中余额最小的账户转账；
                               否则，使用贪心法：将钱包各账户按照余额由高到低排序，依次调用各账户：
                                      若当前账户余额小于等于当前转账金额，将当前账户加入返回结果，并从转账金额中扣除当前账户的余额；
                                      否则，若此时转账金额不为零，则从剩下的账户中，选择余额大于等于当前转账金额的账户中，余额最小的账户加入返回结果；为零则直接返回。


#### （2）签名验证：进行签名，加入到未处理队列，以及交易验证

函数：Neo.Wallets.Wallet:: public bool Sign(ContractParametersContext context);

参数：
ContractParametersContext context：将上一步生成的交易转换为ContractParametersContext类型，包含了交易的所有信息。

函数逻辑：钱包签名，及交易验证，处理结果在context.Completed里面。
签名验证步骤：
（1）对于输入的协议的ScriptHashes的每个对象，取得相应的账户，若对应帐户为空，或该账户没有密钥串，则跳过；

（2）否则，否则获取密钥串后，用该密钥串对输入协议的Verifiable签名，获取返回的byte数组；

（3）进行加签，具体步骤如下：
         若输入的协议是多重签名，则根据输入的contract创建ContextItem。若创建失败或显示已经加签过，返回false；
         否则，对contract.Script的内容，从第二字节跳过第一字节的数值个字节后，开始按照34byte的间隔依次解码生成List<ECPoint> points：每次检查当前字节是否为33，若是则读取下33字节进行解码操作，若非则退出循环。若points中包含pubkey则返回false，否则将(pubkey, signature)键值对加入item.Signatures。
         若创建的ContextItem的Signatures.Count == contract.ParameterList.Length，对于points中的每个点，以及ContextItem中的每个对象作join并按照PublicKey倒序排序，得到的二维字节数组包含了按照索引排序的签名。将这些签名以此添加到contract对应的Parameters中。

         若输入的协议不是多重签名，判断contract.ParameterList中是否存在唯一的0x00（类型ContractParameterType.Signature）：
                若不存在，返回false；
                若存在多个抛异常；
                否则记下这个位置index，将contract的Parameters的index位置赋值为signature

（4）对输入的协议的ScriptHashes的加签结果取或，并返回，作为加签成功与否的结果。

什么时候需要验证一笔交易？

（1）节点收到一笔交易： LocalNode 从 AddTransactionLoop 收到新的交易时候，验证每一笔交易。

（2）节点产生/转发一笔交易： LocalNode 从 Relay(tx) 的时候，验证一笔交易。

（3）节点持久化block后对剩余待验证交易的验证： LocalNode：从 Blockchain_PersistCompleted 持久化block的后，验证每一笔内存池中待剩余验证的交易。

（4）共识过程中的新交易： ConsensusService 共识的过程中，对需要打包 AddTransaction 的新交易的验证。

交易脚本验证的背后思考
（1）对需要验证的脚本，提供好参数，最后通过NVM进行执行，得到是否都返回 True , 则脚本验证通过。

（2）每一个地址，都是一个签名验证checksig代码段，执行的时候，都需要签名参数。类似的多签合约地址，调用的是 Opt.CCHECKMULTISIG 方法，需要指定数量的签名参数。

（3）每一笔交易的待验证脚本包括： tx_in所指向的tx.output.scriptHash 脚本（input的tx的收款人脚本)，这样确保了，只有对应的钱包才能使用该笔 unspend tx output。

（4）当遇到自定义的地址脚本时，需要按照对方的脚本形参，提前准备好参数（不一定是签名参数）进行验证。


#### （3）广播推送：共识议长节点，将改交易，添加到内存池交易，并打包到新的区块中，进行共识处块，最后将新块广播到网络上。

广播推送步骤：

（1）调用 LocalNode.relay(tx) 方法，进行交易的验证 与 p2p广播

         函数：Neo.Network.LocalNode:: public bool Relay(IInventory inventory);
         参数：IInventory inventory：上一步生成的交易对象，包含了交易的所有信息。、

         步骤：
         这里的输入类型为ClaimTransaction。

         若输入inventory为挖矿交易（MinerTransaction）返回false；

         否则若inventory的哈希并未过期不需更新，返回false；

         否则，触发LocalNode_InventoryReceiving事件后，验证并添加inventory，向远端节点推送inventory后，触发LocalNode_InventoryReceived事件并返回推送结果


（2）本地的 LocalNode 会调用 RemoteNode.relay 进行实际的广播交易

（3）本地的 RemoteNode 会先进行bloom filter过滤，是否重复发送, 没有的话进行 tx_hash 消息广播。

（4）远程节点在 StartProtocol 中，监听收到的新消息，首先会调用 OnMessageReceived(msg) 进行消息区分。

（5）远程节点识别到后，调用 OnInvMessageReceived(msg) 进行处理，识别到一条新消息时候，会向本地接节点发送 getdata 事件。

（6）本地的 RemoteNode 识别到是 getdata 消息收，以及是要发送的 tx.hash ，将完整的 tx 数据，发送给远程。

（7）远程节点的 RemoteLocal 收到新消息是 tx 类型时，会在 OnMessageReceived 中，调用 OnInventoryReceived 进行处理。

（8）远程节点的 RemoteLocal 在 OnInventoryReceived 中检测消息的时效性，并触发 LocalNode 的 RemoteNode_InventoryReceived 的操作。

（9）远程节点的 LocalNode ，在 RemoteNode_InventoryReceived 中，将收到的交易，先存放到临时交易池 temp_pool 中，并触发一个新交易事件 new_tx_event 。

（10）AddTransactionLoop 收到交易事件触发后，从临时交易池 temp_pool 中取出交易，进行验证，并将严格合格的交易，放入到内存池中 mem_pool 中。最后，该节点进行p2p的交易广播。

#### （4）交易处理：节点接收到新快后，验证块，最后进行持久化块操作（其中，对不同的交易进行不同的处理，比如投票类，执行合约交易，发布资产类等等）。 钱包对收到的新块，更新相关资产变动账户，交易状态，以及未确认交易队列。

（1）Core.Blockchain 对block的处理

随着对block的persist操作，每种交易的最终处理，也在该过程进行处理:

交易涉及到的 account进行更新操作（增，减）

账户有参与见证人投票的话，更新 validators , validators_count

添加 inputs 涉及到的交易到 spendcoins 列表中。 outputs 涉及到的交易到unspendcoins 列表中。

（2）Wallet 对 Block中的交易处理

Wallet 会启动一个线程，监听新来的block，并对block的交易进行如下处理：

处理 outputs, 更新交易状态， 和 账户变动；

处理 intputs, 移除跟踪的地址，与 交易；

移除ClaimTx中的跟踪地址与交易；

触发 资产变动事件。

（3）资产变动事件处理

从未确认队列中，移除确认的新交易


从一个钱包地址发起转账到另一个钱包地址。创建交易，签名，发送到网络。

经过节点网络传播到共识节点，然后进入内存池，再由共识节点协作写入新的区块。

然后新的区块再次经过节点网络传播到每一个节点。最后这笔交易被保存到各个节点
的数据库。

从一个钱包地址发起转账到另一个钱包地址。创建交易，签名，发送到网络。

经过节点网络传播到共识节点，然后进入内存池，再由共识节点协作写入新的区块。

然后新的区块再次经过节点网络传播到每一个节点。最后这笔交易被保存到各个节点
的数据库。

（需要描述一下 Blockchain 对象所需要提供的功能及接口级别的细节。但是不需要写LevelDB级别的实现细节）

描述不同交易类型(Transaction Type)的执行过程中的不同。

描述不同交易类型的 TransactionAttribute / Usage 的设置。

描述上述过程的详细过程。

※以上内容为建议，可根据对概念的理解调整文章的框架。