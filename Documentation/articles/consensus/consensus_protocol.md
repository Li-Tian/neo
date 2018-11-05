<center><h2>共识协议</center></h2>


共识数据包在网络中传播（共识节点之间互相不知道IP地址）



### 一、共识消息格式


1. **P2P消息格式**

| 尺寸 | 字段 | 类型  | 说明 |
|------|------|-------|------|
|  4    | Magic |  uint | 网络p2p协议魔法数|
| 12   | Command | string | `inv`, `consensus` 两个指令 |
| 4     | length    | uint32 | Payload长度|
| 4     | Checksum | uint | 校验码 |
| length   | Payload | byte[] | `ConsensusPayload` 共识消息内容 |


2. **ConsensusPayload** 

| 尺寸 | 字段 | 类型  | 说明 |
|----|------|-------|------|
| 4  | Version |  uint | 网络p2p协议魔法数|
|   | PrevHash | UInt256 | 指令 |
| 4 | BlockIndex |uint | 当前Block高度 |
| 1 | ValidatorIndex | ushort | 验证人的编号 |
|   | Timestamp | byte[] | 时间戳 |
| ?  |  Data | byte[] | 具体消息内容： `ChangeView`, `PrepareRequest`, `PrepareResponse` |



3. **Changeview** 消息格式

| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
| | Type | ConsensusMessageType |  `0x00` |
| | ViewNumber | int | 当前视图编号 |
| | NewViewNumber | byte |  新视图编号 |


4. **PrepareRequest** 消息格式


| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
| 4 | Type | ConsensusMessageType |  `0x20` |
| 4 | ViewNumber | int | 当前视图编号 |
| 1 | Nonce | byte |  Block随机值 |
|  | NextConsensus | byte |  下一轮共识节点地址的多方签名 |
| | TransactionHashes | byte |  打包的交易hash列表 |
| | MinerTransaction | byte |  矿工交易 |
| | Signature | byte[] |  正在共识的Block的签名 |

5. **PrepareResponse** 消息格式

| 尺寸| 字段 | 类型 | 说明  |
|----|------|-----|-------|
| | Type | ConsensusMessageType |  `0x21` |
| | ViewNumber | int | 当前视图编号 |
| | Signature | byte[] | 正在共识的Block的签名 |



### 二、传输协议


p2p 共识消息的传输

 inv -> inv.hash
 getddata <- inv.hash
 consensus -> inv.data


### 三、 共识消息处理

#### 3.1 校验




#### 3.2 处理


1. **PrepareRequest** 消息处理



2. **PrepareResponse** 消息处理



3. **Changeview** 消息处理


4. **onTimeout** 消息处理


5. **NewBlock** 事件处理



6. **New Tx** 事件处理



