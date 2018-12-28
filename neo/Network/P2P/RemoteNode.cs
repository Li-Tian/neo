using Akka.Actor;
using Akka.Configuration;
using Akka.IO;
using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Actors;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Neo.Network.P2P
{
    // <summary>
    // 描述远程节点的各种属性和功能
    // </summary>
    /// <summary>
    /// Describe the various properties and functions of the remote node
    /// </summary>
    public class RemoteNode : Connection
    {
        internal class Relay { public IInventory Inventory; }

        private readonly NeoSystem system;
        private readonly IActorRef protocol;
        private readonly Queue<Message> message_queue_high = new Queue<Message>();
        private readonly Queue<Message> message_queue_low = new Queue<Message>();
        private ByteString msg_buffer = ByteString.Empty;
        private bool ack = true;
        private BloomFilter bloom_filter;
        private bool verack = false;

        // <summary>
        // 所监听的远端节点的IP和监听的端口
        // </summary>
        /// <summary>
        /// IP and listening port of the remote node being monitored
        /// </summary>
        public IPEndPoint Listener => new IPEndPoint(Remote.Address, ListenerPort);

        // <summary>
        // 监听的端口
        // </summary>
        /// <summary>
        /// listening port
        /// </summary>
        public override int ListenerPort => Version?.Port ?? 0;
        // <summary>
        // 记录当前节点的版本数据和区块高度
        // </summary>
        /// <summary>
        /// Record the current node's version data and block height
        /// </summary>
        public VersionPayload Version { get; private set; }

        // <summary>
        // 构造函数，并向连接的远端节点发送本地节点的版本数据
        // </summary>
        // <param name="system">NEO核心系统类</param>
        // <param name="connection">一个TCP/IP连接对象或一个WebSocket连接对象</param>
        // <param name="remote">远端节点的IP和端口</param>
        // <param name="local">本地节点的IP和端口</param>
        /// <summary>
        /// constructor，create a RemoteNode object and sending the version data of the local node to the connected remote node.
        /// </summary>
        /// <param name="system">Neo core system</param>
        /// <param name="connection">a TCP/IP or WebSocket connection</param>
        /// <param name="remote">IP and port of remote node</param>
        /// <param name="local">IP and port of local node</param>
        public RemoteNode(NeoSystem system, object connection, IPEndPoint remote, IPEndPoint local)
            : base(connection, remote, local)
        {
            this.system = system;
            this.protocol = Context.ActorOf(ProtocolHandler.Props(system));
            LocalNode.Singleton.RemoteNodes.TryAdd(Self, this);
            SendMessage(Message.Create("version", VersionPayload.Create(LocalNode.Singleton.ListenerPort, LocalNode.Nonce, LocalNode.UserAgent, Blockchain.Singleton.Height)));
        }

        private void CheckMessageQueue()
        {
            if (!verack || !ack) return;
            Queue<Message> queue = message_queue_high;
            if (queue.Count == 0) queue = message_queue_low;
            if (queue.Count == 0) return;
            SendMessage(queue.Dequeue());
        }

        private void EnqueueMessage(string command, ISerializable payload = null)
        {
            EnqueueMessage(Message.Create(command, payload));
        }

        private void EnqueueMessage(Message message)
        {
            bool is_single = false;
            switch (message.Command)
            {
                case "addr":
                case "getaddr":
                case "getblocks":
                case "getheaders":
                case "mempool":
                    is_single = true;
                    break;
            }
            Queue<Message> message_queue;
            switch (message.Command)
            {
                case "alert":
                case "consensus":
                case "filteradd":
                case "filterclear":
                case "filterload":
                case "getaddr":
                case "mempool":
                    message_queue = message_queue_high;
                    break;
                default:
                    message_queue = message_queue_low;
                    break;
            }
            if (!is_single || message_queue.All(p => p.Command != message.Command))
                message_queue.Enqueue(message);
            CheckMessageQueue();
        }

        // <summary>
        // 当节点收到ack消息后的回调函数. 将ack标志位设置为true, 然后检查消息队列中的消息, 
        // 并将最早进入消息队列中的最消息弹出并发送,
        // </summary>
        /// <summary>
        /// A callback function to handle ack type message.
        /// Set the ack flag to true, then check the message in the message queue.
        /// and pop up and send the oldest message that first entered the message queue.
        /// </summary>
        protected override void OnAck()
        {
            ack = true;
            CheckMessageQueue();
        }

        // <summary>
        // 解析准备好的数据, 通过actor进行发送
        // </summary>
        // <param name="data">准备发送的的数据</param>
        /// <summary>
        /// Parse the prepared data and send it through the actor reference
        /// </summary>
        /// <param name="data">the prepared data</param>
        protected override void OnData(ByteString data)
        {
            msg_buffer = msg_buffer.Concat(data);
            for (Message message = TryParseMessage(); message != null; message = TryParseMessage())
                protocol.Tell(message);
        }

        // <summary>
        // Akka消息的回调函数, 根据不同消息类型来进行处理
        // </summary>
        // <param name="message">接收到的信息</param>
        /// <summary>
        /// The callback function to processed different types message
        /// </summary>
        /// <param name="message">message</param>
        protected override void OnReceive(object message)
        {
            base.OnReceive(message);
            switch (message)
            {
                case Message msg:
                    EnqueueMessage(msg);
                    break;
                case IInventory inventory:
                    OnSend(inventory);
                    break;
                case Relay relay:
                    OnRelay(relay.Inventory);
                    break;
                case ProtocolHandler.SetVersion setVersion:
                    OnSetVersion(setVersion.Version);
                    break;
                case ProtocolHandler.SetVerack _:
                    OnSetVerack();
                    break;
                case ProtocolHandler.SetFilter setFilter:
                    OnSetFilter(setFilter.Filter);
                    break;
            }
        }

        private void OnRelay(IInventory inventory)
        {
            if (Version?.Relay != true) return;
            if (inventory.InventoryType == InventoryType.TX)
            {
                if (bloom_filter != null && !bloom_filter.Test((Transaction)inventory))
                    return;
            }
            EnqueueMessage("inv", InvPayload.Create(inventory.InventoryType, inventory.Hash));
        }

        private void OnSend(IInventory inventory)
        {
            if (Version?.Relay != true) return;
            if (inventory.InventoryType == InventoryType.TX)
            {
                if (bloom_filter != null && !bloom_filter.Test((Transaction)inventory))
                    return;
            }
            EnqueueMessage(inventory.InventoryType.ToString().ToLower(), inventory);
        }

        private void OnSetFilter(BloomFilter filter)
        {
            bloom_filter = filter;
        }

        private void OnSetVerack()
        {
            verack = true;
            system.TaskManager.Tell(new TaskManager.Register { Version = Version });
            CheckMessageQueue();
        }

        private void OnSetVersion(VersionPayload version)
        {
            this.Version = version;
            if (version.Nonce == LocalNode.Nonce)
            {
                Disconnect(true);
                return;
            }
            if (LocalNode.Singleton.RemoteNodes.Values.Where(p => p != this).Any(p => p.Remote.Address.Equals(Remote.Address) && p.Version?.Nonce == version.Nonce))
            {
                Disconnect(true);
                return;
            }
            SendMessage(Message.Create("verack"));
        }

        // <summary>
        // 停止连接和数据发送, 并将这个远端节点从本地节点的RemoteNodes列表中删除
        // </summary>
        /// <summary>
        /// Stop the connection and data transmission, 
        /// and remove this remote node from the local node's list of RemoteNodes
        /// </summary>
        protected override void PostStop()
        {
            LocalNode.Singleton.RemoteNodes.TryRemove(Self, out _);
            base.PostStop();
        }

        internal static Props Props(NeoSystem system, object connection, IPEndPoint remote, IPEndPoint local)
        {
            return Akka.Actor.Props.Create(() => new RemoteNode(system, connection, remote, local)).WithMailbox("remote-node-mailbox");
        }

        private void SendMessage(Message message)
        {
            ack = false;
            SendData(ByteString.FromBytes(message.ToArray()));
        }

        // <summary>
        // 使用OneForOneStrategy针对异常的那个子actor, 直接终止该子actor.
        // </summary>
        // <returns>返回一个处理错误的SupervisorStrategy用来处理Akka中子actor的错误</returns>
        /// <summary>
        /// Use OneForOneStrategy for anomalous child actor,and directly stop it
        /// </summary>
        /// <returns>Returns a SupervisorStrategy to handle the error of anomalous child actor</returns>
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(ex =>
            {
                Disconnect(true);
                return Directive.Stop;
            }, loggingEnabled: false);
        }

        private Message TryParseMessage()
        {
            if (msg_buffer.Count < sizeof(uint)) return null;
            uint magic = msg_buffer.Slice(0, sizeof(uint)).ToArray().ToUInt32(0);
            if (magic != Message.Magic)
                throw new FormatException();
            if (msg_buffer.Count < Message.HeaderSize) return null;
            int length = msg_buffer.Slice(16, sizeof(int)).ToArray().ToInt32(0);
            if (length > Message.PayloadMaxSize)
                throw new FormatException();
            length += Message.HeaderSize;
            if (msg_buffer.Count < length) return null;
            Message message = msg_buffer.Slice(0, length).ToArray().AsSerializable<Message>();
            msg_buffer = msg_buffer.Slice(length).Compact();
            return message;
        }
    }

    internal class RemoteNodeMailbox : PriorityMailbox
    {
        public RemoteNodeMailbox(Akka.Actor.Settings settings, Config config)
            : base(settings, config)
        {
        }

        protected override bool IsHighPriority(object message)
        {
            switch (message)
            {
                case Tcp.ConnectionClosed _:
                case Connection.Timer _:
                case Connection.Ack _:
                    return true;
                default:
                    return false;
            }
        }
    }
}
