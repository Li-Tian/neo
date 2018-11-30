using Akka.Actor;
using Neo.Consensus;
using Neo.Ledger;
using Neo.Network.P2P;
using Neo.Network.RPC;
using Neo.Persistence;
using Neo.Plugins;
using Neo.Wallets;
using System;
using System.Net;

namespace Neo
{
    /// <summary>
    /// NEO核心系统类，用于控制和运行NEO的各项功能
    /// </summary>
    public class NeoSystem : IDisposable
    {
        /// <summary>
        /// 一个名为NeoSystem的基于Akka框架ActorSystem模型的系统对象
        /// </summary>
        public ActorSystem ActorSystem { get; } = ActorSystem.Create(nameof(NeoSystem),
            $"akka {{ log-dead-letters = off }}" +
            $"blockchain-mailbox {{ mailbox-type: \"{typeof(BlockchainMailbox).AssemblyQualifiedName}\" }}" +
            $"task-manager-mailbox {{ mailbox-type: \"{typeof(TaskManagerMailbox).AssemblyQualifiedName}\" }}" +
            $"remote-node-mailbox {{ mailbox-type: \"{typeof(RemoteNodeMailbox).AssemblyQualifiedName}\" }}" +
            $"protocol-handler-mailbox {{ mailbox-type: \"{typeof(ProtocolHandlerMailbox).AssemblyQualifiedName}\" }}" +
            $"consensus-service-mailbox {{ mailbox-type: \"{typeof(ConsensusServiceMailbox).AssemblyQualifiedName}\" }}");
        /// <summary>
        /// 一个描述Blockchain对象的Actor引用
        /// </summary>
        public IActorRef Blockchain { get; }
        /// <summary>
        /// 一个描述LocalNode本地节点对象的Actor引用
        /// </summary>
        public IActorRef LocalNode { get; }
        /// <summary>
        /// 一个描述TaskManager任务管理对象的Actor引用
        /// </summary>
        internal IActorRef TaskManager { get; }
        /// <summary>
        /// 一个描述Consensus共识服务对象的Actor引用
        /// </summary>
        public IActorRef Consensus { get; private set; }
        /// <summary>
        /// NEO系统提供RPC服务的服务器对象
        /// </summary>
        public RpcServer RpcServer { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="store">数据存储对象</param>
        public NeoSystem(Store store)
        {
            this.Blockchain = ActorSystem.ActorOf(Ledger.Blockchain.Props(this, store));
            this.LocalNode = ActorSystem.ActorOf(Network.P2P.LocalNode.Props(this));
            this.TaskManager = ActorSystem.ActorOf(Network.P2P.TaskManager.Props(this));
            Plugin.LoadPlugins(this);
        }
        /// <summary>
        /// 回收方法，释放资源
        /// </summary>
        public void Dispose()
        {
            RpcServer?.Dispose();
            ActorSystem.Stop(LocalNode);
            ActorSystem.Dispose();
        }
        /// <summary>
        /// 通过Akka框架通知共识服务开始工作
        /// </summary>
        /// <param name="wallet">钱包对象</param>
        public void StartConsensus(Wallet wallet)
        {
            Consensus = ActorSystem.ActorOf(ConsensusService.Props(this, wallet));
            Consensus.Tell(new ConsensusService.Start());
        }
        /// <summary>
        /// 通过Akka框架通知本地节点开始工作
        /// </summary>
        /// <param name="port">tcp监听端口</param>
        /// <param name="wsPort">websocket监听端口</param>
        /// <param name="minDesiredConnections">最小预连接数</param>
        /// <param name="maxConnections">最大连接数</param>
        public void StartNode(int port = 0, int wsPort = 0, int minDesiredConnections = Peer.DefaultMinDesiredConnections,
            int maxConnections = Peer.DefaultMaxConnections)
        {
            LocalNode.Tell(new Peer.Start
            {
                Port = port,
                WsPort = wsPort,
                MinDesiredConnections = minDesiredConnections,
                MaxConnections = maxConnections
            });
        }
        /// <summary>
        /// 通过Akka框架通知RPC服务开始工作
        /// </summary>
        /// <param name="bindAddress">RPC服务绑定的IP地址</param>
        /// <param name="port">RPC服务绑定的端口</param>
        /// <param name="wallet">钱包对象</param>
        /// <param name="sslCert">ssl证书</param>
        /// <param name="password">ssl证书密码</param>
        /// <param name="trustedAuthorities">信任名单</param>
        /// <param name="maxGasInvoke">最大gas调用限额</param>
        public void StartRpc(IPAddress bindAddress, int port, Wallet wallet = null, string sslCert = null, string password = null,
            string[] trustedAuthorities = null, Fixed8 maxGasInvoke = default(Fixed8))
        {
            RpcServer = new RpcServer(this, wallet, maxGasInvoke);
            RpcServer.Start(bindAddress, port, sslCert, password, trustedAuthorities);
        }
    }
}
