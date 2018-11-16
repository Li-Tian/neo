using Akka.Actor;
using Akka.Configuration;
using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Actors;
using Neo.Ledger;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    /// 共识服务，实现了dBFF算法
    /// </summary>
    /// <remarks>
    ///  算法介绍： http://docs.neo.org/en-us/basic/consensus/whitepaper.html
    /// </remarks>
    public sealed class ConsensusService : UntypedActor
    {
        /// <summary>
        /// 开始共识消息
        /// </summary>
        public class Start { }

        /// <summary>
        /// 更新视图消息
        /// </summary>
        public class SetViewNumber { public byte ViewNumber; }

        /// <summary>
        /// 超时消息
        /// </summary>
        internal class Timer { public uint Height; public byte ViewNumber; }

        /// <summary>
        /// 上下文
        /// </summary>
        private readonly ConsensusContext context = new ConsensusContext();
        private readonly NeoSystem system;
        private readonly Wallet wallet;

        /// <summary>
        /// 最新收块时间
        /// </summary>
        private DateTime block_received_time;

        /// <summary>
        /// 构建共识服务
        /// </summary>
        /// <param name="system"></param>
        /// <param name="wallet"></param>
        public ConsensusService(NeoSystem system, Wallet wallet)
        {
            this.system = system;
            this.wallet = wallet;
        }

        /// <summary>
        /// 添加新交易
        /// </summary>
        /// <remarks>
        /// 注意，1. 若提案block的交易全部收齐时，将发送PrepareReponse消息
        /// 2. 校验失败时，会触发ChangeView
        /// </remarks>
        /// <param name="tx">待添加交易</param>
        /// <param name="verify">是否进行交易验证</param>
        /// <returns></returns>
        private bool AddTransaction(Transaction tx, bool verify)
        {
            if (context.Snapshot.ContainsTransaction(tx.Hash) ||
                (verify && !tx.Verify(context.Snapshot, context.Transactions.Values)) ||
                !Plugin.CheckPolicy(tx))
            {
                Log($"reject tx: {tx.Hash}{Environment.NewLine}{tx.ToArray().ToHexString()}", LogLevel.Warning);
                RequestChangeView();
                return false;
            }
            context.Transactions[tx.Hash] = tx;
            if (context.TransactionHashes.Length == context.Transactions.Count)
            {
                if (context.VerifyRequest())
                {
                    Log($"send prepare response");
                    context.State |= ConsensusState.SignatureSent;
                    context.Signatures[context.MyIndex] = context.MakeHeader().Sign(context.KeyPair);
                    SignAndRelay(context.MakePrepareResponse(context.Signatures[context.MyIndex]));
                    CheckSignatures();
                }
                else
                {
                    RequestChangeView();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 更新定时器
        /// </summary>
        /// <param name="delay">delay秒后超时</param>
        private void ChangeTimer(TimeSpan delay)
        {
            Context.System.Scheduler.ScheduleTellOnce(delay, Self, new Timer
            {
                Height = context.BlockIndex,
                ViewNumber = context.ViewNumber
            }, ActorRefs.NoSender);
        }

        /// <summary>
        /// 检查视图更换是否达成一致 
        /// </summary>
        /// <remarks>
        /// 当有最少M个节点的期望视图编号都等于view_number时，则视图更换完成，重置共识活动以及使该视图编号
        /// </remarks>
        /// <param name="view_number">视图编号</param>
        private void CheckExpectedView(byte view_number)
        {
            if (context.ViewNumber == view_number) return;
            if (context.ExpectedView.Count(p => p == view_number) >= context.M)
            {
                InitializeConsensus(view_number);
            }
        }

        /// <summary>
        /// 检查收到的提案block的签名是否齐全，若已收到最少M个签名时，则提案通过，并广播完整的区块
        /// </summary>
        private void CheckSignatures()
        {
            if (context.Signatures.Count(p => p != null) >= context.M && context.TransactionHashes.All(p => context.Transactions.ContainsKey(p)))
            {
                Contract contract = Contract.CreateMultiSigContract(context.M, context.Validators);
                Block block = context.MakeHeader();
                ContractParametersContext sc = new ContractParametersContext(block);
                for (int i = 0, j = 0; i < context.Validators.Length && j < context.M; i++)
                    if (context.Signatures[i] != null)
                    {
                        sc.AddSignature(contract, context.Validators[i], context.Signatures[i]);
                        j++;
                    }
                sc.Verifiable.Witnesses = sc.GetWitnesses();
                block.Transactions = context.TransactionHashes.Select(p => context.Transactions[p]).ToArray();
                Log($"relay block: {block.Hash}");
                system.LocalNode.Tell(new LocalNode.Relay { Inventory = block });
                context.State |= ConsensusState.BlockSent;
            }
        }

        /// <summary>
        /// 初始化共识活动
        /// </summary>
        /// <param name="view_number">视图编号</param>
        private void InitializeConsensus(byte view_number)
        {
            if (view_number == 0)
                context.Reset(wallet);
            else
                context.ChangeView(view_number);
            if (context.MyIndex < 0) return;
            if (view_number > 0)
                Log($"changeview: view={view_number} primary={context.Validators[context.GetPrimaryIndex((byte)(view_number - 1u))]}", LogLevel.Warning);
            Log($"initialize: height={context.BlockIndex} view={view_number} index={context.MyIndex} role={(context.MyIndex == context.PrimaryIndex ? ConsensusState.Primary : ConsensusState.Backup)}");
            if (context.MyIndex == context.PrimaryIndex)
            {
                context.State |= ConsensusState.Primary;
                TimeSpan span = DateTime.UtcNow - block_received_time;
                if (span >= Blockchain.TimePerBlock)
                    ChangeTimer(TimeSpan.Zero);
                else
                    ChangeTimer(Blockchain.TimePerBlock - span);
            }
            else
            {
                context.State = ConsensusState.Backup;// 重置 State
                ChangeTimer(TimeSpan.FromSeconds(Blockchain.SecondsPerBlock << (view_number + 1)));
            }
        }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            Plugin.Log(nameof(ConsensusService), level, message);
        }


        /// <summary>
        /// ChangeView消息处理
        /// </summary>
        /// <param name="payload">ChangeView货物</param>
        /// <param name="message">ChangeView消息</param>
        private void OnChangeViewReceived(ConsensusPayload payload, ChangeView message)
        {
            if (message.NewViewNumber <= context.ExpectedView[payload.ValidatorIndex])
                return;
            Log($"{nameof(OnChangeViewReceived)}: height={payload.BlockIndex} view={message.ViewNumber} index={payload.ValidatorIndex} nv={message.NewViewNumber}");
            context.ExpectedView[payload.ValidatorIndex] = message.NewViewNumber;
            CheckExpectedView(message.NewViewNumber);
        }

        /// <summary>
        /// 共识消息货物的处理，检查与具体共识消息处理
        /// </summary>
        /// <param name="payload">共识消息货物</param>
        private void OnConsensusPayload(ConsensusPayload payload)
        {
            if (context.State.HasFlag(ConsensusState.BlockSent)) return;
            if (payload.ValidatorIndex == context.MyIndex) return;
            if (payload.Version != ConsensusContext.Version)
                return;
            if (payload.PrevHash != context.PrevHash || payload.BlockIndex != context.BlockIndex)
            {
                if (context.Snapshot.Height + 1 < payload.BlockIndex)
                {
                    Log($"chain sync: expected={payload.BlockIndex} current: {context.Snapshot.Height} nodes={LocalNode.Singleton.ConnectedCount}", LogLevel.Warning);
                }                          
                return;
            }
            if (payload.ValidatorIndex >= context.Validators.Length) return;
            ConsensusMessage message;
            try
            {
                message = ConsensusMessage.DeserializeFrom(payload.Data);
            }
            catch
            {
                return;
            } 
            if (message.ViewNumber != context.ViewNumber && message.Type != ConsensusMessageType.ChangeView)
                return; 
            switch (message.Type)
            {
                case ConsensusMessageType.ChangeView:
                    OnChangeViewReceived(payload, (ChangeView)message);
                    break;
                case ConsensusMessageType.PrepareRequest:
                    OnPrepareRequestReceived(payload, (PrepareRequest)message);
                    break;
                case ConsensusMessageType.PrepareResponse:
                    OnPrepareResponseReceived(payload, (PrepareResponse)message);
                    break;
            }
        }

        /// <summary>
        /// 块持久化完毕消息处理，重置共识过程
        /// </summary>
        /// <param name="block">已处理完的block</param>
        private void OnPersistCompleted(Block block)
        {
            Log($"persist block: {block.Hash}");
            block_received_time = DateTime.UtcNow;
            InitializeConsensus(0);
        }

        /// <summary>
        /// PrepareRequest消息处理
        /// </summary>
        /// <remarks>
        /// 1. 检查消息
        /// 2. 保存提案内容： Timestamp, Nonce, NextConsenus, TransactionHashes, and Signature
        /// 3. 过滤 signatures 数组
        /// 4. 若还有缺少的交易，则发送inv消息附带上缺少的交易hash
        /// </remarks>
        /// <param name="payload">PrepareRequest货物</param>
        /// <param name="message">PrepareRequest消息</param>
        private void OnPrepareRequestReceived(ConsensusPayload payload, PrepareRequest message)
        {
            if (context.State.HasFlag(ConsensusState.RequestReceived)) return;
            if (payload.ValidatorIndex != context.PrimaryIndex) return;
            Log($"{nameof(OnPrepareRequestReceived)}: height={payload.BlockIndex} view={message.ViewNumber} index={payload.ValidatorIndex} tx={message.TransactionHashes.Length}");
            if (!context.State.HasFlag(ConsensusState.Backup)) return;
            if (payload.Timestamp <= context.Snapshot.GetHeader(context.PrevHash).Timestamp || payload.Timestamp > DateTime.UtcNow.AddMinutes(10).ToTimestamp())
            {
                Log($"Timestamp incorrect: {payload.Timestamp}", LogLevel.Warning);
                return;
            }
            context.State |= ConsensusState.RequestReceived;
            context.Timestamp = payload.Timestamp;
            context.Nonce = message.Nonce;
            context.NextConsensus = message.NextConsensus;
            context.TransactionHashes = message.TransactionHashes;
            context.Transactions = new Dictionary<UInt256, Transaction>();
            byte[] hashData = context.MakeHeader().GetHashData();
            if (!Crypto.Default.VerifySignature(hashData, message.Signature, context.Validators[payload.ValidatorIndex].EncodePoint(false))) return;
            for (int i = 0; i < context.Signatures.Length; i++)
                if (context.Signatures[i] != null)
                    if (!Crypto.Default.VerifySignature(hashData, context.Signatures[i], context.Validators[i].EncodePoint(false)))
                        context.Signatures[i] = null;
            context.Signatures[payload.ValidatorIndex] = message.Signature;
            Dictionary<UInt256, Transaction> mempool = Blockchain.Singleton.GetMemoryPool().ToDictionary(p => p.Hash);
            List<Transaction> unverified = new List<Transaction>();
            foreach (UInt256 hash in context.TransactionHashes.Skip(1))
            {
                if (mempool.TryGetValue(hash, out Transaction tx))
                {
                    if (!AddTransaction(tx, false))
                        return;
                }
                else
                {
                    tx = Blockchain.Singleton.GetUnverifiedTransaction(hash);
                    if (tx != null)
                        unverified.Add(tx);
                }
            }
            foreach (Transaction tx in unverified)
                if (!AddTransaction(tx, true))
                    return;
            if (!AddTransaction(message.MinerTransaction, true)) return;
            if (context.Transactions.Count < context.TransactionHashes.Length)
            {
                UInt256[] hashes = context.TransactionHashes.Where(i => !context.Transactions.ContainsKey(i)).ToArray();
                system.TaskManager.Tell(new TaskManager.RestartTasks
                {
                    Payload = InvPayload.Create(InventoryType.TX, hashes)
                });
            }
        }

        /// <summary>
        /// PrepareResponse 消息处理
        /// </summary>
        /// 若PrepareRequest消息已收过，则验证签名数据后收下；否则先收下（后续在收到PrepareRequset消息时，会进行验证）
        /// <param name="payload">PrepareResponse货物</param>
        /// <param name="message">PrepareResponse消息</param>
        private void OnPrepareResponseReceived(ConsensusPayload payload, PrepareResponse message)
        {
            if (context.Signatures[payload.ValidatorIndex] != null) return;
            Log($"{nameof(OnPrepareResponseReceived)}: height={payload.BlockIndex} view={message.ViewNumber} index={payload.ValidatorIndex}");
            byte[] hashData = context.MakeHeader()?.GetHashData();
            if (hashData == null)
            {
                context.Signatures[payload.ValidatorIndex] = message.Signature;
            }
            else if (Crypto.Default.VerifySignature(hashData, message.Signature, context.Validators[payload.ValidatorIndex].EncodePoint(false)))
            {
                context.Signatures[payload.ValidatorIndex] = message.Signature;
                CheckSignatures();
            }
        }

        /// <summary>
        /// 消息接收器： Start, SetViewNumber, Timer, ConsensusPayload, Transaction and Blockchain.PersistCompleted
        /// </summary>
        /// <param name="message"></param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Start _:
                    OnStart();
                    break;
                case SetViewNumber setView:
                    InitializeConsensus(setView.ViewNumber);
                    break;
                case Timer timer:
                    OnTimer(timer);
                    break;
                case ConsensusPayload payload:
                    OnConsensusPayload(payload);
                    break;
                case Transaction transaction:
                    OnTransaction(transaction);
                    break;
                case Blockchain.PersistCompleted completed:
                    OnPersistCompleted(completed.Block);
                    break;
            }
        }

        /// <summary>
        /// 启动共识事件处理
        /// </summary>
        private void OnStart()
        {
            Log("OnStart");
            InitializeConsensus(0);
        }

        /// <summary>
        /// 超时处理
        /// </summary>
        /// <remarks>
        /// 若是议长且尚未发送 PrepareRequest, 则发送PrepareRequest消息,否则发送ChangeView
        /// </remarks>
        /// <param name="timer"></param>
        private void OnTimer(Timer timer)
        {
            if (context.State.HasFlag(ConsensusState.BlockSent)) return;
            if (timer.Height != context.BlockIndex || timer.ViewNumber != context.ViewNumber) return;
            Log($"timeout: height={timer.Height} view={timer.ViewNumber} state={context.State}");
            if (context.State.HasFlag(ConsensusState.Primary) && !context.State.HasFlag(ConsensusState.RequestSent))
            {
                Log($"send prepare request: height={timer.Height} view={timer.ViewNumber}");
                context.State |= ConsensusState.RequestSent;
                if (!context.State.HasFlag(ConsensusState.SignatureSent))
                {
                    context.Fill(wallet);
                    context.Timestamp = Math.Max(DateTime.UtcNow.ToTimestamp(), context.Snapshot.GetHeader(context.PrevHash).Timestamp + 1);
                    context.Signatures[context.MyIndex] = context.MakeHeader().Sign(context.KeyPair);
                }
                SignAndRelay(context.MakePrepareRequest());
                if (context.TransactionHashes.Length > 1)
                {
                    foreach (InvPayload payload in InvPayload.CreateGroup(InventoryType.TX, context.TransactionHashes.Skip(1).ToArray()))
                        system.LocalNode.Tell(Message.Create("inv", payload));
                }
                ChangeTimer(TimeSpan.FromSeconds(Blockchain.SecondsPerBlock << (timer.ViewNumber + 1)));
            }
            else if ((context.State.HasFlag(ConsensusState.Primary) && context.State.HasFlag(ConsensusState.RequestSent)) || context.State.HasFlag(ConsensusState.Backup))
            {
                RequestChangeView();
            }
        }

        /// <summary>
        /// 新交易消息处理
        /// </summary>
        /// <remarks>
        /// 若缺少该交易，并在提案块中，则添加
        /// </remarks>
        /// <param name="transaction"></param>
        private void OnTransaction(Transaction transaction)
        {
            if (transaction.Type == TransactionType.MinerTransaction) return;
            if (!context.State.HasFlag(ConsensusState.Backup) || !context.State.HasFlag(ConsensusState.RequestReceived) || context.State.HasFlag(ConsensusState.SignatureSent) || context.State.HasFlag(ConsensusState.ViewChanging) || context.State.HasFlag(ConsensusState.BlockSent))
                return;
            if (context.Transactions.ContainsKey(transaction.Hash)) return;
            if (!context.TransactionHashes.Contains(transaction.Hash)) return;
            AddTransaction(transaction, true);
        }

        protected override void PostStop()
        {
            Log("OnStop");
            context.Dispose();
            base.PostStop();
        }

        /// <summary>
        /// 创建ActorRef并带邮箱`consensus-service-mailbox`
        /// </summary>
        /// <param name="system">NEO系统</param>
        /// <param name="wallet">钱包</param>
        /// <returns></returns>
        public static Props Props(NeoSystem system, Wallet wallet)
        {
            return Akka.Actor.Props.Create(() => new ConsensusService(system, wallet)).WithMailbox("consensus-service-mailbox");
        }

        /// <summary>
        /// 发送改变视图消息
        /// </summary>
        private void RequestChangeView()
        {
            context.State |= ConsensusState.ViewChanging;
            context.ExpectedView[context.MyIndex]++;
            Log($"request change view: height={context.BlockIndex} view={context.ViewNumber} nv={context.ExpectedView[context.MyIndex]} state={context.State}");
            ChangeTimer(TimeSpan.FromSeconds(Blockchain.SecondsPerBlock << (context.ExpectedView[context.MyIndex] + 1)));
            SignAndRelay(context.MakeChangeView());
            CheckExpectedView(context.ExpectedView[context.MyIndex]);
        }

        /// <summary>
        /// 签名数据并转发
        /// </summary>
        /// <param name="payload">待签名数据</param>
        private void SignAndRelay(ConsensusPayload payload)
        {
            ContractParametersContext sc;
            try
            {
                sc = new ContractParametersContext(payload);
                wallet.Sign(sc);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            sc.Verifiable.Witnesses = sc.GetWitnesses();
            system.LocalNode.Tell(new LocalNode.SendDirectly { Inventory = payload });
        }
    }

    /// <summary>
    /// 共识服务优先级邮箱
    /// </summary>
    internal class ConsensusServiceMailbox : PriorityMailbox
    {
        /// <summary>
        /// 注册共识服务邮箱
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="config"></param>
        public ConsensusServiceMailbox(Akka.Actor.Settings settings, Config config)
            : base(settings, config)
        {
        }

        /// <summary>
        /// 检查消息是否是高优先级
        /// <list type="bullet">
        /// <item>
        /// <term>ConsensusPayload</term>
        /// <description>高优先级</description>
        /// </item>
        /// <item>
        /// <term>SetViewNumber</term>
        /// <description>高优先级</description>
        /// </item>
        /// <item>
        /// <term>Timer</term>
        /// <description>高优先级</description>
        /// </item>
        /// <item>
        /// <term>Blockchain.PersistCompleted </term>
        /// <description>高优先级</description>
        /// </item>
        /// <item>
        /// <term>Start</term>
        /// <description>低优先级</description>
        /// </item>
        /// <item>
        /// <term>Transaction</term>
        /// <description>低优先级</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override bool IsHighPriority(object message)
        {
            switch (message)
            {
                case ConsensusPayload _:
                case ConsensusService.SetViewNumber _:
                case ConsensusService.Timer _:
                case Blockchain.PersistCompleted _:
                    return true;
                default:
                    return false;
            }
        }
    }
}
