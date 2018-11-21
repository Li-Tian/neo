using Microsoft.EntityFrameworkCore;
using Neo.Cryptography;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;

namespace Neo.Wallets.SQLite
{
    /// <summary>
    /// UserWallet类是Wallet类的子类，是NEO钱包db3格式的实现
    /// </summary>
    public class UserWallet : Wallet
    {
        /// <summary>
        /// 钱包交易的委托，在收到交易时，调用绑定的方法
        /// </summary>
        public override event EventHandler<WalletTransactionEventArgs> WalletTransaction;

        private readonly object db_lock = new object();
        private readonly WalletIndexer indexer;
        private readonly string path;
        private readonly byte[] iv;
        private readonly byte[] masterKey;
        private readonly Dictionary<UInt160, UserWalletAccount> accounts;
        private readonly Dictionary<UInt256, Transaction> unconfirmed = new Dictionary<UInt256, Transaction>();
        /// <summary>
        /// 钱包名称，由db3钱包文件的文件名提供
        /// </summary>
        public override string Name => Path.GetFileNameWithoutExtension(path);
        /// <summary>
        /// 钱包高度，由钱包索引提供
        /// </summary>
        public override uint WalletHeight => indexer.IndexHeight;
        /// <summary>
        /// 钱包版本，从db3钱包文件中读取Version字段获得。
        /// </summary>
        public override Version Version
        {
            get
            {
                byte[] buffer = LoadStoredData("Version");
                if (buffer == null || buffer.Length < 16) return new Version(0, 0);
                int major = buffer.ToInt32(0);
                int minor = buffer.ToInt32(4);
                int build = buffer.ToInt32(8);
                int revision = buffer.ToInt32(12);
                return new Version(major, minor, build, revision);
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="indexer">钱包索引对象</param>
        /// <param name="path">db3文件路径</param>
        /// <param name="passwordKey">用户输入的钱包的密码</param>
        /// <param name="create">构造标志位，true代表首次创建钱包文件，
        ///                      并将钱包对象的相关信息存入钱包文件，
        ///                      false代表，读取已有钱包文件，直接使用
        ///                      钱包文件内相关信息构建钱包对象</param>
        /// <remarks>
        /// 钱包内存储的相关字段：
        /// IV：
        /// PasswordHash：
        /// MasterKey：
        /// Version：
        /// </remarks>
        private UserWallet(WalletIndexer indexer, string path, byte[] passwordKey, bool create)
        {
            this.indexer = indexer;
            this.path = path;
            if (create)
            {
                this.iv = new byte[16];
                this.masterKey = new byte[32];
                this.accounts = new Dictionary<UInt160, UserWalletAccount>();
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                    rng.GetBytes(masterKey);
                }
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                BuildDatabase();
                SaveStoredData("PasswordHash", passwordKey.Sha256());
                SaveStoredData("IV", iv);
                SaveStoredData("MasterKey", masterKey.AesEncrypt(passwordKey, iv));
                SaveStoredData("Version", new[] { version.Major, version.Minor, version.Build, version.Revision }.Select(p => BitConverter.GetBytes(p)).SelectMany(p => p).ToArray());
            }
            else
            {
                byte[] passwordHash = LoadStoredData("PasswordHash");
                if (passwordHash != null && !passwordHash.SequenceEqual(passwordKey.Sha256()))
                    throw new CryptographicException();
                this.iv = LoadStoredData("IV");
                this.masterKey = LoadStoredData("MasterKey").AesDecrypt(passwordKey, iv);
                this.accounts = LoadAccounts();
                indexer.RegisterAccounts(accounts.Keys);
            }
            indexer.WalletTransaction += WalletIndexer_WalletTransaction;
        }

        private void AddAccount(UserWalletAccount account, bool is_import)
        {
            lock (accounts)
            {
                if (accounts.TryGetValue(account.ScriptHash, out UserWalletAccount account_old))
                {
                    if (account.Contract == null)
                    {
                        account.Contract = account_old.Contract;
                    }
                }
                else
                {
                    indexer.RegisterAccounts(new[] { account.ScriptHash }, is_import ? 0 : Blockchain.Singleton.Height);
                }
                accounts[account.ScriptHash] = account;
            }
            lock (db_lock)
                using (WalletDataContext ctx = new WalletDataContext(path))
                {
                    if (account.HasKey)
                    {
                        byte[] decryptedPrivateKey = new byte[96];
                        Buffer.BlockCopy(account.Key.PublicKey.EncodePoint(false), 1, decryptedPrivateKey, 0, 64);
                        Buffer.BlockCopy(account.Key.PrivateKey, 0, decryptedPrivateKey, 64, 32);
                        byte[] encryptedPrivateKey = EncryptPrivateKey(decryptedPrivateKey);
                        Array.Clear(decryptedPrivateKey, 0, decryptedPrivateKey.Length);
                        Account db_account = ctx.Accounts.FirstOrDefault(p => p.PublicKeyHash.SequenceEqual(account.Key.PublicKeyHash.ToArray()));
                        if (db_account == null)
                        {
                            db_account = ctx.Accounts.Add(new Account
                            {
                                PrivateKeyEncrypted = encryptedPrivateKey,
                                PublicKeyHash = account.Key.PublicKeyHash.ToArray()
                            }).Entity;
                        }
                        else
                        {
                            db_account.PrivateKeyEncrypted = encryptedPrivateKey;
                        }
                    }
                    if (account.Contract != null)
                    {
                        Contract db_contract = ctx.Contracts.FirstOrDefault(p => p.ScriptHash.SequenceEqual(account.Contract.ScriptHash.ToArray()));
                        if (db_contract != null)
                        {
                            db_contract.PublicKeyHash = account.Key.PublicKeyHash.ToArray();
                        }
                        else
                        {
                            ctx.Contracts.Add(new Contract
                            {
                                RawData = ((VerificationContract)account.Contract).ToArray(),
                                ScriptHash = account.Contract.ScriptHash.ToArray(),
                                PublicKeyHash = account.Key.PublicKeyHash.ToArray()
                            });
                        }
                    }
                    //add address
                    {
                        Address db_address = ctx.Addresses.FirstOrDefault(p => p.ScriptHash.SequenceEqual(account.Contract.ScriptHash.ToArray()));
                        if (db_address == null)
                        {
                            ctx.Addresses.Add(new Address
                            {
                                ScriptHash = account.Contract.ScriptHash.ToArray()
                            });
                        }
                    }
                    ctx.SaveChanges();
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

        private void BuildDatabase()
        {
            using (WalletDataContext ctx = new WalletDataContext(path))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }
        }
        /// <summary>
        /// 变更db3钱包的密码
        /// </summary>
        /// <param name="password_old">旧密码</param>
        /// <param name="password_new">新密码</param>
        /// <returns>旧密码验证错误时，返回false,否则返回true</returns>
        public bool ChangePassword(string password_old, string password_new)
        {
            if (!VerifyPassword(password_old)) return false;
            byte[] passwordKey = password_new.ToAesKey();
            try
            {
                SaveStoredData("PasswordHash", passwordKey.Sha256());
                SaveStoredData("MasterKey", masterKey.AesEncrypt(passwordKey, iv));
                return true;
            }
            finally
            {
                Array.Clear(passwordKey, 0, passwordKey.Length);
            }
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
        /// 创建db3钱包对象
        /// </summary>
        /// <param name="indexer">钱包索引</param>
        /// <param name="path">钱包文件路径</param>
        /// <param name="password">钱包密码（普通字符串）</param>
        /// <returns>创建的钱包对象</returns>
        public static UserWallet Create(WalletIndexer indexer, string path, string password)
        {
            return new UserWallet(indexer, path, password.ToAesKey(), true);
        }
        /// <summary>
        /// 创建db3钱包对象
        /// </summary>
        /// <param name="indexer">钱包索引</param>
        /// <param name="path">钱包文件路径</param>
        /// <param name="password">钱包密码（安全字符串）</param>
        /// <returns>创建的钱包对象</returns>
        public static UserWallet Create(WalletIndexer indexer, string path, SecureString password)
        {
            return new UserWallet(indexer, path, password.ToAesKey(), true);
        }
        /// <summary>
        /// 通过私钥创建账户
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <returns>创建的钱包账户对象</returns>
        public override WalletAccount CreateAccount(byte[] privateKey)
        {
            KeyPair key = new KeyPair(privateKey);
            VerificationContract contract = new VerificationContract
            {
                Script = SmartContract.Contract.CreateSignatureRedeemScript(key.PublicKey),
                ParameterList = new[] { ContractParameterType.Signature }
            };
            UserWalletAccount account = new UserWalletAccount(contract.ScriptHash)
            {
                Key = key,
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
        public override WalletAccount CreateAccount(SmartContract.Contract contract, KeyPair key = null)
        {
            VerificationContract verification_contract = contract as VerificationContract;
            if (verification_contract == null)
            {
                verification_contract = new VerificationContract
                {
                    Script = contract.Script,
                    ParameterList = contract.ParameterList
                };
            }
            UserWalletAccount account = new UserWalletAccount(verification_contract.ScriptHash)
            {
                Key = key,
                Contract = verification_contract
            };
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
            UserWalletAccount account = new UserWalletAccount(scriptHash);
            AddAccount(account, true);
            return account;
        }

        private byte[] DecryptPrivateKey(byte[] encryptedPrivateKey)
        {
            if (encryptedPrivateKey == null) throw new ArgumentNullException(nameof(encryptedPrivateKey));
            if (encryptedPrivateKey.Length != 96) throw new ArgumentException();
            return encryptedPrivateKey.AesDecrypt(masterKey, iv);
        }
        /// <summary>
        /// 删除钱包账户列表内指定账户对象，并删除db3文件中的相关数据
        /// </summary>
        /// <param name="scriptHash">指定账户对象的脚本哈希</param>
        /// <returns>账户列表内存在指定账户时返回true,否则返回false</returns>
        public override bool DeleteAccount(UInt160 scriptHash)
        {
            UserWalletAccount account;
            lock (accounts)
            {
                if (accounts.TryGetValue(scriptHash, out account))
                    accounts.Remove(scriptHash);
            }
            if (account != null)
            {
                indexer.UnregisterAccounts(new[] { scriptHash });
                lock (db_lock)
                    using (WalletDataContext ctx = new WalletDataContext(path))
                    {
                        if (account.HasKey)
                        {
                            Account db_account = ctx.Accounts.First(p => p.PublicKeyHash.SequenceEqual(account.Key.PublicKeyHash.ToArray()));
                            ctx.Accounts.Remove(db_account);
                        }
                        if (account.Contract != null)
                        {
                            Contract db_contract = ctx.Contracts.First(p => p.ScriptHash.SequenceEqual(scriptHash.ToArray()));
                            ctx.Contracts.Remove(db_contract);
                        }
                        //delete address
                        {
                            Address db_address = ctx.Addresses.First(p => p.ScriptHash.SequenceEqual(scriptHash.ToArray()));
                            ctx.Addresses.Remove(db_address);
                        }
                        ctx.SaveChanges();
                    }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 回收方法，删除钱包交易委托上绑定的事件
        /// </summary>
        public override void Dispose()
        {
            indexer.WalletTransaction -= WalletIndexer_WalletTransaction;
        }

        private byte[] EncryptPrivateKey(byte[] decryptedPrivateKey)
        {
            return decryptedPrivateKey.AesEncrypt(masterKey, iv);
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
                accounts.TryGetValue(scriptHash, out UserWalletAccount account);
                return account;
            }
        }
        /// <summary>
        /// 获取db3钱包账户列表内所有的账户对象
        /// </summary>
        /// <returns>账户对象的集合</returns>
        public override IEnumerable<WalletAccount> GetAccounts()
        {
            lock (accounts)
            {
                foreach (UserWalletAccount account in accounts.Values)
                    yield return account;
            }
        }

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

        private Dictionary<UInt160, UserWalletAccount> LoadAccounts()
        {
            using (WalletDataContext ctx = new WalletDataContext(path))
            {
                Dictionary<UInt160, UserWalletAccount> accounts = ctx.Addresses.Select(p => p.ScriptHash).AsEnumerable().Select(p => new UserWalletAccount(new UInt160(p))).ToDictionary(p => p.ScriptHash);
                foreach (Contract db_contract in ctx.Contracts.Include(p => p.Account))
                {
                    VerificationContract contract = db_contract.RawData.AsSerializable<VerificationContract>();
                    UserWalletAccount account = accounts[contract.ScriptHash];
                    account.Contract = contract;
                    account.Key = new KeyPair(DecryptPrivateKey(db_contract.Account.PrivateKeyEncrypted));
                }
                return accounts;
            }
        }
        private byte[] LoadStoredData(string name)
        {
            using (WalletDataContext ctx = new WalletDataContext(path))
            {
                return ctx.Keys.FirstOrDefault(p => p.Name == name)?.Value;
            }
        }
        /// <summary>
        /// 打开钱包，并返回一个db3钱包对象
        /// </summary>
        /// <param name="indexer">钱包索引</param>
        /// <param name="path">db3钱包文件路径</param>
        /// <param name="password">钱包密码（普通字符串）</param>
        /// <returns>生成的钱包对象</returns>
        public static UserWallet Open(WalletIndexer indexer, string path, string password)
        {
            return new UserWallet(indexer, path, password.ToAesKey(), false);
        }
        /// <summary>
        /// 打开钱包，并返回一个db3钱包对象
        /// </summary>
        /// <param name="indexer">钱包索引</param>
        /// <param name="path">db3钱包文件路径</param>
        /// <param name="password">钱包密码（安全字符串）</param>
        /// <returns>生成的钱包对象</returns>
        public static UserWallet Open(WalletIndexer indexer, string path, SecureString password)
        {
            return new UserWallet(indexer, path, password.ToAesKey(), false);
        }

        private void SaveStoredData(string name, byte[] value)
        {
            lock (db_lock)
                using (WalletDataContext ctx = new WalletDataContext(path))
                {
                    SaveStoredData(ctx, name, value);
                    ctx.SaveChanges();
                }
        }
        private static void SaveStoredData(WalletDataContext ctx, string name, byte[] value)
        {
            Key key = ctx.Keys.FirstOrDefault(p => p.Name == name);
            if (key == null)
            {
                ctx.Keys.Add(new Key
                {
                    Name = name,
                    Value = value
                });
            }
            else
            {
                key.Value = value;
            }
        }
        /// <summary>
        /// 验证钱包密码
        /// </summary>
        /// <param name="password">用户输入的密码</param>
        /// <returns>验证通过为true，失败为false</returns>
        public override bool VerifyPassword(string password)
        {
            return password.ToAesKey().Sha256().SequenceEqual(LoadStoredData("PasswordHash"));
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
