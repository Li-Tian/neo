<center><h2> The dBFT Algorithm </h2></center>

&emsp;&emsp;The  dBFT(Delegated Byzantine Fault Tolerant) algorithm is based on PBFT(Practical Byzantine Fault Tolerance) algorithm, more suitable in blockchain. PBFT algorithm can solve distributed network consensus effectively, but the more nodes join, the faster the performance drops, as the time complexity is O(n<sup>2</sup>) . On this basis, NEO proposes a dBFT algorithm which combines the characteristics of PoS mode. By voting on the blockchain, it decides the next round of consensus nodes, namely authorizing a few nodes to create block, and the other nodes as ordinary nodes to receive and verify block.


* **Consensus Node**: This node participates in the consensus activity, make a block proposal and vote.

* **Ordinary Node**: This node can transfer, make a transaction, but not participate in the consensus activity.

* **Speaker(One)**: The Speaker is responsible for transmitting a proposal block to the system.

* **Delegates(Multiple)**: Delegates are responsible for voting on the proposal block. The proposal will be accepted, if more than `2f+1` consensus nodes vote it.

* **Validator**: Nodes participating in elections, namely consensus candidate nodes.

* **View**: The dataset used during one consensus activity. The view number start from 0 in each round, and increase number when reach on a consensus failed in one round.


## Algorithm Flow

 
### Symbolic Definition

- N: The number of active consensus nodes.

- f：The maximum threshold of faulty consensus nodes in the system, no more than ⌊(N-1)/3⌋.

- v: The view number, start from 0.

- ℎ：The current block height during consensus activity.

- p: The index of Speaker in array. `p = (h - v) mod N`

- i：The index of consensus node in array. 

- t: The block time, config in `protocol.json/SecondsPerBlock`, default 15 seconds.

- 𝑏𝑙𝑜𝑐𝑘：The proposal block

- 〈𝑏𝑙𝑜𝑐𝑘〉<sub>𝜎𝑖</sub>: The block's signature of the `i`th consensus node.

### General Procedures


Assume the total number of active consensus nodes is `N`, up to `f` fault tolerance nodes. At the begin, the nodes have the same view number `v = 0`, and block height `h = current block height`. If not at the same height, it can be achieved by block synchronization between P2Ps. The process involved in the consensus algorithm is as follows:

1. Users initate a transaction through wallet, such as transfer, deploy intelligent contract, release assets, etc.

2. The wallet signs the transaction data, and broadcasts to the p2p network.

3. The consensus nodes received the transaction, and put into the memory pool.

4. In one round, the Speaker package the transactions from the memory pool into a new block,  then broadcast the proposal block 〈𝑃𝑟𝑒𝑝𝑎𝑟𝑒𝑅𝑒𝑞𝑢𝑒𝑠𝑡,ℎ,𝑣,𝑝,𝑏𝑙𝑜𝑐𝑘,
〈𝑏𝑙𝑜𝑐𝑘〉<sub>𝜎𝑝</sub>〉. 

   1. Load all the transactions in memory pool.

   2. Load [`IPolicyPlugin`](https://github.com/neo-project/neo-plugins) plugin, sort and filter the transactions.
   
   3. Calculate the network fee (`= inputs.GAS - outputs.GAS - transactions_system_fee `), and take it as the reward for the current Speaker in `MinerTransaction`.

   4. Combining the above transactions and the previous validators votes, calculate the next round consensus nodes, and assign the hash of multi-signature script to `block.NextConsensus`, locking the consensus nodes of the next round.

   5.  Set the timestamp of block to the current time and calculate the signature of the Speaker.
   
   6. Broadcast `PrepareRequset` messsage.

   7. Broadcast `inv` message, attached with transactions' hashs except `MinerTransaction`, to notify the other nodes to synchronize the transactions in the proposal block.

5. Delegates recieved the proposal block, and verify the new block, then broadcast 〈𝑃𝑟𝑒𝑝𝑎𝑟𝑒𝑅𝑒𝑠𝑝𝑜𝑛𝑠𝑒,ℎ,𝑣,𝑖,〈𝑏𝑙𝑜𝑐𝑘〉<sub>𝜎𝑖</sub>〉 message.

6. Any node, receiving at leat `n-f` 〈𝑏𝑙𝑜𝑐𝑘〉<sub>𝜎𝑖</sub> , reaches a consensus and publishes the full block.

7. Any node, after receiving the full block, deletes all the full block's transactions in the memory pool. If the node is the consensus node, then enter the next round consensus.


[![dbft_two_phase](../../images/consensus/dbft_two_phase_en.jpg)](../../images/consensus/dbft_two_phase_en.jpg)


The algorithm can be divided into three stages.<BR/>
1) `PRE-PREPARE`, the speaker of this round is responsible for broadcasting `Prepare-request` message to the delegates and initiating the proposal block.<BR/>
2) `PREPARE`, the delegates after receiving `PRE-PREPARE`, then broadcast `Prepare-Response` if the proposal verified successful. When a node receives at least `N-f` 〈𝑏𝑙𝑜𝑐𝑘〉<sub>𝜎𝑖</sub>, it enters the third stage.<BR/>
3) `PERSIST`, the node publishes the full node and enter the next consensus round.<BR/>

> [!Note]
> 1. At the beigining of the blockchain network started, `StandbyValidators` are read from the configureation file `protocol.json` by default.
> 2. Unlike ordinary block, genesis block is the first block in the blockchain by default, which is not published by consensus nodes. The `NextConsensus` in the genesis block specifies the conosensus nodes of the next block as the `StandbyValidators` nodes.

### View Change

As the process of consensus on a open p2p network environment, there may be network delay, evil node sending illegal data, etc. The consensus nodes can initiate a `ChangeView` proposal. They enter a new view with new speaker, and restart consensus, after receiving at least `N-f` `ChangeView` messages with the same view number.

[![dbft_state_graph](../../images/consensus/dbft_state_graph.jpg)](../../images/consensus/dbft_state_graph.jpg)

The View Change will take place, when one consensus node could not reach a consensus in 2<sup>v+1</sup>⋅ 𝑡  time interval, or received illegal proposals such as contain invalid transactions.

1. Given 𝑘 = 1, 𝑣<sub>𝑘 </sub>= 𝑣 + 𝑘； 

2. The `𝑖`th node initiate a 〈𝐶ℎ𝑎𝑛𝑔𝑒𝑉𝑖𝑒𝑤,ℎ,𝑣,𝑖,𝑣<sub>𝑘</sub>〉 proposal.

3. When any one node received at least `N-f` `ChangeView` with the same 𝑣<sub>𝑘</sub> from different consensus nodes, the View Change will be completed. Set 𝑣 = 𝑣<sub>𝑘</sub> and start the consensus process.

4. If the View Change is not completed in 2<sup>𝑣<sub>𝑘 </sub>+1</sup> ⋅ 𝑡 time interval, then increase k and back to step 2).


With the k increase, the overtime waiting time will increase exponentially, which can avoid frequent View Change and make the nodes reach agreement as soon as possible. The original view `v` is still valid until the completion of View Change, avoiding unnecessary View Change due to accidental network latency.

[1] [A Byzantine Fault Tolerance Algorithm for Blockchain](http://docs.neo.org/zh-cn/basic/consensus/whitepaper.html)<br/>
[2] [Consensus Diagram](http://docs.neo.org/zh-cn/basic/consensus/consensus.html)<br/>
[3] [Practical Byzantine Fault Tolerance](http://pmg.csail.mit.edu/papers/osdi99.pdf)<br/>
[4] [The Byzantine Generals Problem](https://www.microsoft.com/en-us/research/wp-content/uploads/2016/12/The-Byzantine-Generals-Problem.pdf)<br/>
[5] [Consensus Plugin](https://github.com/neo-project/neo-plugins)



> [!NOTE]
> In case of dead links, please contact <feedback@neo.org>


