<center><h2>Consensus Mechanism</h2></center>

&emsp;&emsp; All public-blockchains run on the peer-to-peer network, their nodes maintain the common account. Therefore, how to motivate the node to supply reliable service, how to solve the "Byzantine Fault Tolerant" problem, and support high performance at the sametime, it becomes a big problem for most blockchain companies. Decentralization, scalability and security have become the "Impossible Triangle" problem at present.

&emsp;&emsp;On the consensus mechanism, many blockchains can be divided into the following categories:

- POW (Proof of work): Represented by bitcoin, it uses computing power to handle fault tolerance. Criticisms about it requiring a lot of energy and not scaling well, but has been proven to work over 10 year.
- POS (Proof of Stake):  Represented by peercoin, all nodes can mine block, but the reward depends on the coin-age. The more coin you hold, the more rewards you get. It also faces the high performance problem.
- DPOS (Delegated Proof of Stake): Represented by eos, the community votes to select 21 super nodes to exercise the block writing rights. When there is a evil node, the deposit will be confiscated. 


&emsp;&emsp; NEO has implemented a delegated Byzantine Fault Tolerant algorithm, provides fault tolerance of `f = ⌊ (n-1) / 3 ⌋` nodes, and 1, 000TPS transaction throughput in mainnet network, which is possiable to reach 10, 000TPS in the future, for supporting large-scale commercial applications. Different from DPoS, NEO stakeholders can vote and select the consensus nodes in neo network at any time. Consensus nodes packet a new block in turn which overcomes low perfomance without all nodes mining in PoS, and get the block's network fee GAS as rewards. 