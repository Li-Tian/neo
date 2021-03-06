﻿using Akka.Actor;
using Akka.Configuration;
using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Actors;
using Neo.Ledger;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.Plugins;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Consensus
{
    /// <summary>
    /// 共识服务，实现了dBFT算法
    /// </summary>
    /// <remarks>
    ///  算法介绍： http://docs.neo.org/en-us/basic/consensus/whitepaper.html
    /// </remarks>
    public sealed class ConsensusService : UntypedActor
    {
        /// <summary>
        /// 开始共识消息（AKKA自定义消息类型）
        /// </summary>
        public class Start { }

        /// <summary>
        /// 更新/设置视图编号（AKKA自定义消息类型）
        /// </summary>
        public class SetViewNumber {
            /// <summary>
            /// 视图编号
            /// </summary>
            public byte ViewNumber;
        }

        /// <summary>
        /// 超时消息（AKKA自定义消息类型）
        /// </summary>
        /// <remarks>
        /// 给出超时区块的块高度和视图编号
        /// </remarks>
        internal class Timer { public uint Height; public byte ViewNumber; }

        // <summary>
        // 共识上下文
        // </summary>
        //private readonly ConsensusContext context;
        
        // <summary>
        // Neo系统的当前状态
        // </summary>
        //private readonly NeoSystem system;

        private readonly IConsensusContext context;
        private readonly IActorRef localNode;
        private readonly IActorRef taskManager;

        private ICancelable timer_token;
        /// <summary>
        /// 最新收块时间
        /// </summary>
        private DateTime block_received_time;

        // <summary>
        // 构建共识服务
        // </summary>
        // <param name="system">Neo系统的当前状态</param>
        // <param name="wallet">钱包</param>
        //public ConsensusService(NeoSystem system, Wallet wallet)

        /// <summary>
        /// 共识服务构造函数
        /// </summary>
        /// <param name="localNode">本地节点</param>
        /// <param name="taskManager">任务管理器</param>
        /// <param name="wallet">钱包</param>
        public ConsensusService(IActorRef localNode, IActorRef taskManager, Wallet wallet)
            : this(localNode, taskManager, new ConsensusContext(wallet))
        {
        }
        /// <summary>
        /// 共识服务构造函数
        /// </summary>
        /// <param name="localNode">本地节点</param>
        /// <param name="taskManager">任务管理器</param>
        /// <param name="context">共识上下文</param>
        public ConsensusService(IActorRef localNode, IActorRef taskManager, IConsensusContext context)
        {
            this.localNode = localNode;
            this.taskManager = taskManager;
            this.context = context;
        }


        /// <summary>
        /// 添加新交易
        /// </summary>
        /// <remarks>
        /// 大致算法过程解释如下：
        /// 1. 若以下3个条件满足任意一条，这条交易会被拒收
        /// 1) 当前上下文的快照中已经包含这条交易；
        /// 2）传入的verify参数为True，意味着要验证；且验证的结果为false;
        /// 3) 这条交易不满足Plugin中的某条policy；
        /// 
        /// 2. 将这条交易放入Transactions数组；
        /// 3. 当提案block的交易全部收齐，即TransactionHashes的长度和Transactions数组的长度一致时：
        ///    若通过了上下文中的VerifyRequest测试，该节点将发送PrepareReponse消息；否则该节点将发送ChangeView请求；
        ///    VerifyRequest测试的具体内容见ConsensusContext.cs中的VerifyRequest方法。
        /// </remarks>
        /// <param name="tx">待添加交易</param>
        /// <param name="verify">是否进行交易验证</param>
        /// <returns>添加成功返回 true，添加失败返回 false</returns>
        private bool AddTransaction(Transaction tx, bool verify)
        {
            if (verify && !context.VerifyTransaction(tx))
            {
                Log($"Invalid transaction: {tx.Hash}{Environment.NewLine}{tx.ToArray().ToHexString()}", LogLevel.Warning);
                RequestChangeView();
                return false;
            }
            if (!Plugin.CheckPolicy(tx))
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
                    context.SignHeader();
                    localNode.Tell(new LocalNode.SendDirectly { Inventory = context.MakePrepareResponse(context.Signatures[context.MyIndex]) });
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
            timer_token.CancelIfNotNull();
            timer_token = Context.System.Scheduler.ScheduleTellOnceCancelable(delay, Self, new Timer
            {
                Height = context.BlockIndex,
                ViewNumber = context.ViewNumber
            }, ActorRefs.NoSender);
        }

        /// <summary>
        /// 检查视图更换的决定是否达成一致 
        /// </summary>
        /// <remarks>
        /// 当上下文中的当前视图标号与输入的这个视图标号一样时，不做任何处理，返回；否则，检查是不是有
        /// 有最少M个节点的期望视图编号都等于一个view_number时，则更新到该视图编号开始重置共识活动；
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
                Block block = context.CreateBlock();
                Log($"relay block: {block.Hash}");
                localNode.Tell(new LocalNode.Relay { Inventory = block });
                context.State |= ConsensusState.BlockSent;
            }
        }

        /// <summary>
        /// 初始化共识活动
        /// </summary>
        /// <param name="view_number">视图编号</param>
        /// <remarks>
        /// 视图编号为0，意味着这是个新块，刚开始第一轮共识，重置上下文中的内容；
        /// 视图编号不为0，则这次共识时对同一块进行视图转换试图达成共识
        /// 如果上下文中MyIndex小于0，意味着此节点非验证人节点，不参与共识过程
        /// 如果试图编号大于零，发出Log消息，表明新的一轮共识过程以新的视图编号开启
        /// 发出Log消息，给出当前共识要确定的块的高度、视图编号、此节点的角色(议长还是议员)等
        /// 如果该节点是议长，议长需要将其状态改为"Primary"，并设置计时器，因为系统设定了一个块约15秒生成一个新块
        /// 如果该节点是议员，它就需要将其状态改为"Backup"， 并将计时器乘以2。视图每加1，计时器时长变为之前的两倍
        /// </remarks>

        private void InitializeConsensus(byte view_number)
        {
            if (view_number == 0)
                context.Reset();
            else
                context.ChangeView(view_number);
            if (context.MyIndex < 0) return;
            if (view_number > 0)
                Log($"changeview: view={view_number} primary={context.Validators[context.GetPrimaryIndex((byte)(view_number - 1u))]}", LogLevel.Warning);
            Log($"initialize: height={context.BlockIndex} view={view_number} index={context.MyIndex} role={(context.MyIndex == context.PrimaryIndex ? ConsensusState.Primary : ConsensusState.Backup)}");
            if (context.MyIndex == context.PrimaryIndex)
            {
                context.State |= ConsensusState.Primary;
                TimeSpan span = TimeProvider.Current.UtcNow - block_received_time;
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
        /// <remark>
        /// 在以下3中情况下，此节点不处理收到的这个ConsensusPayload包：
        /// 1. 此节点已经确认了这个块；
        /// 2. 这个共识包实际上是此节点自己发出去的；
        /// 3. 共识包的版本号与当前共识上下文中的版本号不一致，即不是一个共识版本；
        /// 4. 在发现落后于其他共识节点后，此节点发送链同步Log；
        /// 5. 这个共识包中的验证人个数和上下文中的不符；
        /// 
        /// 此共识包通过以上验证之后，进行解包。解开发现这个包的视图号与当前共识的不符并且这不是一个请求转换视图的共识包，不做处理。
        /// 至此若此共识包没有被丢弃，则按照其类型，分别处理。
        /// </remark>
        private void OnConsensusPayload(ConsensusPayload payload)
        {
            if (context.State.HasFlag(ConsensusState.BlockSent)) return;
            if (payload.ValidatorIndex == context.MyIndex) return;
            if (payload.Version != ConsensusContext.Version)
                return;
            if (payload.PrevHash != context.PrevHash || payload.BlockIndex != context.BlockIndex)
            {
                if (context.BlockIndex < payload.BlockIndex)
                {
                    Log($"chain sync: expected={payload.BlockIndex} current={context.BlockIndex - 1} nodes={LocalNode.Singleton.ConnectedCount}", LogLevel.Warning);
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
            block_received_time = TimeProvider.Current.UtcNow;
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
            if (payload.Timestamp <= context.PrevHeader.Timestamp || payload.Timestamp > TimeProvider.Current.UtcNow.AddMinutes(10).ToTimestamp())
            {
                Log($"Timestamp incorrect: {payload.Timestamp}", LogLevel.Warning);
                return;
            }
            if (message.TransactionHashes.Any(p => context.TransactionExists(p)))
            {
                Log($"Invalid request: transaction already exists", LogLevel.Warning);
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
                taskManager.Tell(new TaskManager.RestartTasks
                {
                    Payload = InvPayload.Create(InventoryType.TX, hashes)
                });
            }
        }

        /// <summary>
        /// PrepareResponse消息处理
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
        /// 消息接收器（AKKA框架的方法）
        /// </summary>
        /// <param name="message">
        /// 六类消息：<BR/>
        /// ・Start：启动共识事件处理。<BR/>
        /// ・SetViewNumber：设置共识的视图编号，初始化共识活动。<BR/>
        /// ・Timer：超时处理。若是议长且尚未发送 PrepareRequest, 则发送PrepareRequest消息,否则发送ChangeView。<BR/>
        /// ・ConsensusPayload：共识消息货物的处理，检查与具体共识消息处理<BR/>
        /// ・Transaction：新交易消息处理<BR/>
        /// ・Blockchain.PersistCompleted：块持久化完毕消息处理，重置共识过程<BR/>
        /// </param>
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
                    context.Fill();
                    context.SignHeader();
                }
                localNode.Tell(new LocalNode.SendDirectly { Inventory = context.MakePrepareRequest() });
                if (context.TransactionHashes.Length > 1)
                {
                    foreach (InvPayload payload in InvPayload.CreateGroup(InventoryType.TX, context.TransactionHashes.Skip(1).ToArray()))
                        localNode.Tell(Message.Create("inv", payload));
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

        /// <summary>
        /// Actor停止前回调，回收资源（AKKA框架）
        /// </summary>
        protected override void PostStop()
        {
            Log("OnStop");
            context.Dispose();
            base.PostStop();
        }

        // <summary>
        // 创建ActorRef并带邮箱`consensus-service-mailbox`
        // </summary>
        // <param name="system">NEO系统</param>
        // <param name="wallet">钱包</param>
        // <returns></returns>
        //public static Props Props(NeoSystem system, Wallet wallet)

        /// <summary>
        /// 创建ActorRef并带邮箱`consensus-service-mailbox`
        /// </summary>
        /// <param name="localNode">local node</param>
        /// <param name="taskManager">task manager</param>
        /// <param name="wallet">钱包</param>
        /// <returns>Akka.Actor.Props</returns>
        public static Props Props(IActorRef localNode, IActorRef taskManager, Wallet wallet)
        {
            return Akka.Actor.Props.Create(() => new ConsensusService(localNode, taskManager, wallet)).WithMailbox("consensus-service-mailbox");
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
            localNode.Tell(new LocalNode.SendDirectly { Inventory = context.MakeChangeView() });
            CheckExpectedView(context.ExpectedView[context.MyIndex]);
        }

        // This method is deleted by 70680c4
        ///// <summary>
        ///// 签名数据并转发
        ///// </summary>
        ///// <param name="payload">待签名数据</param>
        //private void SignAndRelay(ConsensusPayload payload)
        //{
        //    ContractParametersContext sc;
        //    try
        //    {
        //        sc = new ContractParametersContext(payload);
        //        wallet.Sign(sc);
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        return;
        //    }
        //    sc.Verifiable.Witnesses = sc.GetWitnesses();
        //    system.LocalNode.Tell(new LocalNode.SendDirectly { Inventory = payload });
        //}
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
