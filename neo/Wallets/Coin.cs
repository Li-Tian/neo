using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using System;

namespace Neo.Wallets
{
    // <summary>
    // 一个可以使用的 UTXO 资产。它可以通过 CoinReference来描述，
    // 也可以通过 TransactionOutput来描述，两者指向同一个实体。
    // </summary>
    /// <summary>
    /// a usable asset。It could be descripted by CoinReference,
    /// and be descripted by TransactionOutput.They both point to the same entity
    /// </summary>
    public class Coin : IEquatable<Coin>
    {
        // <summary>
        // Coin的引用，即交易输入
        // </summary>
        /// <summary>
        /// The reference to the Coin，transcation input
        /// </summary>
        public CoinReference Reference;

        // <summary>
        // 交易输出
        // </summary>
        /// <summary>
        /// transcation output
        /// </summary>
        public TransactionOutput Output;

        // <summary>
        // 用一个CoinState对象来表示这个Coin的状态
        // </summary>
        /// <summary>
        /// use a CoinState object to represent the state of the Coin
        /// </summary>
        public CoinState State;

        private string _address = null;

        // <summary>
        // 返回这个Coin的TransactionOutput的地址
        // </summary>
        /// <summary>
        /// return TransactionOutput Address of the Coin
        /// </summary>
        public string Address
        {
            get
            {
                if (_address == null)
                {
                    _address = Output.ScriptHash.ToAddress();
                }
                return _address;
            }
        }

        // <summary>
        //  判断两个Coin对象是否相等
        // </summary>
        // <param name="other">等待比较的Coin对象</param>
        // <returns>
        // 如果两个Coin对象的Reference相等，返回<c>true</c>.<br/>
        // 如果被比较的Coin对象是null， 返回<c>false</c>.<br/>
        // 否则，根据比较两个Coin的Reference来返回是否相等<br/>
        // </returns>
        /// <summary>
        ///  Determine if two Coin objects are equal
        /// </summary>
        /// <param name="other">Coin object to be compared</param>
        /// <returns>
        /// If two Coin objects are equal，return <c>true</c>.<br/>
        /// If Coin object to be compared is null,return <c>false</c>.<br/>
        /// Otherwise，return the comparisons between two Coin objects<br/>
        /// </returns>
        public bool Equals(Coin other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return Reference.Equals(other.Reference);
        }


        // <summary>
        // 判断两个Coin对象是否相等
        // </summary>
        // <param name="obj">等待比较的Coin对象</param>
        // <returns>如果两个Coin相等返回<c>true</c>, 否则返回<c>false</c></returns>
        /// <summary>
        /// Determine if Coin object and another object are equal
        /// </summary>
        /// <param name="obj">another object to be compared</param>
        /// <returns>
        /// If Coin object and another object are equal,return true<br/>
        /// Otherwise，return false<br/>
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Coin);
        }

        // <summary>
        // 返回一个由Reference产生的HashCode
        // </summary>
        // <returns>返回一个HashCode</returns>
        /// <summary>
        /// return HashCode of the Reference
        /// </summary>
        /// <returns>return HashCode of the Reference</returns>
        public override int GetHashCode()
        {
            return Reference.GetHashCode();
        }
    }
}
