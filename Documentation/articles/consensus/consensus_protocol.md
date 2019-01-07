<center><h2>共识协议</center></h2>

## 共识消息格式


### P2P消息格式

| 尺寸 | 字段 | 类型  | 说明 |
|------|------|-------|------|
|  4    | Magic |  uint | 网络p2p协议魔法数，默认为配置文件`protocol.json`中的`ProtocolConfiguration.Magic`值，主网为 `7630401`, 测试网为`1953787457`   |
| 12   | Command | string | `consensus`，共识消息  |
| 4     | length    | uint32 | Payload长度|
| 4     | Checksum | uint | 校验码 |
| length   | Payload | byte[] | `ConsensusPayload` 共识消息内容 |


### ConsensusPayload

| 尺寸 | 字段 | 类型  | 说明 |
|----|------|-------|------|
| 4  | Version |  uint | 共识版本号， 目前为`0` |
| 32  | PrevHash | UInt256 | 上一个区块的hash |
| 4 | BlockIndex |uint | 当前Block高度 |
| 2 | ValidatorIndex | ushort | 发送共识消息的议员，在议员列表中的序号 |
| 4  | Timestamp | uint | 时间戳 |
| ?  |  Data | byte[] | 具体消息内容： `ChangeView`, `PrepareRequest`, `PrepareResponse` |
| 1 |  - | uint8 | 	固定为 1 |
| ? | Witness | Witness | 见证人 |


### Changeview 消息格式

| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
| 1 | Type | ConsensusMessageType | 值 `0x00` |
| 1 | ViewNumber | byte | 当前视图编号 |
| 1 | NewViewNumber | byte |  新视图编号 |


### PrepareRequest 消息格式


| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
| 1 | Type | ConsensusMessageType | 值 `0x20` |
| 1 | ViewNumber | byte | 当前视图编号 |
| 8 | Nonce | ulong |  Block随机值 |
| 20  | NextConsensus | UInt160 |  下一轮共识节点地址的多方签名 |
| 4 + 32 * length   | TransactionHashes | UInt256[] |  打包的交易hash列表 |
| 78  | MinerTransaction | MinerTransaction | 议长的出块奖励交易 |
|  64 | PrepReqSignature | byte[] |  议长对PrepareRequest消息的签名  |
|  64 | FinalSignature | byte[] |  议长对Block的签名 |

### PrepareResponse 消息格式

| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
|  1  | Type | ConsensusMessageType | 值 `0x21` |
|  1  | ViewNumber | byte | 当前视图编号 |
|  ?  | PreparePayload | byte[] | 议长发送的PrepareRequest消息  |
|  64  | ResponseSignature | byte[] | 节点对议长发的PrepareRequest消息签名 |



### CommitAgree 消息格式 


| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
|  1  | Type | ConsensusMessageType | 值 `0x21` |
|  1  | ViewNumber | byte | 当前视图编号 |
|  64  | FinalSignature | byte[] | Block的签名 |


### Regeneration 消息格式 

| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
|  1  | Type | ConsensusMessageType | 值 `0x21` |
|  1  | ViewNumber | byte | 当前视图编号 |
|  ?  | PrepareRequestPayload | byte[] | 议长发送的 PrepareRequest 消息 |
|  64  | SignedPayloads | byte[][] | 收到的 ResponseSignature 签名列表  |


## 传输协议


共识消息进入P2P网络后，和其他数据包一样，进行广播传输，（因为共识节点之间并不知道对方的IP地址), 即普通都可能收到共识数据包。共识消息的广播流程如下图。

[![consensus_msg_seq](../../images/consensus/consensus_msg_seq.jpg)](../../images/consensus/consensus_msg_seq.jpg)


  1. 共识节点A， 直接将共识消息`consensus` 广播连接上的节点B

  2. 节点B在收到`consensus`消息后，先进行共识消息处理，再进行共识消息的转发。转发共识消息前，先发送`inv`消息，并携带上`consensus`消息的`payload`的hash数据。

  3. 若节点C已经收到过该hash对应的数据，或在短时间内，已经重复获取该`inv`消息时，则不处理；否则，进入步骤四。

  4. C向外广播`getdata`消息，附带上`inv`消息中的hash数据。

  5. 节点B收到`getdata`消息后，则发送`consenus`消息给对方。

  6. C节点收到`consensus`消息后，则触发共识模块对消息处理, 以及转发该共识消息,回到步骤二。


### inv 消息格式

| 尺寸 | 字段 | 类型  | 说明 |
|------|------|-------|------|
|  4    | Magic |  uint | 网络p2p协议魔法数|
| 12   | Command | string | `inv`  |
| 4     | length    | uint32 | Payload长度|
| 4     | Checksum | uint | 校验码 |
| length   | Payload | byte[] | `0xe0` + `0x00000001` + `ConsensusPayload.Hash` |

> [!Note] 
> Payload格式为： `inv.type + inv.payloads.length + inv.payload`
> `inv` 消息的payload，有三种类型:
> 1. `0x01`: 交易， inv.payload存放为交易hash列表
> 2. `0x02`: 区块， inv.payload 存放共识消息`ConsensusPayload`的hash列表
> 3. `0xe0`: 共识， inv.payload 存放区块的hash列表


### getdata 消息格式

| 尺寸 | 字段 | 类型  | 说明 |
|------|------|-------|------|
|  4    | Magic |  uint | 网络p2p协议魔法数|
| 12   | Command | string | `getdata`  |
| 4     | length    | uint32 | Payload长度|
| 4     | Checksum | uint | 校验码 |
| length   | Payload | byte[] | `0xe0` + `0x00000001` + `ConsensusPayload.Hash` |

> [!Note] 
> `getdata` 消息主要用来获取 `inv`消息附带hash列表对应的具体内容。




## 共识消息处理

###  校验

1. 检查`ConsensusPayload.BlockIndex`。若小于或等于当前高度，则忽略该消息。

2. 检查验证脚本是否通过，以及验证脚本的地址hash，是否等于`ConsensusPayload.ValidatorIndex`所在议员列表中，对应的地址签名脚本hash。

3. 检查`ConsensusPayload.ValidatorIndex`。若等于当前节点的共识列表序号，则忽略该消息（自己发出的消息，不接收）。

4. 检查共识版本号。若不等于当前共识协议版本号时，则忽略。

5. 检查`ConsensusPayload.PreHash` 和 `ConsensusPayload.BlockIndex`, 是否等于当前共识节点上下文的`PreHash`与`BlockIndex`。 若不是，则忽略。

6. 检查`ConsensusPayload.ValidatorIndex`，若超过当前议员总数时，则忽略。

7. 检查`ConsensusMessage.ViewNumber`, 若不等于当前共识阶段上下文的`ViewNumber`且不是`ChangeView`消息时，则忽略。


### 处理


1. **PrepareRequest** 由一轮共识的议长发出，其中附带了`block`相关的数据：

   1. 若节点已经收到过`PrepareRequest`消息时，则忽略该消息。

   2. 根据`ConsensusPayload.ValidatorIndex` 确定对方是不是本轮的议长，若不是，则忽略。 

   3. 若节点不是议员，且消息不是从`Regeneration`消息中发来的时，则忽略该消息。

   4. 检查 `ConsensusPayload.Timestamp`， 若小于等于上一个区块的时间戳，或者超过了当前时间10分钟以上，则认为消息过期，忽略。

   5. 若消息中的交易，已经存在于上下文时，则忽略该消息。

   6. 检查对议长对`PrepareRequest`消息的签名是否正确，若不是则忽略。
 
   7. 更新上下文，收下议长对提案块和消息的签名。

   8. 检查内存池已经包含的block所需的交易。若交易已经在区块链中，或者插件校验失败，则认为交易数据不对，发起`ChangeView`消息。

   9. 检查未确认交易池中包含的block所需的交易，首先进行交易验证，再进行步骤 8）的检查。

   10. 检查block中第一笔交易，即挖矿交易，同步骤8检查，并进行交易自身验证。若验证失败，忽略该消息。

   11. 若缺少`block`中的交易时，发送`getdata`消息，附带缺少交易的hash列表。

   12. 若交易齐全时，首先校验`PrepareRequest.NextConsensus` 是否等于 结合了最新交易的下一个区块共识节点的多方签名脚本hash，以及检查提案块的网络费是否正确。若不是，则发起`ChangeView`消息；
      1. 若是，则对议长发送的`PrepareRequest`消息进行签名，并广播`PrepareResponse`消息。 
      
      2. 并检查收到的`ResponseSignature`签名个数，若不少于`2f+1`时，则广播 `CommitAgree`消息。

      3. 再检查收到的block签名个数，若不少于`2f+1`时，则共识形成，广播完整的新块。



2. **PrepareResponse** 议员对议长发的`PrepareRequest`消息回应，并附带了对block的签名：
  
   1. 若节点处于`COMMIT-WAITING`阶段时，则忽略该消息

   2. 若对方签名已经收到过，则忽略。

   3. 若在此之前尚未收到 `PrepareRequest` 消息时，则先通过`PrepareResponse`内部附带的`PrepareRequest`进行内部处理。

   4. 校验对方的签名，若通过，则收下`ResponseSignature`签名，否则忽略。

   5. 检查`ResponseSignature`签名数，若已经满足`2f+1`个签名，则广播 `CommitAgree`消息。

   6.  再检查收到的block签名个数，若不少于`2f+1`时，则共识形成，广播完整的新块。



3. **Changeview** 议员或者议长，在遇到超时（议长第一次超时例外，发送`PrepareRequest`消息），或者校验失败时，则发送`ChangeView`消息。议员，议长收到`ChangeView`消息做如下处理：

   1. 若节点已经进入了`COMMIT-WAITING`阶段，则向对方回复`Regeneration`消息，并结束处理。
   
   2. 若新视图编号，小于该议员之前的视图编号，则忽略。
   
   3. 若有不少于`2f+1`个议员的视图编号等于新视图编号时，则切换视图成功，当前议员重置共识流程，视图编号为新的视图编号。



4. **CommitAgree** 当节点收到不少于`2f+1`个 `ResponseSignature`时，则广播`CommitAgree`消息，并附带上对提案块的签名.

    1. 校验对方的提案块的签名

    2. 检查收到的block签名是否达到了`2f+1`， 若达到了，则出新块，并广播。



5. **Regeneration** 由处于 `COMMIT-WAITING`阶段的节点，超时或遇到更改视图事件时，对外广播`Regeneration`消息。

    1. 若节点已经处于`COMMIT-WAITING`阶段时，则忽略该消息

    2. 检查消息体重中的`ResponseSignature`签名

    3. 若`ResponseSignature`签名合格率不少于`2f+1`时，重置共识上下文，将合格的`ResponseSignature`签名收下，并视图编号为`Regeneration`中的视图编号，最后通过`Regeneration`消息附带的 `PrepareRequest`消息触发内部对该消息的处理。



5. **onTimeout** 消息处理

   1. 若是议长超时，且尚未发送`PrepareRequest`消息时，则向外广播`PrepareRequest`消息，并重置定时器。

   2. 若是议长且已经发送过`PrepareRequesst`消息 或 节点是议员时，若尚未进入`COMMIT-WAITING`阶段时， 直接发送`ChangeView`消息， 否则忽略。


6. **NewBlock** 事件处理
 
   1. 重置共识过程。

7. **New Tx** 事件处理

    1. 若是挖矿交易，则忽略。（挖矿交易是直接附在`PrepareRequest`消息中，不需要单独进行广播该交易）。

    2. 若当前节点是议长，或已经发送过`PrepareRequset`或者`PrepareResponse`消息，或正在切换视图中，则忽略该交易。

    3. 若已经收到过该交易，则忽略。

    4. 若交易不在待打包block里面的，则忽略。

    5. 将交易收下，放到待打包的block里。

<a name="6_tx_handler"/>
