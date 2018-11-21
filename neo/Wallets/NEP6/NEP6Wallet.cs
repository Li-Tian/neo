using Neo.IO.Json;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UserWallet = Neo.Wallets.SQLite.UserWallet;

namespace Neo.Wallets.NEP6
{
    /// <summary>
    /// NEP6Wallet是Wallet类的子类，是钱包满足NEP6规范的实现
    /// </summary>
    public class NEP6Wallet : Wallet
    {
        /// <summary>
        /// 钱包交易的委托，在收到交易时，调用绑定的方法
        /// </summary>
        public override event EventHandler<WalletTransactionEventArgs> WalletTransaction;

        private readonly WalletIndexer indexer;
        private readonly string path;
        private string password;
        private string name;
        private Version version;
        /// <summary>
        /// NEP6钱包使用Scrypt算法加密解密NEP2密文所需的参数
        /// </summary>
        public readonly ScryptParameters Scrypt;
        private readonly Dictionary<UInt160, NEP6Account> accounts;
        private readonly JObject extra;
        private readonly Dictionary<UInt256, Transaction> unconfirmed = new Dictionary<UInt256, Transaction>();
        /// <summary>
        /// 钱包的名称
        /// </summary>
        public override string Name => name;
        /// <summary>
        /// 钱包的版本
        /// </summary>
        public override Version Version => version;
        /// <summary>
        /// 钱包高度，由钱包索引提供
        /// </summary>
        public override uint WalletHeight => indexer.IndexHeight;
        /// <summary>
        /// 构造方法，构造NEP6钱包对象，并向钱包索引中注册其包含的钱包账户
        /// </summary>
        /// <param name="indexer">钱包索引</param>
        /// <param name="path">钱包文件的路径</param>
        /// <param name="name">钱包名称</param>
        public NEP6Wallet(WalletIndexer indexer, string path, string name = null)
        {
            this.indexer = indexer;
            this.path = path;
            if (File.Exists(path))
            {
                JObject wallet;
                using (StreamReader reader = new StreamReader(path))
                {
                    wallet = JObject.Parse(reader);
                }
                this.name = wallet["name"]?.AsString();
                this.version = Version.Parse(wallet["version"].AsString());
                this.Scrypt = ScryptParameters.FromJson(wallet["scrypt"]);
                this.accounts = ((JArray)wallet["accounts"]).Select(p => NEP6Account.FromJson(p, this)).ToDictionary(p => p.ScriptHash);
                this.extra = wallet["extra"];
                indexer.RegisterAccounts(accounts.Keys);
            }
            else
            {
                this.name = name;
                this.version = Version.Parse("1.0");
                this.Scrypt = ScryptParameters.Default;
                this.accounts = new Dictionary<UInt160, NEP6Account>();
                this.extra = JObject.Null;
            }
            indexer.WalletTransaction += WalletIndexer_WalletTransaction;
        }

        private void AddAccount(NEP6Account account, bool is_import)
        {
            lock (accounts)
            {
                if (accounts.TryGetValue(account.ScriptHash, out NEP6Account account_old))
                {
                    account.Label = account_old.Label;
                    account.IsDefault = account_old.IsDefault;
                    account.Lock = account_old.Lock;
                    if (account.Contract == null)
                    {
                        account.Contract = account_old.Contract;
                    }
                    else
                    {
                        NEP6Contract contract_old = (NEP6Contract)account_old.Contract;
                        if (contract_old != null)
                        {
                            NEP6Contract contract = (NEP6Contract)account.Contract;
                            contract.ParameterNames = contract_old.ParameterNames;
                            contract.Deployed = contract_old.Deployed;
                        }
                    }
                    account.Extra = account_old.Extra;
                }
                else
                {
                    indexer.RegisterAccounts(new[] { account.ScriptHash }, is_import ? 0 : Blockchain.Singleton.Height);
                }
                accounts[account.ScriptHash] = account;
            }
        }

        public override void ApplyTransaction(Transaction tx)
        {
            lock (unconfirmed)
            {
                unconfirmed[tx.Hash] = tx;
            }
            WalletTransaction?.Invoke(this, new WalletTransactionEventArgs
            {
                Transaction = tx,
                RelatedAccounts = tx.Witnesses.Select(p => p.ScriptHash).Union(tx.Outputs.Select(p => p.ScriptHash)).Where(p => Contains(p)).ToArray(),
                Height = null,
                Time = DateTime.UtcNow.ToTimestamp()
            });
        }
        /// <summary>
        /// 判断钱包账户列表内是否存在指定的账户
        /// </summary>
        /// <param name="scriptHash">指定账户的脚本哈希值</param>
        /// <returns>存在返回true，否则返回false</returns>
        public override bool Contains(UInt160 scriptHash)
        {
            lock (accounts)
            {
                return accounts.ContainsKey(scriptHash);
            }
        }
        /// <summary>
        /// 通过私钥创建账户
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <returns>创建的钱包账户对象</returns>
        public override WalletAccount CreateAccount(byte[] privateKey)
        {
            KeyPair key = new KeyPair(privateKey);
            NEP6Contract contract = new NEP6Contract
            {
                Script = Contract.CreateSignatureRedeemScript(key.PublicKey),
                ParameterList = new[] { ContractParameterType.Signature },
                ParameterNames = new[] { "signature" },
                Deployed = false
            };
            NEP6Account account = new NEP6Account(this, contract.ScriptHash, key, password)
            {
                Contract = contract
            };
            AddAccount(account, false);
            return account;
        }
        /// <summary>
        /// 通过合约对象和密钥对创建钱包对象
        /// </summary>
        /// <param name="contract">合约对象</param>
        /// <param name="key">密钥对</param>
        /// <returns>创建的钱包账户对象</returns>
        public override WalletAccount CreateAccount(Contract contract, KeyPair key = null)
        {
            NEP6Contract nep6contract = contract as NEP6Contract;
            if (nep6contract == null)
            {
                nep6contract = new NEP6Contract
                {
                    Script = contract.Script,
                    ParameterList = contract.ParameterList,
                    ParameterNames = contract.ParameterList.Select((p, i) => $"parameter{i}").ToArray(),
                    Deployed = false
                };
            }
            NEP6Account account;
            if (key == null)
                account = new NEP6Account(this, nep6contract.ScriptHash);
            else
                account = new NEP6Account(this, nep6contract.ScriptHash, key, password);
            account.Contract = nep6contract;
            AddAccount(account, false);
            return account;
        }
        /// <summary>
        /// 利用脚本哈希创建钱包账户
        /// </summary>
        /// <param name="scriptHash">脚本哈希</param>
        /// <returns>创建的钱包账户对象</returns>
        public override WalletAccount CreateAccount(UInt160 scriptHash)
        {
            NEP6Account account = new NEP6Account(this, scriptHash);
            AddAccount(account, true);
            return account;
        }
        /// <summary>
        /// 解密NEP2格式密文，生成密钥对
        /// </summary>
        /// <param name="nep2key">NEP2格式密文</param>
        /// <returns>解密出的密钥对</returns>
        public KeyPair DecryptKey(string nep2key)
        {
            return new KeyPair(GetPrivateKeyFromNEP2(nep2key, password, Scrypt.N, Scrypt.R, Scrypt.P));
        }
        /// <summary>
        /// 利用脚本哈希从账户列表中删除指定账户对象
        /// </summary>
        /// <param name="scriptHash">指定账户对象对应的脚本哈希</param>
        /// <returns></returns>
        public override bool DeleteAccount(UInt160 scriptHash)
        {
            bool removed;
            lock (accounts)
            {
                removed = accounts.Remove(scriptHash);
            }
            if (removed)
            {
                indexer.UnregisterAccounts(new[] { scriptHash });
            }
            return removed;
        }
        /// <summary>
        /// 回收方法，删除钱包交易委托上绑定的事件
        /// </summary>
        public override void Dispose()
        {
            indexer.WalletTransaction -= WalletIndexer_WalletTransaction;
        }
        /// <summary>
        ///  查询指定账户集合内某一全局资产（neo、gas）所有未花费的Coin集合中满足指定金额的子集(按照降序查找、优先使用鉴权合约地址（普通地址）)
        /// </summary>
        /// <param name="asset_id">指定全局资产的ID</param>
        /// <param name="amount">指定金额</param>
        /// <param name="from">指定账户集合</param>
        /// <returns>查询到的Coin的集合</returns>
        public override Coin[] FindUnspentCoins(UInt256 asset_id, Fixed8 amount, UInt160[] from)
        {
            return FindUnspentCoins(FindUnspentCoins(from).ToArray().Where(p => GetAccount(p.Output.ScriptHash).Contract.Script.IsSignatureContract()), asset_id, amount) ?? base.FindUnspentCoins(asset_id, amount, from);
        }
        /// <summary>
        /// 从钱包账户列表内查找指定账户对象
        /// </summary>
        /// <param name="scriptHash">指定账户的脚本哈希</param>
        /// <returns>指定账户对象</returns>
        public override WalletAccount GetAccount(UInt160 scriptHash)
        {
            lock (accounts)
            {
                accounts.TryGetValue(scriptHash, out NEP6Account account);
                return account;
            }
        }
        /// <summary>
        /// 获取NEP6钱包账户列表内所有的账户对象
        /// </summary>
        /// <returns>账户对象的集合</returns>
        public override IEnumerable<WalletAccount> GetAccounts()
        {
            lock (accounts)
            {
                foreach (NEP6Account account in accounts.Values)
                    yield return account;
            }
        }
        /// <summary>
        /// 获取指定账户集合所所持有的Coin的集合
        /// </summary>
        /// <param name="accounts">指定账户集合</param>
        /// <returns></returns>
        public override IEnumerable<Coin> GetCoins(IEnumerable<UInt160> accounts)
        {
            if (unconfirmed.Count == 0)
                return indexer.GetCoins(accounts);
            else
                return GetCoinsInternal();
            IEnumerable<Coin> GetCoinsInternal()
            {
                HashSet<CoinReference> inputs, claims;
                Coin[] coins_unconfirmed;
                lock (unconfirmed)
                {
                    inputs = new HashSet<CoinReference>(unconfirmed.Values.SelectMany(p => p.Inputs));
                    claims = new HashSet<CoinReference>(unconfirmed.Values.OfType<ClaimTransaction>().SelectMany(p => p.Claims));
                    coins_unconfirmed = unconfirmed.Values.Select(tx => tx.Outputs.Select((o, i) => new Coin
                    {
                        Reference = new CoinReference
                        {
                            PrevHash = tx.Hash,
                            PrevIndex = (ushort)i
                        },
                        Output = o,
                        State = CoinState.Unconfirmed
                    })).SelectMany(p => p).ToArray();
                }
                foreach (Coin coin in indexer.GetCoins(accounts))
                {
                    if (inputs.Contains(coin.Reference))
                    {
                        if (coin.Output.AssetId.Equals(Blockchain.GoverningToken.Hash))
                            yield return new Coin
                            {
                                Reference = coin.Reference,
                                Output = coin.Output,
                                State = coin.State | CoinState.Spent
                            };
                        continue;
                    }
                    else if (claims.Contains(coin.Reference))
                    {
                        continue;
                    }
                    yield return coin;
                }
                HashSet<UInt160> accounts_set = new HashSet<UInt160>(accounts);
                foreach (Coin coin in coins_unconfirmed)
                {
                    if (accounts_set.Contains(coin.Output.ScriptHash))
                        yield return coin;
                }
            }
        }
        /// <summary>
        /// 获取钱包索引以及未确认交易中与钱包中账户有关的交易的哈希
        /// </summary>
        /// <returns>所有有关交易的哈希的集合</returns>
        public override IEnumerable<UInt256> GetTransactions()
        {
            foreach (UInt256 hash in indexer.GetTransactions(accounts.Keys))
                yield return hash;
            lock (unconfirmed)
            {
                foreach (UInt256 hash in unconfirmed.Keys)
                    yield return hash;
            }
        }
        /// <summary>
        /// 导入钱包账户，通过X509格式数字证书
        /// </summary>
        /// <param name="cert">X509格式数字证书</param>
        /// <returns>导入的钱包账户对象</returns>
        public override WalletAccount Import(X509Certificate2 cert)
        {
            KeyPair key;
            using (ECDsa ecdsa = cert.GetECDsaPrivateKey())
            {
                key = new KeyPair(ecdsa.ExportParameters(true).D);
            }
            NEP6Contract contract = new NEP6Contract
            {
                Script = Contract.CreateSignatureRedeemScript(key.PublicKey),
                ParameterList = new[] { ContractParameterType.Signature },
                ParameterNames = new[] { "signature" },
                Deployed = false
            };
            NEP6Account account = new NEP6Account(this, contract.ScriptHash, key, password)
            {
                Contract = contract
            };
            AddAccount(account, true);
            return account;
        }
        /// <summary>
        /// 导入钱包账户，通过wif格式私钥
        /// </summary>
        /// <param name="wif">wif格式私钥</param>
        /// <returns>导入的钱包账户对象</returns>
        public override WalletAccount Import(string wif)
        {
            KeyPair key = new KeyPair(GetPrivateKeyFromWIF(wif));
            NEP6Contract contract = new NEP6Contract
            {
                Script = Contract.CreateSignatureRedeemScript(key.PublicKey),
                ParameterList = new[] { ContractParameterType.Signature },
                ParameterNames = new[] { "signature" },
                Deployed = false
            };
            NEP6Account account = new NEP6Account(this, contract.ScriptHash, key, password)
            {
                Contract = contract
            };
            AddAccount(account, true);
            return account;
        }
        /// <summary>
        /// 导入钱包账户，通过NEP2密文
        /// </summary>
        /// <param name="nep2">NEP2密文</param>
        /// <param name="passphrase">NEP2密文的密码</param>
        /// <returns>导入的钱包账户对象</returns>
        public override WalletAccount Import(string nep2, string passphrase)
        {
            KeyPair key = new KeyPair(GetPrivateKeyFromNEP2(nep2, passphrase));
            NEP6Contract contract = new NEP6Contract
            {
                Script = Contract.CreateSignatureRedeemScript(key.PublicKey),
                ParameterList = new[] { ContractParameterType.Signature },
                ParameterNames = new[] { "signature" },
                Deployed = false
            };
            NEP6Account account;
            if (Scrypt.N == 16384 && Scrypt.R == 8 && Scrypt.P == 8)
                account = new NEP6Account(this, contract.ScriptHash, nep2);
            else
                account = new NEP6Account(this, contract.ScriptHash, key, passphrase);
            account.Contract = contract;
            AddAccount(account, true);
            return account;
        }
        ///
        internal void Lock()
        {
            password = null;
        }
        /// <summary>
        /// 将db3格式钱包内数据迁移到新的NEP6钱包文件中，并生成对应的NEP6钱包对象
        /// </summary>
        /// <param name="indexer">钱包索引</param>
        /// <param name="path">新的NEP6钱包文件路径</param>
        /// <param name="db3path">db3格式钱包文件路径</param>
        /// <param name="password">密码</param>
        /// <returns>生成的NEP6钱包对象</returns>
        public static NEP6Wallet Migrate(WalletIndexer indexer, string path, string db3path, string password)
        {
            using (UserWallet wallet_old = UserWallet.Open(indexer, db3path, password))
            {
                NEP6Wallet wallet_new = new NEP6Wallet(indexer, path, wallet_old.Name);
                using (wallet_new.Unlock(password))
                {
                    foreach (WalletAccount account in wallet_old.GetAccounts())
                    {
                        wallet_new.CreateAccount(account.Contract, account.GetKey());
                    }
                }
                return wallet_new;
            }
        }
        /// <summary>
        /// 保存钱包信息，将钱包的名称、版本、scrypt参数、账户、额外数据存入钱包文件中
        /// </summary>
        public void Save()
        {
            JObject wallet = new JObject();
            wallet["name"] = name;
            wallet["version"] = version.ToString();
            wallet["scrypt"] = Scrypt.ToJson();
            wallet["accounts"] = new JArray(accounts.Values.Select(p => p.ToJson()));
            wallet["extra"] = extra;
            File.WriteAllText(path, wallet.ToString());
        }
        /// <summary>
        /// 解锁钱包
        /// </summary>
        /// <param name="password">用户输入的密码</param>
        /// <returns></returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">密码验证失败时抛出</exception>
        public IDisposable Unlock(string password)
        {
            if (!VerifyPassword(password))
                throw new CryptographicException();
            this.password = password;
            return new WalletLocker(this);
        }
        /// <summary>
        /// 验证NEP6钱包的密码
        /// </summary>
        /// <param name="password">用户输入的密码</param>
        /// <returns>验证结果，验证通过返回true,否则返回false</returns>
        /// <exception cref="System.FormatException">获取NEP6账户的密钥对失败时抛出</exception>
        public override bool VerifyPassword(string password)
        {
            lock (accounts)
            {
                NEP6Account account = accounts.Values.FirstOrDefault(p => !p.Decrypted);
                if (account == null)
                {
                    account = accounts.Values.FirstOrDefault(p => p.HasKey);
                }
                if (account == null) return true;
                if (account.Decrypted)
                {
                    return account.VerifyPassword(password);
                }
                else
                {
                    try
                    {
                        account.GetKey(password);
                        return true;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                }
            }
        }

        private void WalletIndexer_WalletTransaction(object sender, WalletTransactionEventArgs e)
        {
            lock (unconfirmed)
            {
                unconfirmed.Remove(e.Transaction.Hash);
            }
            UInt160[] relatedAccounts;
            lock (accounts)
            {
                relatedAccounts = e.RelatedAccounts.Where(p => accounts.ContainsKey(p)).ToArray();
            }
            if (relatedAccounts.Length > 0)
            {
                WalletTransaction?.Invoke(this, new WalletTransactionEventArgs
                {
                    Transaction = e.Transaction,
                    RelatedAccounts = relatedAccounts,
                    Height = e.Height,
                    Time = e.Time
                });
            }
        }
    }
}
