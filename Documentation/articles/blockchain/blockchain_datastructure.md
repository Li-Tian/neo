<center><h2>Block</h2></center>

<!-- &emsp;&emsp;在比特币系统中，可以存在多条链并存，即暂时分叉，但经过足够长的时间后最长的链条（提供了最多的工作量证明）成为确认的区块链。 Neo因为采用了dBFT共识算法，在系统正常运行时，不会同时出现一条以上的链。请参见`共识`部分。 -->
&emsp;&emsp; The Blockchain is a data structure. The block is composed of block header and block body. As each block has a crpytographic hash of the previous block, a timestamp, and transaction data (generally represented as a merkle tree root hash), and a chain structure is formed.

&emsp;&emsp; The data structure of block as following:

| Size | Field | Name  | Type | Description |
|----|-----|-------|------|------|
| 4 | Version  | uint | Block version, current is `0` |
|32 | PrevHash  | UInt256 | The previous block's hash |
|32 | MerkleRoot | Uint256 |The merkle tree root of the block's transactions  |
| 4 | Timestamp  | uint |   |
| 4 | Index | uint |  Block height, and the Genesis Block's index is 0 |
| 8 | ConsensusData | Nonce | ulong | It's random value |
|20 | NextConsensus  | UInt160 |  The script hash of consensus nodes' multi-party signature contract in the next round. |
| 1 | - | uint8 | it's fixed 1   |
| ? | Witness | |  Witness | The executable verification scripts|
| 1 | - | uint8 | it's fixed 1   |
|?*?| Transactions  |  Transaction[] | trransaction list |

[![../../images/blockchain/blockchain.jpg](../../images/blockchain/blockchain.jpg)](../../images/blockchain/blockchain.jpg)


### **Block Header** 
### **区块头** 


&emsp;&emsp; The block header contains the basic information of a block and provide verification of a block.  The first 10 attributes of the block constitute the header.

&emsp;&emsp;Block `hash` and `index` can be used to identity a block. The hash value is obtained by concating the first seven attributes of the block header and performing SHA256 operation twice. Normally, NEO has only one chain, and each block is confirmed by more than two-thirds of the consensus nodes, before added to the blockchain. Therefore, the height of the each block is unique. Block height must be equal to the previous block height plus 1, and the Genesis Block height is 0. 


&emsp;&emsp;`Timestamp` is the bock's time stamp, must bigger than the previous block's.The invternal between two block is about 15 seconds, set by the variable `SecondsPerBlock` in configuration file `protocol.json`.   

&emsp;&emsp;`NextConsensus` is a hash of mulit-signature contract, which needs more than two-thirds of the signatures of the consensus nodes as parameters. The example script as below. Each block, with the `NextConsensus` field, locks the nodes participating in the next round of consensus activity. In the previous round of consensus activity, the Speaker calcualted the consensus nodes of the next round based on the voting at that time, generated the multi-party signature contract, and assigned the hash value of the contract to the block's `NextConsensus` field. 

&emsp;&emsp;`Witness` is the verification script of the block, it contains `InvocationScript` and `VerificationScript`. The `InvocationScript` provides the parameters for the `VerificationScript` to execute. 


[![../../images/blockchain/nextconsensus_script.jpg](../../images/blockchain/nextconsensus_script.jpg)](../../images/blockchain/nextconsensus_script.jpg)

### **Block Body**
### **区块主体**

&emsp;&emsp; The block body is a transaction list. In one round of consensus activity, the Speaker select all the transactions in memeory pool, sort and filter by plugin, package them into a new proposal block. For more details about consensus, please read "Consensus Mechanism" section.

&emsp;&emsp; The first transaction of each block must be `MinerTransaction`, which is used for distribution of transaction's network fees in the block. At present, there are up to 500 transactions per block and 20 free transactions.


> [!NOTE]
> When a block persistent, it stores a hash list of the block's transaction, and the transaction data is stored separately for facilitate query.

