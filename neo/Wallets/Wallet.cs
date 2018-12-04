using Neo.Cryptography;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Wallets
{
    /// <summary>
    /// Wallet类是一个钱包抽象类，是各种钱包实现类的父类。
    /// 用于描述一个钱包所需实现的各种功能
    /// </summary>
    public abstract class Wallet : IDisposable
    {
        /// <summary>
        /// 钱包交易的委托，在收到交易时，调用绑定的方法
        /// </summary>
        public abstract event EventHandler<WalletTransactionEventArgs> WalletTransaction;

        private static readonly Random rand = new Random();
        /// <summary>
        /// 钱包名称
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// 钱包版本
        /// </summary>
        public abstract Version Version { get; }
        /// <summary>
        /// 钱包高度
        /// </summary>
        public abstract uint WalletHeight { get; }
        /// <summary>
        /// 发送交易，将交易添加进未确认交易列表，并发出通知触发委托
        /// </summary>
        /// <param name="tx">交易对象</param>
        public abstract void ApplyTransaction(Transaction tx);
        /// <summary>
        /// 判断钱包内是否存在某个脚本哈希对应的账户
        /// </summary>
        /// <param name="scriptHash">待查询的脚本哈希</param>
        /// <returns>存在，返回true,否则返回false</returns>
        public abstract bool Contains(UInt160 scriptHash);
        /// <summary>
        /// 利用私钥创建钱包账户
        /// 这是个抽象方法
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <returns>生成的钱包账户对象</returns>
        public abstract WalletAccount CreateAccount(byte[] privateKey);
        /// <summary>
        /// 利用合约对象和密钥对创建钱包账户
        /// </summary>
        /// <param name="contract">合约对象</param>
        /// <param name="key">密钥对</param>
        /// <returns>生成的钱包账户对象</returns>
        public abstract WalletAccount CreateAccount(Contract contract, KeyPair key = null);
        /// <summary>
        /// 利用脚本哈希创建钱包账户
        /// </summary>
        /// <param name="scriptHash">脚本哈希</param>
        /// <returns>生成的钱包账户对象</returns>
        public abstract WalletAccount CreateAccount(UInt160 scriptHash);
        /// <summary>
        /// 根据脚本哈希删除钱包内对应的账户对象
        /// 这是个抽象方法
        /// </summary>
        /// <param name="scriptHash">指定账户的脚本哈希</param>
        /// <returns>删除成功返回true,否则返回false</returns>
        public abstract bool DeleteAccount(UInt160 scriptHash);
        /// <summary>
        /// 根据脚本哈希获取钱包内对应的账户对象
        /// 这是个抽象方法
        /// </summary>
        /// <param name="scriptHash">指定账户的脚本哈希</param>
        /// <returns>获取的钱包账户对象</returns>
        public abstract WalletAccount GetAccount(UInt160 scriptHash);
        /// <summary>
        /// 获取钱包内所有的账户对象
        /// </summary>
        /// <returns>钱包内所有的账户对象</returns>
        public abstract IEnumerable<WalletAccount> GetAccounts();
        /// <summary>
        /// 获取指定账户对象集合内的Coin集合
        /// </summary>
        /// <param name="accounts">指定账户对象集合</param>
        /// <returns>查询到的Coin集合</returns>
        public abstract IEnumerable<Coin> GetCoins(IEnumerable<UInt160> accounts);
        /// <summary>
        /// 查询与钱包内账户有关的交易
        /// </summary>
        /// <returns>查询到的交易的哈希</returns>
        public abstract IEnumerable<UInt256> GetTransactions();
        /// <summary>
        /// 创建钱包账户对象，不带参数（使用随机数生成账户的私钥）
        /// </summary>
        /// <returns>生成的钱包账户对象</returns>
        public WalletAccount CreateAccount()
        {
            byte[] privateKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }
            WalletAccount account = CreateAccount(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);
            return account;
        }
        /// <summary>
        /// 利用合约对象和私钥创建账户对象
        /// </summary>
        /// <param name="contract">合约对象</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>生成的账户对象</returns>
        public WalletAccount CreateAccount(Contract contract, byte[] privateKey)
        {
            if (privateKey == null) return CreateAccount(contract);
            return CreateAccount(contract, new KeyPair(privateKey));
        }
        /// <summary>
        /// 回收方法，保留
        /// </summary>
        public virtual void Dispose()
        {
        }
        /// <summary>
        /// 查询指定账户地址集合内所有非锁定账户和非观察账户中所有未花费的Coin集合
        /// 如果指定账户地址集合为空，则取钱包账户列表内所有账户作为指定账户地址集合
        /// </summary>
        /// <param name="from">待查询账户地址集合</param>
        /// <returns>查询到的Coin集合</returns>
        public IEnumerable<Coin> FindUnspentCoins(params UInt160[] from)
        {
            IEnumerable<UInt160> accounts = from.Length > 0 ? from : GetAccounts().Where(p => !p.Lock && !p.WatchOnly).Select(p => p.ScriptHash);
            return GetCoins(accounts).Where(p => p.State.HasFlag(CoinState.Confirmed) && !p.State.HasFlag(CoinState.Spent) && !p.State.HasFlag(CoinState.Frozen));
        }
        /// <summary>
        /// 查询指定账户集合内某一全局资产（neo、gas）所有未花费的Coin集合中满足指定金额的子集(按照降序查找)
        /// </summary>
        /// <param name="asset_id">指定全局资产的ID</param>
        /// <param name="amount">指定金额</param>
        /// <param name="from">指定账户集合</param>
        /// <returns>查询到的Coin的集合</returns>
        public virtual Coin[] FindUnspentCoins(UInt256 asset_id, Fixed8 amount, params UInt160[] from)
        {
            return FindUnspentCoins(FindUnspentCoins(from), asset_id, amount);
        }
        /// <summary>
        /// 查询某一未花费Coin集合内某一全局资产（neo、gas）的Coin集合中满足指定金额的子集(按照降序查找)
        /// </summary>
        /// <param name="unspents">某未花费Coin集合</param>
        /// <param name="asset_id">指定全局资产的ID</param>
        /// <param name="amount">指定金额</param>
        /// <returns>查询到的Coin集合</returns>
        protected static Coin[] FindUnspentCoins(IEnumerable<Coin> unspents, UInt256 asset_id, Fixed8 amount)
        {
            Coin[] unspents_asset = unspents.Where(p => p.Output.AssetId == asset_id).ToArray();
            Fixed8 sum = unspents_asset.Sum(p => p.Output.Value);
            if (sum < amount) return null;
            if (sum == amount) return unspents_asset;
            Coin[] unspents_ordered = unspents_asset.OrderByDescending(p => p.Output.Value).ToArray();
            int i = 0;
            while (unspents_ordered[i].Output.Value <= amount)
                amount -= unspents_ordered[i++].Output.Value;
            if (amount == Fixed8.Zero)
                return unspents_ordered.Take(i).ToArray();
            else
                return unspents_ordered.Take(i).Concat(new[] { unspents_ordered.Last(p => p.Output.Value >= amount) }).ToArray();
        }
        /// <summary>
        /// 根据公钥查询钱包内对应的账户对象
        /// </summary>
        /// <param name="pubkey">公钥</param>
        /// <returns>查询到的账户对象</returns>
        public WalletAccount GetAccount(ECPoint pubkey)
        {
            return GetAccount(Contract.CreateSignatureRedeemScript(pubkey).ToScriptHash());
        }
        /// <summary>
        /// 查询钱包内指定全局资产(neo、gas)的可用金额
        /// </summary>
        /// <param name="asset_id">指定的全局资产（neo、gas）ID</param>
        /// <returns>钱包内指定的全局资产可用金额</returns>
        public Fixed8 GetAvailable(UInt256 asset_id)
        {
            return FindUnspentCoins().Where(p => p.Output.AssetId.Equals(asset_id)).Sum(p => p.Output.Value);
        }
        /// <summary>
        /// 查询钱包内指定的资产(全局资产、NEP5资产)可用金额
        /// </summary>
        /// <param name="asset_id">指定的资产id</param>
        /// <returns>钱包内指定资产的可用金额</returns>
        public BigDecimal GetAvailable(UIntBase asset_id)
        {
            if (asset_id is UInt160 asset_id_160)
            {
                byte[] script;
                UInt160[] accounts = GetAccounts().Where(p => !p.WatchOnly).Select(p => p.ScriptHash).ToArray();
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    sb.EmitPush(0);
                    foreach (UInt160 account in accounts)
                    {
                        sb.EmitAppCall(asset_id_160, "balanceOf", account);
                        sb.Emit(OpCode.ADD);
                    }
                    sb.EmitAppCall(asset_id_160, "decimals");
                    script = sb.ToArray();
                }
                ApplicationEngine engine = ApplicationEngine.Run(script, extraGAS: Fixed8.FromDecimal(0.2m) * accounts.Length);
                if (engine.State.HasFlag(VMState.FAULT))
                    return new BigDecimal(0, 0);
                byte decimals = (byte)engine.ResultStack.Pop().GetBigInteger();
                BigInteger amount = engine.ResultStack.Pop().GetBigInteger();
                return new BigDecimal(amount, decimals);
            }
            else
            {
                return new BigDecimal(GetAvailable((UInt256)asset_id).GetData(), 8);
            }
        }
        /// <summary>
        /// 查询钱包内所有账户某个全局资产（neo、gas）的可用余额
        /// </summary>
        /// <param name="asset_id">指定全局资产的id</param>
        /// <returns>指定全局资产的可用余额</returns>
        public Fixed8 GetBalance(UInt256 asset_id)
        {
            return GetCoins(GetAccounts().Select(p => p.ScriptHash)).Where(p => !p.State.HasFlag(CoinState.Spent) && p.Output.AssetId.Equals(asset_id)).Sum(p => p.Output.Value);
        }

        /// <summary>
        /// 选择这个钱包中账户中的一个作为找零地址. 
        /// 选择顺序为先选择默认账户, 已有签名合约的账号 ,非监视账号, 普通账号.
        /// </summary>
        /// <returns>返回一个找零地址</returns>
        public virtual UInt160 GetChangeAddress()
        {
            WalletAccount[] accounts = GetAccounts().ToArray();
            WalletAccount account = accounts.FirstOrDefault(p => p.IsDefault);
            if (account == null)
                account = accounts.FirstOrDefault(p => p.Contract?.Script.IsSignatureContract() == true);
            if (account == null)
                account = accounts.FirstOrDefault(p => !p.WatchOnly);
            if (account == null)
                account = accounts.FirstOrDefault();
            return account?.ScriptHash;
        }
        /// <summary>
        /// 查询获取钱包内所有账户的Coin集合
        /// </summary>
        /// <returns>钱包内所有账户的Coin集合</returns>
        public IEnumerable<Coin> GetCoins()
        {
            return GetCoins(GetAccounts().Select(p => p.ScriptHash));
        }
        /// <summary>
        /// 解析NEP2格式密文中的私钥，并且会验证密码是否正确
        /// </summary>
        /// <param name="nep2">NEP2格式的密文</param>
        /// <param name="passphrase">NEP2格式的密文的加密密码</param>
        /// <param name="N">NEP2格式的解密算法Scrypt使用的参数N,默认为16384</param>
        /// <param name="r">NEP2格式的解密算法Scrypt使用的参数r，默认为8</param>
        /// <param name="p">NEP2格式的解密算法Scrypt使用的参数p，默认为8</param>
        /// <returns>从NEP2格式的密文中解析出的私钥</returns>
        /// <exception cref="System.FormatException">验证密码不正确时抛出</exception>
        public static byte[] GetPrivateKeyFromNEP2(string nep2, string passphrase, int N = 16384, int r = 8, int p = 8)
        {
            if (nep2 == null) throw new ArgumentNullException(nameof(nep2));
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            byte[] data = nep2.Base58CheckDecode();
            if (data.Length != 39 || data[0] != 0x01 || data[1] != 0x42 || data[2] != 0xe0)
                throw new FormatException();
            byte[] addresshash = new byte[4];
            Buffer.BlockCopy(data, 3, addresshash, 0, 4);
            byte[] derivedkey = SCrypt.DeriveKey(Encoding.UTF8.GetBytes(passphrase), addresshash, N, r, p, 64);
            byte[] derivedhalf1 = derivedkey.Take(32).ToArray();
            byte[] derivedhalf2 = derivedkey.Skip(32).ToArray();
            byte[] encryptedkey = new byte[32];
            Buffer.BlockCopy(data, 7, encryptedkey, 0, 32);
            byte[] prikey = XOR(encryptedkey.AES256Decrypt(derivedhalf2), derivedhalf1);
            Cryptography.ECC.ECPoint pubkey = Cryptography.ECC.ECCurve.Secp256r1.G * prikey;
            UInt160 script_hash = Contract.CreateSignatureRedeemScript(pubkey).ToScriptHash();
            string address = script_hash.ToAddress();
            if (!Encoding.ASCII.GetBytes(address).Sha256().Sha256().Take(4).SequenceEqual(addresshash))
                throw new FormatException();
            return prikey;
        }

        /// <summary>
        /// 解析WIF格式字符串中的私钥
        /// </summary>
        /// <param name="wif">WIF格式字符串</param>
        /// <returns>从WIF格式字符串中解析出的私钥</returns>
        /// <exception cref="System.ArgumentNullException">wif为null时抛出</exception>
        public static byte[] GetPrivateKeyFromWIF(string wif)
        {
            if (wif == null) throw new ArgumentNullException();
            byte[] data = wif.Base58CheckDecode();
            if (data.Length != 34 || data[0] != 0x80 || data[33] != 0x01)
                throw new FormatException();
            byte[] privateKey = new byte[32];
            Buffer.BlockCopy(data, 1, privateKey, 0, privateKey.Length);
            Array.Clear(data, 0, data.Length);
            return privateKey;
        }

        /// <summary>
        /// 查询获取钱包内所有可用账户内满足条件(已确认、已花费、未提取gas、未冻结)的neo的Coin集合
        /// </summary>
        /// <returns>查询出的Coin集合</returns>
        public IEnumerable<Coin> GetUnclaimedCoins()
        {
            IEnumerable<UInt160> accounts = GetAccounts().Where(p => !p.Lock && !p.WatchOnly).Select(p => p.ScriptHash);
            IEnumerable<Coin> coins = GetCoins(accounts);
            coins = coins.Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash));
            coins = coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.State.HasFlag(CoinState.Spent));
            coins = coins.Where(p => !p.State.HasFlag(CoinState.Claimed) && !p.State.HasFlag(CoinState.Frozen));
            return coins;
        }
        /// <summary>
        /// 导入X509证书，利用其中的数据生成钱包账户对象
        /// </summary>
        /// <param name="cert">X509格式证书</param>
        /// <returns>利用X509格式证书中的数据生成钱包账户对象</returns>
        public virtual WalletAccount Import(X509Certificate2 cert)
        {
            byte[] privateKey;
            using (ECDsa ecdsa = cert.GetECDsaPrivateKey())
            {
                privateKey = ecdsa.ExportParameters(true).D;
            }
            WalletAccount account = CreateAccount(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);
            return account;
        }
        /// <summary>
        /// 导入wif格式字符串，利用其中的数据生成钱包账户对象
        /// </summary>
        /// <param name="wif">wif格式字符串</param>
        /// <returns>wif格式字符串生成的钱包账户对象</returns>
        public virtual WalletAccount Import(string wif)
        {
            byte[] privateKey = GetPrivateKeyFromWIF(wif);
            WalletAccount account = CreateAccount(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);
            return account;
        }
        /// <summary>
        /// 导入NEP2格式密文，利用其中的数据生成钱包账户对象
        /// </summary>
        /// <param name="nep2">NEP2格式密文</param>
        /// <param name="passphrase">解析NEP2格式密文的密码</param>
        /// <returns>利用NEP2格式密文中的数据生成的钱包账户</returns>
        public virtual WalletAccount Import(string nep2, string passphrase)
        {
            byte[] privateKey = GetPrivateKeyFromNEP2(nep2, passphrase);
            WalletAccount account = CreateAccount(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);
            return account;
        }

        /// <summary>
        /// 构建指定交易类型的交易
        /// </summary>
        /// <typeparam name="T">指定交易类型</typeparam>
        /// <param name="tx">交易对象</param>
        /// <param name="from">付款账户</param>
        /// <param name="change_address">找零地址</param>
        /// <param name="fee">网络手续费</param>
        /// <returns>构建的交易对象</returns>
        public T MakeTransaction<T>(T tx, UInt160 from = null, UInt160 change_address = null, Fixed8 fee = default(Fixed8)) where T : Transaction
        {
            if (tx.Outputs == null) tx.Outputs = new TransactionOutput[0];
            if (tx.Attributes == null) tx.Attributes = new TransactionAttribute[0];
            fee += tx.SystemFee;
            var pay_total = (typeof(T) == typeof(IssueTransaction) ? new TransactionOutput[0] : tx.Outputs).GroupBy(p => p.AssetId, (k, g) => new
            {
                AssetId = k,
                Value = g.Sum(p => p.Value)
            }).ToDictionary(p => p.AssetId);
            if (fee > Fixed8.Zero)
            {
                if (pay_total.ContainsKey(Blockchain.UtilityToken.Hash))
                {
                    pay_total[Blockchain.UtilityToken.Hash] = new
                    {
                        AssetId = Blockchain.UtilityToken.Hash,
                        Value = pay_total[Blockchain.UtilityToken.Hash].Value + fee
                    };
                }
                else
                {
                    pay_total.Add(Blockchain.UtilityToken.Hash, new
                    {
                        AssetId = Blockchain.UtilityToken.Hash,
                        Value = fee
                    });
                }
            }
            var pay_coins = pay_total.Select(p => new
            {
                AssetId = p.Key,
                Unspents = from == null ? FindUnspentCoins(p.Key, p.Value.Value) : FindUnspentCoins(p.Key, p.Value.Value, from)
            }).ToDictionary(p => p.AssetId);
            if (pay_coins.Any(p => p.Value.Unspents == null)) return null;
            var input_sum = pay_coins.Values.ToDictionary(p => p.AssetId, p => new
            {
                p.AssetId,
                Value = p.Unspents.Sum(q => q.Output.Value)
            });
            if (change_address == null) change_address = GetChangeAddress();
            List<TransactionOutput> outputs_new = new List<TransactionOutput>(tx.Outputs);
            foreach (UInt256 asset_id in input_sum.Keys)
            {
                if (input_sum[asset_id].Value > pay_total[asset_id].Value)
                {
                    outputs_new.Add(new TransactionOutput
                    {
                        AssetId = asset_id,
                        Value = input_sum[asset_id].Value - pay_total[asset_id].Value,
                        ScriptHash = change_address
                    });
                }
            }
            tx.Inputs = pay_coins.Values.SelectMany(p => p.Unspents).Select(p => p.Reference).ToArray();
            tx.Outputs = outputs_new.ToArray();
            return tx;
        }

        /// <summary>
        ///  构建一笔交易
        /// </summary>
        /// <param name="attributes">交易的参数列表</param>
        /// <param name="outputs">交易输出</param>
        /// <param name="from">交易的付款地址</param>
        /// <param name="change_address">找零地址</param>
        /// <param name="fee">网络费用</param>
        /// <returns>交易对象</returns>
        public Transaction MakeTransaction(List<TransactionAttribute> attributes, IEnumerable<TransferOutput> outputs, UInt160 from = null, UInt160 change_address = null, Fixed8 fee = default(Fixed8))
        {
            var cOutputs = outputs.Where(p => !p.IsGlobalAsset).GroupBy(p => new
            {
                AssetId = (UInt160)p.AssetId,
                Account = p.ScriptHash
            }, (k, g) => new
            {
                k.AssetId,
                Value = g.Aggregate(BigInteger.Zero, (x, y) => x + y.Value.Value),
                k.Account
            }).ToArray();
            Transaction tx;
            if (attributes == null) attributes = new List<TransactionAttribute>();
            if (cOutputs.Length == 0)
            {
                tx = new ContractTransaction();
            }
            else
            {
                UInt160[] accounts = from == null ? GetAccounts().Where(p => !p.Lock && !p.WatchOnly).Select(p => p.ScriptHash).ToArray() : new[] { from };
                HashSet<UInt160> sAttributes = new HashSet<UInt160>();
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    foreach (var output in cOutputs)
                    {
                        var balances = new List<(UInt160 Account, BigInteger Value)>();
                        foreach (UInt160 account in accounts)
                        {
                            byte[] script;
                            using (ScriptBuilder sb2 = new ScriptBuilder())
                            {
                                sb2.EmitAppCall(output.AssetId, "balanceOf", account);
                                script = sb2.ToArray();
                            }
                            ApplicationEngine engine = ApplicationEngine.Run(script);
                            if (engine.State.HasFlag(VMState.FAULT)) return null;
                            balances.Add((account, engine.ResultStack.Pop().GetBigInteger()));
                        }
                        BigInteger sum = balances.Aggregate(BigInteger.Zero, (x, y) => x + y.Value);
                        if (sum < output.Value) return null;
                        if (sum != output.Value)
                        {
                            balances = balances.OrderByDescending(p => p.Value).ToList();
                            BigInteger amount = output.Value;
                            int i = 0;
                            while (balances[i].Value <= amount)
                                amount -= balances[i++].Value;
                            if (amount == BigInteger.Zero)
                                balances = balances.Take(i).ToList();
                            else
                                balances = balances.Take(i).Concat(new[] { balances.Last(p => p.Value >= amount) }).ToList();
                            sum = balances.Aggregate(BigInteger.Zero, (x, y) => x + y.Value);
                        }
                        sAttributes.UnionWith(balances.Select(p => p.Account));
                        for (int i = 0; i < balances.Count; i++)
                        {
                            BigInteger value = balances[i].Value;
                            if (i == 0)
                            {
                                BigInteger change = sum - output.Value;
                                if (change > 0) value -= change;
                            }
                            sb.EmitAppCall(output.AssetId, "transfer", balances[i].Account, output.Account, value);
                            sb.Emit(OpCode.THROWIFNOT);
                        }
                    }
                    byte[] nonce = new byte[8];
                    rand.NextBytes(nonce);
                    sb.Emit(OpCode.RET, nonce);
                    tx = new InvocationTransaction
                    {
                        Version = 1,
                        Script = sb.ToArray()
                    };
                }
                attributes.AddRange(sAttributes.Select(p => new TransactionAttribute
                {
                    Usage = TransactionAttributeUsage.Script,
                    Data = p.ToArray()
                }));
            }
            tx.Attributes = attributes.ToArray();
            tx.Inputs = new CoinReference[0];
            tx.Outputs = outputs.Where(p => p.IsGlobalAsset).Select(p => p.ToTxOutput()).ToArray();
            tx.Witnesses = new Witness[0];
            if (tx is InvocationTransaction itx)
            {
                ApplicationEngine engine = ApplicationEngine.Run(itx.Script, itx);
                if (engine.State.HasFlag(VMState.FAULT)) return null;
                tx = new InvocationTransaction
                {
                    Version = itx.Version,
                    Script = itx.Script,
                    Gas = InvocationTransaction.GetGas(engine.GasConsumed),
                    Attributes = itx.Attributes,
                    Inputs = itx.Inputs,
                    Outputs = itx.Outputs
                };
            }
            tx = MakeTransaction(tx, from, change_address, fee);
            return tx;
        }
        /// <summary>
        /// 对合约参数上下文对象（交易、共识等）中的数据进行签名
        /// </summary>
        /// <param name="context">合约参数上下文对象（交易、共识等）</param>
        /// <returns>签名的结果，成功为true,失败为false</returns>
        public bool Sign(ContractParametersContext context)
        {
            bool fSuccess = false;
            foreach (UInt160 scriptHash in context.ScriptHashes)
            {
                WalletAccount account = GetAccount(scriptHash);
                if (account?.HasKey != true) continue;
                KeyPair key = account.GetKey();
                byte[] signature = context.Verifiable.Sign(key);
                fSuccess |= context.AddSignature(account.Contract, key.PublicKey, signature);
            }
            return fSuccess;
        }
        /// <summary>
        /// 验证用户输入的钱包密码的正确性.
        /// 这是个抽象方法。
        /// </summary>
        /// <param name="password">用户输入的钱包密码</param>
        /// <returns>验证成功返回true，否则返回false</returns>
        public abstract bool VerifyPassword(string password);

        private static byte[] XOR(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) throw new ArgumentException();
            return x.Zip(y, (a, b) => (byte)(a ^ b)).ToArray();
        }
    }
}
