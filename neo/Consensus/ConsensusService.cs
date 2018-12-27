using Akka.Actor;
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
    /// Consensus sevice, implemented the dBFT algorithm
    /// </summary>
    /// <remarks>
    ///  http://docs.neo.org/en-us/basic/consensus/whitepaper.html
    /// </remarks>
    public sealed class ConsensusService : UntypedActor
    {
        /// <summary>
        /// Start consensus activity message
        /// 开始共识消息（AKKA自定义消息类型）
        /// </summary>
        public class Start { }

        /// <summary>
        /// Change the current view number message (AKKA customized message type)
        /// </summary>
        public class SetViewNumber {
            /// <summary>
            /// view number
            /// </summary>
            public byte ViewNumber;
        }

        /// <summary>
        /// Time out message (AKKA customized message type)
        /// </summary>
        /// <remarks>
        /// It contains the `Height` and `ViewNumber` of the timeout block.
        /// </remarks>
        internal class Timer { public uint Height; public byte ViewNumber; }

        // <summary>
        // The context of consensus activity
        // </summary>
        //private readonly ConsensusContext context;
        private readonly IConsensusContext context;

        // <summary>
        // Local node, send and receive message
        // </summary>
        //private readonly ConsensusContext context;
        private readonly IActorRef localNode;

        // <summary>
        // Task manager
        // </summary>
        //private readonly ConsensusContext context;
        private readonly IActorRef taskManager;

        private ICancelable timer_token;
        /// <summary>
        /// The latest block received time
        /// </summary>
        private DateTime block_received_time;


        /// <summary>
        /// Construct a ConsensusService
        /// </summary>
        /// <param name="localNode">local node</param>
        /// <param name="taskManager">task manager</param>
        /// <param name="wallet">wallet</param>
        public ConsensusService(IActorRef localNode, IActorRef taskManager, Wallet wallet)
            : this(localNode, taskManager, new ConsensusContext(wallet))
        {
        }
        /// <summary>
        /// Construction a ConsensusService
        /// </summary>
        /// <param name="localNode">local node</param>
        /// <param name="taskManager">task manager</param>
        /// <param name="context">consensus context</param>
        public ConsensusService(IActorRef localNode, IActorRef taskManager, IConsensusContext context)
        {
            this.localNode = localNode;
            this.taskManager = taskManager;
            this.context = context;
        }



        // <summary>
        // Add new transaction
        // </summary>
        // <remarks>
        // Note, if the proposal block's transactions are all received, the PrepareResponse will be send.
        // If the verification fails, the ChangeView will be send.
        // </remarks>
        // <param name="tx"></param>
        // <param name="verify">Whether or not to the verify the transaction</param>
        // <returns></returns>

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
        /// Change timer
        /// </summary>
        /// <param name="delay">`delay` seconds timeout</param>
        private void ChangeTimer(TimeSpan delay)
        {
            timer_token.CancelIfNotNull();
            timer_token = Context.System.Scheduler.ScheduleTellOnceCancelable(delay, Self, new Timer
            {
                Height = context.BlockIndex,
                ViewNumber = context.ViewNumber
            }, ActorRefs.NoSender);
        }

        // <summary>
        // Check the exptected view number array 
        // </summary>
        // <remarks>
        // If there are at least M nodes meeting the EV[i] == view_number, the view change completed.

        /// <summary>
        /// 检查视图更换的决定是否达成一致 
        /// </summary>
        /// <remarks>
        /// 当上下文中的当前视图标号与输入的这个视图标号一样时，不做任何处理，返回；否则，检查是不是有
        /// 有最少M个节点的期望视图编号都等于一个view_number时，则更新到该视图编号开始重置共识活动；
        /// </remarks>
        /// <param name="view_number"></param>
        private void CheckExpectedView(byte view_number)
        {
            if (context.ViewNumber == view_number) return;
            if (context.ExpectedView.Count(p => p == view_number) >= context.M)
            {
                InitializeConsensus(view_number);
            }
        }

        /// <summary>
        /// Check signatures
        /// </summary>
        /// <remarks>
        /// if there are at least M signatures, the proposal block will be accepted and send the full block
        /// </remarks>
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
        /// Initialize the consensus activity with view_number
        /// </summary>
        /// <param name="view_number">view number</param>
        /// <remarks>
        /// If view number is zero, it means a new block in the first round, then reset context.
        /// Else, change view.
        /// If Myindex of context is less than zero, not a validator, then not participate the consensus activity.
        /// If node is the Primary/Speaker, set context's state `ConsensusState.Primary` flag true, then reset the timer as block time is 15 seconds.
        /// If node is the Backup/Delegates, set context's state `ConsensusState.Backup` flag true, 
        /// then reset the timer with 15 << view_number +1) seconds to timeout, to avoid frequent view change.
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
                context.State = ConsensusState.Backup;// reset State
                ChangeTimer(TimeSpan.FromSeconds(Blockchain.SecondsPerBlock << (view_number + 1)));
            }
        }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            Plugin.Log(nameof(ConsensusService), level, message);
        }


        /// <summary>
        /// ChangeView processing
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="payload"></param>
        /// <param name="message"></param>
        private void OnChangeViewReceived(ConsensusPayload payload, ChangeView message)
        {
            if (message.NewViewNumber <= context.ExpectedView[payload.ValidatorIndex])
                return;
            Log($"{nameof(OnChangeViewReceived)}: height={payload.BlockIndex} view={message.ViewNumber} index={payload.ValidatorIndex} nv={message.NewViewNumber}");
            context.ExpectedView[payload.ValidatorIndex] = message.NewViewNumber;
            CheckExpectedView(message.NewViewNumber);
        }


        /// <summary>
        /// Consensus message checking and processing
        /// </summary>
        /// <param name="payload">consensus message payload</param>
        /// <remark>
        /// This node will ignore the message when one the below cases happens:
        /// 1. This node has already send the full prosoal block before enter the next round.
        /// 2. The payload was sent from myself.
        /// 3. The payload's version is not equal to the current context's version.
        /// 4. If the context's BlockIndex is less than the payload's, then send a sync log and ignore.
        /// 5. If the index of the payload's validator is more than the number of current context's validators, then ignore.
        /// 6. Deserialize consensus message from the payload's `Data`, if the consensus message's view number is same to the context's view number and the conosensus message is not `ChangeView`, then ignore.
        /// 7. If the consensus message is not a `ChangeView`, `PrepareResponse` or `PrepareResponse`, then ignore
        ///
        /// Process the consensus message
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
                    //Log($"chain sync: expected={payload.BlockIndex} current: {context.Snapshot.Height} nodes={LocalNode.Singleton.ConnectedCount}", LogLevel.Warning);
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
        /// BlockPersistComleted message proccessing
        /// </summary>
        /// <param name="block"></param>
        private void OnPersistCompleted(Block block)
        {
            Log($"persist block: {block.Hash}");
            block_received_time = TimeProvider.Current.UtcNow;
            InitializeConsensus(0);
        }

        /// <summary>
        /// PrepareRequest message proccessing
        /// </summary>
        /// <remarks>
        /// 1. Validate the message
        /// 2. Reserve the proposal block's data
        /// 3. Filter signatures
        /// 4. Send inv message to acquire the missing txs
        /// </remarks>
        /// <param name="payload"></param>
        /// <param name="message"></param>
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
        /// PrepareResponse message processing
        /// </summary>
        /// If the PrepareRequest received before, verify the signature, otherwise, reserve the signature and will filter it in PrepareRequest processing.
        /// <param name="payload"></param>
        /// <param name="message"></param>
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
        /// Message receiver, inlcudes Start, SetViewNumber, Timer, ConsensusPayload, Transaction and Blockchain.PersistCompleted, which is a method of AKKA. 
        /// </summary>
        /// <param name="message">
        /// Six types of messages：<BR/>
        /// ・Start：State message processing, initialize consensus activity<BR/>
        /// ・SetViewNumber：Initialize the consensus activity with view number<BR/>
        /// ・Timer：Timeout processing. If it's Primary and has not send PrepareRequest message, then send PrepareRequest message, otherwise, send ChangeView<BR/>
        /// ・ConsensusPayload：Consensus message checking and processing<BR/>
        /// ・Transaction：New transaction message processing<BR/>
        /// ・Blockchain.PersistCompleted：BlockPersistComleted message proccessing<BR/>
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
        /// State message processing, initialize consensus activity
        /// </summary>
        private void OnStart()
        {
            Log("OnStart");
            InitializeConsensus(0);
        }

        /// <summary>
        /// Timeout processing
        /// </summary>
        /// <remarks>
        /// If it's Primary and has not send PrepareRequest message, then send PrepareRequest message, otherwise, send ChangeView
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
        /// New transaction message processing
        /// </summary>
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
        /// Post stop method, free resource. It's a method of AKKA.
        /// </summary>
        protected override void PostStop()
        {
            Log("OnStop");
            context.Dispose();
            base.PostStop();
        }

        /// <summary>
        /// Create ActorRef with mail box `consensus-service-mailbox`
        /// </summary>
        /// <param name="localNode">local node</param>
        /// <param name="taskManager">task manager</param>
        /// <param name="wallet">wallet</param>
        /// <returns>Akka.Actor.Props</returns>
        public static Props Props(IActorRef localNode, IActorRef taskManager, Wallet wallet)
        {
            return Akka.Actor.Props.Create(() => new ConsensusService(localNode, taskManager, wallet)).WithMailbox("consensus-service-mailbox");
        }


        /// <summary>
        /// Send ChangeView message
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

    internal class ConsensusServiceMailbox : PriorityMailbox
    {
        /// <summary>
        /// Register consensus service mailbox
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="config"></param>
        public ConsensusServiceMailbox(Akka.Actor.Settings settings, Config config)
            : base(settings, config)
        {
        }

        /// <summary>
        /// Check if the message is high priority
        /// <list type="bullet">
        /// <item>
        /// <term>ConsensusPayload</term>
        /// <description>high priority</description>
        /// </item>
        /// <item>
        /// <term>SetViewNumber</term>
        /// <description>high priority</description>
        /// </item>
        /// <item>
        /// <term>Timer</term>
        /// <description>high priority</description>
        /// </item>
        /// <item>
        /// <term>Blockchain.PersistCompleted </term>
        /// <description>high priority</description>
        /// </item>
        /// <item>
        /// <term>Start</term>
        /// <description>low priority</description>
        /// </item>
        /// <item>
        /// <term>Transaction</term>
        /// <description>low priority</description>
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