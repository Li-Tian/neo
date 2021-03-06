﻿using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using System;

namespace Neo.Wallets
{
    /// <summary>
    /// 一个可以使用的 UTXO 资产。它可以通过 CoinReference来描述，
    /// 也可以通过 TransactionOutput来描述，两者指向同一个实体。
    /// </summary>
    public class Coin : IEquatable<Coin>
    {
        /// <summary>
        /// Coin的引用，即交易输入
        /// </summary>
        public CoinReference Reference;

        /// <summary>
        /// 交易输出
        /// </summary>
        public TransactionOutput Output;

        /// <summary>
        /// 用一个CoinState对象来表示这个Coin的状态
        /// </summary>
        public CoinState State;

        private string _address = null;

        /// <summary>
        /// 返回这个Coin的TransactionOutput的地址
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

        /// <summary>
        ///  判断两个Coin对象是否相等
        /// </summary>
        /// <param name="other">等待比较的Coin对象</param>
        /// <returns>
        /// 如果两个Coin对象的Reference相等，返回<c>true</c>.<br/>
        /// 如果被比较的Coin对象是null， 返回<c>false</c>.<br/>
        /// 否则，根据比较两个Coin的Reference来返回是否相等<br/>
        /// </returns>
        public bool Equals(Coin other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return Reference.Equals(other.Reference);
        }

    
        /// <summary>
        /// 判断两个Coin对象是否相等
        /// </summary>
        /// <param name="obj">等待比较的Coin对象</param>
        /// <returns>如果两个Coin相等返回<c>true</c>, 否则返回<c>false</c></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Coin);
        }

        /// <summary>
        /// 返回一个由Reference产生的HashCode
        /// </summary>
        /// <returns>返回一个HashCode</returns>
        public override int GetHashCode()
        {
            return Reference.GetHashCode();
        }
    }
}
