<center><h2> Merkle Tree </h2></center>

　Merkle tree is such a kind of binary tree: it's able to quickly check & induce massive data, and verify the completeness of block transaction records. NEO uses Merkle tree to construct block model. Different from Bitcoin, NEO's block head stores the Merkle root of all transactions within the block. Block data area stores transaction array.

[![MerkleTree01](../../images/blockchain_paradigm/MerkleTree01.png)](../../images/blockchain_paradigm/MerkleTree01.png)

Attribute of Merkle tree：

  1. Merkle tree is majorily binary tree, with all features of tree structure.

  2. Merkle tree's leaf nodes' value is unit data of data set, or unit data HASH.

  3. Non-leaf nodes' value is computed by Hash method according to all child nodes' value.

Transaction verification deduction:

　Transcation001's validity can be verified by comparing original Top Hash value with the value computed from Transcation001, Transcation002 and Hash1.


Scenarios：

1. A Merkle tree root is maintained upon block head creating.

2. Use SPV wallet to verify block data.

Reference:

1. <https://en.wikipedia.org/wiki/Merkle_tree>

> [!NOTE]
> In case of dead links, please contact <feedback@neo.org>
