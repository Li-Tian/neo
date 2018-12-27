using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Cryptography
{
    // <summary>
    // 梅克尔树的实现类
    // </summary>
    /// <summary>
    /// MerkleTree implement class
    /// </summary>
    public class MerkleTree
    {
        private MerkleTreeNode root;
        // <summary>
        // MerkleTree的深度
        // </summary>
        /// <summary>
        /// MerkleTree depth
        /// </summary>
        public int Depth { get; private set; }

        internal MerkleTree(UInt256[] hashes)
        {
            if (hashes.Length == 0) throw new ArgumentException();
            this.root = Build(hashes.Select(p => new MerkleTreeNode { Hash = p }).ToArray());
            int depth = 1;
            for (MerkleTreeNode i = root; i.LeftChild != null; i = i.LeftChild)
                depth++;
            this.Depth = depth;
        }

        private static MerkleTreeNode Build(MerkleTreeNode[] leaves)
        {
            if (leaves.Length == 0) throw new ArgumentException();
            if (leaves.Length == 1) return leaves[0];
            MerkleTreeNode[] parents = new MerkleTreeNode[(leaves.Length + 1) / 2];
            for (int i = 0; i < parents.Length; i++)
            {
                parents[i] = new MerkleTreeNode();
                parents[i].LeftChild = leaves[i * 2];
                leaves[i * 2].Parent = parents[i];
                if (i * 2 + 1 == leaves.Length)
                {
                    parents[i].RightChild = parents[i].LeftChild;
                }
                else
                {
                    parents[i].RightChild = leaves[i * 2 + 1];
                    leaves[i * 2 + 1].Parent = parents[i];
                }
                parents[i].Hash = new UInt256(Crypto.Default.Hash256(parents[i].LeftChild.Hash.ToArray().Concat(parents[i].RightChild.Hash.ToArray()).ToArray()));
            }
            return Build(parents); //TailCall
        }
        // <summary>
        // 传入所有交易的哈希值, 构建MerkleTree, 并返回构建后根节点的哈希值
        // </summary>
        // <param name="hashes">用来构建Merkle树的交易哈希数组</param>
        // <exception cref="ArgumentException">如果没有数据拿来构建Merkle树，则抛出该异常</exception>
        // <returns>返回MerkleTree根节点的哈希值</returns>
        /// <summary>
        /// Passing in a transcation hashes array ,use it to build a MerkleTree, and return the root hash of MerkleTree
        /// </summary>
        /// <param name="hashes">a transcation hashes array</param>
        /// <exception cref="ArgumentException">if the length of the transcation hashes array is 0</exception>
        /// <returns>root hash of MerkleTree</returns>
        public static UInt256 ComputeRoot(UInt256[] hashes)
        {
            if (hashes.Length == 0) throw new ArgumentException();
            if (hashes.Length == 1) return hashes[0];
            MerkleTree tree = new MerkleTree(hashes);
            return tree.root.Hash;
        }

        private static void DepthFirstSearch(MerkleTreeNode node, IList<UInt256> hashes)
        {
            if (node.LeftChild == null)
            {
                // if left is null, then right must be null
                hashes.Add(node.Hash);
            }
            else
            {
                DepthFirstSearch(node.LeftChild, hashes);
                DepthFirstSearch(node.RightChild, hashes);
            }
        }

        // <summary>
        // 通过深度优先搜索算法将一个Merkle树的所有叶节点的哈希值转换成数组返回
        // </summary>
        // <returns>所有叶节点的交易哈希值构成的数组</returns>
        // depth-first order
        /// <summary>
        /// Convert hashes  of all leaf nodes of a Merkle tree to a hash array by invoking  DepthFirstSearch method
        /// </summary>
        /// <returns>a hash array</returns>
        public UInt256[] ToHashArray()
        {
            List<UInt256> hashes = new List<UInt256>();
            DepthFirstSearch(root, hashes);
            return hashes.ToArray();
        }

        // <summary> 
        // 根据标志位修剪梅克尔树。flags为所有叶节点的标志位。从叶子节点向上检测。<br/> 
        // 1.对所有高度为2的节点，如果其左子节点和右子节点的标志位都为false,将该节点的左子节点和右子节点置为null。检测完成后进入第二步；<br/> 
        // 2.对所有高度为3的节点，如果其左子节点的左子节点和其右子节点的右子节点都为null时，将该节点的左子节点和右子节点都置为null；<br/> 
        // 3.对高度为4的节点继续执行步骤2，依次类推，一直到根节点；<br/> 
        // 4.完成后便得到修剪后的梅克尔树。 
        // </summary> 
        // <param name="flags">BitArray标志位</param>
        /// <summary>     
        /// Trim the Merkel tree according to flags. Dlags is the flag set of all leaf nodes. Detect from the leaf node up.
        /// 1.For all nodes with deepth of 2, if the flags of their left and right children are both false, the left and right children of the node are set to null. After that, the second step is entered;<br/> 
        /// 2.For all nodes with a height of 3, if the left child of its left child and the right child of its right child are both null, the left and right children of the node are both set to Null;<br/>
        /// 3.Continue to step 2 for nodes with deepth of 4, and so on, up to the root node;<br/> 
        /// 4.Upon completion, the trimmed Merkel tree is obtained.
        /// </summary> 
        /// <param name="flags">BitArray flags</param>
        public void Trim(BitArray flags)
        {
            flags = new BitArray(flags);
            flags.Length = 1 << (Depth - 1);
            Trim(root, 0, Depth, flags);
        }

        private static void Trim(MerkleTreeNode node, int index, int depth, BitArray flags)
        {
            if (depth == 1) return;
            if (node.LeftChild == null) return; // if left is null, then right must be null
            if (depth == 2)
            {
                if (!flags.Get(index * 2) && !flags.Get(index * 2 + 1))
                {
                    node.LeftChild = null;
                    node.RightChild = null;
                }
            }
            else
            {
                Trim(node.LeftChild, index * 2, depth - 1, flags);
                Trim(node.RightChild, index * 2 + 1, depth - 1, flags);
                if (node.LeftChild.LeftChild == null && node.RightChild.RightChild == null)
                {
                    node.LeftChild = null;
                    node.RightChild = null;
                }
            }
        }
    }
}
