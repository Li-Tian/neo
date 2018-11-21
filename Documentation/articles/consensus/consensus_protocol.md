<center><h2>Consensus Protocol</center></h2>

### 一、Consensus Message Format


##### **P2p** message format

| Size | Field | Type  | Description |
|------|------|-------|------|
|  4    | Magic |  uint | Protocol ID, defined in the configuration file `protocol.json`, mainnet is `7630401`, testnet is `1953787457`   |
| 12   | Command | string | Command, all consensus messages' command is `consensus`  |
| 4     | length    | uint32 | Length of payload|
| 4     | Checksum | uint | Checksum |
| length  | Payload | byte[] | Content of message, all consensus messages' payload is `ConsensusPayload`  |


##### **ConsensusPayload** 

| Size | Field | Type  | Description |
|----|------|-------|------|
| 4  | Version |  uint | 	Version of protocol, 0 for now |
| 32  | PrevHash | UInt256 | Previous block's hash |
| 4 | BlockIndex |uint | Height of the block |
| 2 | ValidatorIndex | ushort | The index of the current consensus node in validators array |
| 4  | Timestamp | byte[] | Time-stamp |
| ?  |  Data | byte[] | Includes `ChangeView`, `PrepareRequest` and `PrepareResponse` |
| 1 |  - | uint8 | It's fixed to 1 |
| ? | Witness | Witness | Witness contains invokecation script and verificatioin script |


##### **Changeview** 

| Size | Field | Type  | Description |
|----|------|-----|-------|
| 1 | Type | ConsensusMessageType |  `0x00` |
| 1 | ViewNumber | byte | Current view number |
| 1 | NewViewNumber | byte |  New view number |


##### **PrepareRequest**


| Size | Field | Type  | Description |
|----|------|-----|-------|
| 1 | Type | ConsensusMessageType |  `0x20` |
| 1 | ViewNumber | byte | Current view number |
| 1 | Nonce | byte |  block nonce |
| 20  | NextConsensus | UInt160 |  The script hash of the next round consensus nodes' multi-sign contract  |
| 4 + 32 * length   | TransactionHashes | UInt256[] |  The proposal block's transaction hashes |
| 78  | MinerTransaction | MinerTransaction |  It is used to reward all transaction fees of the current block to the speaker. |
|  64 | Signature | byte[] |  Block signature |

##### **PrepareResponse**

| Size | Field | Type  | Description |
|----|------|-----|-------|
|  1  | Type | ConsensusMessageType |  `0x21` |
|  1  | ViewNumber | byte | Current view number |
|  64  | Signature | byte[] | Block signature |



### 二、Transport Protocol


When consensus message enters the P2P network, it broadcasts and transmits like other data packets, becuase consensus nodes do not know each other's IP address. That is to say, ordinary nodes can receive consensus message. The broadcast flow of consensus messages is as follows.

<p align="center"><img src="../../images/consensus/consensus_msg_seq.jpg" /><br></p>


  1. Before send `consensus` message, send `inv` message attached with `consensus.payload.hash` first. 

  2. If a node has received thee hash corresponding data, or has repeatedly acquiredd the `inv` message in a short time, it will not process it. Otherwise, goto step 3).

  3. Broadcast `getdata` message attached with the hash in the `inv` message.

  4. The consensus node send `consensus` message to it, after receiving `getdata` message.

  5. After receiving the `consensus` message, the node triggers the consensus module to process the message.


##### **inv message format** 

| Size | Field | Type  | Description |
|------|------|-------|------|
|  4    | Magic |  uint | Protocol ID |
| 12   | Command | string | `inv`  |
| 4     | length    | uint32 | Length of payload|
| 4     | Checksum | uint | Checksum |
| length   | Payload | byte[] | Format: `0xe0` + `0x00000001` + `ConsensusPayload.Hash` |

> [!Note] 
> Payload's format： `inv.type + inv.payloads.length + inv.payload`
> `inv` message's payload has three types as follow:
> 1. `0x01`: Transaction. inv.payload is assigned to transaction's hash.
> 2. `0x02`: Block.  inv.payload is assigned to `ConsensusPayload` message's hash.
> 3. `0xe0`: Consensus. inv.payload is assigned to block's hash.


##### **getdata message format** 

| Size | Field | Type  | Description |
|------|------|-------|------|
|  4    | Magic |  uint | Protocol ID|
| 12   | Command | string | `getdata`  |
| 4     | length    | uint32 |  Length of payload|
| 4     | Checksum | uint | Checksum |
| length   | Payload | byte[] | Format: `0xe0` + `0x00000001` + `ConsensusPayload.Hash` |

> [!Note] 
> `getdata` message is mainly used to get the `inv` message with specific content hash.



### 三、 Consensus Message Process

#####  **Verification**

1. If the `ConsensusPayload.BlockIndex` no more than current block height, then ignore.

2. If the verification script executed failed or the script hash not equal to `ConsensusPayload.ValidatorIndex` address's script hash, then ignore.

3. If the `ConsensusPayload.ValidatorIndex` equal to current node index, then ignore.

4. If the `ConsensusPayload.Version` not equal to current consensus version, then ignore.

5. If the `ConsensusPayload.PreHash` and `ConsensusPayload.BlockIndex` are not equal to the current node context's, then ignore.

6. If the `ConsensusPayload.ValidatorIndex` more than the length of the current consensus nodes array, then ignore.

7. If the `ConsensusMessage.ViewNumber` not equal to the current node context's `ViewNumber` and the consensus message is not `ChangeView`, then ignore.

##### **Process**


1. **PrepareRequest** was send by the speaker, attached with block proposal data.

   1. If the current node is not delegates in the consensus round, or the `PrepareRequest` received already, then ignore.

   2. If the `ConsensusPayload.ValidatorIndex` is not the index of the current round speaker, then ignore.

   3. If the `ConsensusPayload.Timestamp` no more than the time stamp of the previous block, or more than 10 minutes above the current time, then ignore.

   4. If the block signature is incorrent, then ingore.

   5. If the block's transantions in the memory pool, are in the blockchain already, or verified failure by the plugin-in, then the transaction data is considered incorrent and initiate the `ChangeView` message.

   6. Check the first transaction in block -- `MinerTransaction`, like step 5). If validation fails, then ignore

   7. Collect the signature in the `PrepareRequest` message.

   8. If there is a lack of transactions in `block`, send the `getdata` message with the hashes of those transactions. 


   9. If the block's transactions all received, then check the `PrepareRequest.NextConsensus` is equal to the script hash of the next round consensus nodes' multi-sign contract. If it is, then broadcast `PrepareResponse` with block signature. If not, then initiate `ChangeView` message.


2. **PrepareResponse** is the Delegates' answer to the `PrepareRequest` message sent by the Speaker attached with block signture.

   1. If the current node has published the new full block, then ignore.

   2. If the block signature has collected already, then ignore.

   3. Check the signature is correct. If not, then ingore, else collect.

   4. If there are at least `N+f` signatures, then publish the new full block.


3. **Changeview** was send by consensus node, when timeout occured or received illegal data

   1. If the view number less than the  sender view number, then ignore.
   
   2. If the new view number less than current node view number, then ignore.

   3. If received at least `N-f` `ChangeView` messages with the same new view number, then the View Change completed. The current node reset the consensus process with the new view number.


4. **onTimeout** 

   1. If the Speaker timeout, the `PrepareRequeset` message will be sent for the first time, and the `ChangeView` message will be luanched subsequently.

   2. If Delegates timeout, then broadcast `ChangeView` message directly.


5. **New Block** 
 
   1. resetting consensus process


6. **New Transaction** 

    1. If the transaction is the `MinerTransaction`, then ignore. As the `PrepareRequest` contains the `MinerTransaction`.

    2. If the current node is the Speaker, or the node has sent `PrepareRequest` or `PrepareResponse` messages, or in switching view process, then ignore.

    3. If the transaction has received before, then ignore.

    4. If the transacion isn't in the propoal block, then ignore.

    5. Collect the transaction.