using Akka.Actor;
using Akka.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Neo.Network.P2P
{
    // <summary>
    // NEO所用P2P网络中的peer类.描述节点的基本的网络功能
    // </summary>
    /// <summary>
    /// The peer class in the P2P network used by NEO. 
    /// Describe the basic network functions of the node.
    /// </summary>
    public abstract class Peer : UntypedActor
    {
        // <summary>
        // 自定义Akka消息类型，代表节点的启动,描述了节点的监听端口、连接数等属性
        // </summary>
        /// <summary>
        /// Custom Akka message type, which represents the startup of the node, describes the node's listening port, number of connections, etc.
        /// </summary>
        public class Start {
            // <summary>
            // tcp/ip监听端口号
            // </summary>
            /// <summary>
            /// tcp/ip listening port
            /// </summary>
            public int Port;
            // <summary>
            //  WebSocket监听端口
            // </summary>
            /// <summary>
            ///  WebSocket listening port
            /// </summary>
            public int WsPort;
            // <summary>
            // 最小需要的连接数
            // </summary>
            /// <summary>
            /// min desired connection amount
            /// </summary>
            public int MinDesiredConnections;
            // <summary>
            // 最大的连接数
            // </summary>
            /// <summary>
            /// max connection amount
            /// </summary>
            public int MaxConnections;
        }
        // <summary>
        // 自定义Akka消息类型，代表添加未连接节点列表,描述需要添加到未连接节点列表的节点.
        // </summary>
        /// <summary>
        /// Custom Akka message type, which represents adding a list of unconnected nodes, describing the nodes that need to be added to the list of unconnected nodes.
        /// </summary>
        public class Peers {
            // <summary>
            // 所有peer节点IPEndPoint的集合
            // </summary>
            /// <summary>
            /// a collection of peer nodes 's IP and Point
            /// </summary>
            public IEnumerable<IPEndPoint> EndPoints;
        }
        // <summary>
        // 自定义Akka消息类型，代表连接节点,描述了连接到节点的IP和端口、是否可信等属性
        // </summary>
        /// <summary>
        /// Custom Akka message type, which represents the connection node, describes the IP and port connected to the node, whether it is trusted, etc.
        /// </summary>
        public class Connect {
            // <summary>
            //  连接到的Peer的IPEndPoint
            // </summary>
            /// <summary>
            ///  the IP and Point of the node which connects to.
            /// </summary>
            public IPEndPoint EndPoint;
            /// <summary>
            /// Determine if the connected node is trusted
            /// </summary>
            public bool IsTrusted = false;
        }
        private class Timer { }
        private class WsConnected { public WebSocket Socket; public IPEndPoint Remote; public IPEndPoint Local; }

        private const int MaxConnectionsPerAddress = 3;
        // <summary>
        // 默认最小需要连接数，默认值为10
        // </summary>
        /// <summary>
        /// Default min desired connection amount，default value is 10
        /// </summary>
        public const int DefaultMinDesiredConnections = 10;
        // <summary>
        // 默认最大连接数，默认是默认最小需要连接数*4
        // </summary>
        /// <summary>
        /// Default max connection amount，default value is DefaultMinDesiredConnections*4
        /// </summary>
        public const int DefaultMaxConnections = DefaultMinDesiredConnections * 4;

        private static readonly IActorRef tcp_manager = Context.System.Tcp();
        private IActorRef tcp_listener;
        private IWebHost ws_host;
        private ICancelable timer;
        // <summary>
        // 获取所有活动连接
        // </summary>
        /// <summary>
        /// Get all active connections
        /// </summary>
        protected ActorSelection Connections => Context.ActorSelection("connection_*");

        private static readonly HashSet<IPAddress> localAddresses = new HashSet<IPAddress>();
        private readonly Dictionary<IPAddress, int> ConnectedAddresses = new Dictionary<IPAddress, int>();
        // <summary>
        // 活动的连接的关系映射。key是活动连接的引用，value是活动连接的远程IP地址和端口号。
        // </summary>
        /// <summary>
        /// Dictionary of active connections.Key is the reference to the active connection, and value is the remote IP address and port number of the active connection.
        /// </summary>
        protected readonly ConcurrentDictionary<IActorRef, IPEndPoint> ConnectedPeers = new ConcurrentDictionary<IActorRef, IPEndPoint>();

        // <summary>
        // 未连接的已知节点集合
        // </summary>
        /// <summary>
        /// a unconnected known node set
        /// </summary>
        protected ImmutableHashSet<IPEndPoint> UnconnectedPeers = ImmutableHashSet<IPEndPoint>.Empty;
        // <summary>
        // 正在连接中的已知节点集合
        // </summary>
        /// <summary>
        /// a connecting known node set
        /// </summary>
        protected ImmutableHashSet<IPEndPoint> ConnectingPeers = ImmutableHashSet<IPEndPoint>.Empty;
        // <summary>
        // 可信任的IP地址集合。在连接数达到最大连接数以后，仍然被允许创建连接的地址集合。
        // </summary>
        // <value>
        // 返回可信任的IP地址集合
        // </value>
        /// <summary>
        /// A collection of trusted IP addresses. 
        /// After the number of connections reaches the max amount of connections, 
        /// it is still allowed to create a connection to the  address in the collection.
        /// </summary>
        /// <value>
        /// A collection of trusted IP addresses
        /// </value>
        protected HashSet<IPAddress> TrustedIpAddresses { get; } = new HashSet<IPAddress>();

        // <summary>
        // 监听端口
        // </summary>
        // <value>返回这个Peer对象</value>
        /// <summary>
        /// Listening port
        /// </summary>
        /// <value>Return this Peer object</value>
        public int ListenerPort { get; private set; }
        // <summary>
        // 当前peer需要的连接的最小值.默认值为10.
        // </summary>
        // <value>返回peer需要的连接的最小值</value>
        /// <summary>
        /// Min desired connection amount.Default value is 10.
        /// </summary>
        /// <value>Min desired connection amount</value>
        public int MinDesiredConnections { get; private set; } = DefaultMinDesiredConnections;
        // <summary>
        // 当前peer的最大连接数
        // </summary>
        /// <summary>
        /// Max connection amount
        /// </summary>
        public int MaxConnections { get; private set; } = DefaultMaxConnections;

        // <summary>
        // 未连接的peer列表中peer个数的最大值. 默认1000
        // </summary>
        // <value>未连接的peer列表中peer个数的最大值</value>
        /// <summary>
        /// The max number of peers in the unconnected peer list. Default 1000
        /// </summary>
        /// <value>未连接的peer列表中peer个数的最大值</value>
        protected int UnconnectedMax { get; } = 1000;
        // <summary>
        // 允许的正在连接中的连接数
        // </summary>
        /// <summary>
        /// max connecting amount
        /// </summary>
        protected virtual int ConnectingMax
        {
            get
            {
                var allowedConnecting = MinDesiredConnections * 4;
                allowedConnecting = MaxConnections != -1 && allowedConnecting > MaxConnections 
                    ? MaxConnections : allowedConnecting; 
                return allowedConnecting - ConnectedPeers.Count;
            }
        }
        // <summary>
        // 静态的启动模块
        // </summary>
        /// <summary>
        /// static boot module
        /// </summary>
        static Peer()
        {
            localAddresses.UnionWith(NetworkInterface.GetAllNetworkInterfaces().SelectMany(p => p.GetIPProperties().UnicastAddresses).Select(p => p.Address.Unmap()));
        }

        // <summary>
        // 将一个peer集合添加到本地未连接的peer列表中.
        // </summary>
        // <param name="peers">被添加的peer集合</param>
        /// <summary>
        /// Add a peer collection to the list of local unconnected peers.
        /// </summary>
        /// <param name="peers">被添加的peer集合</param>
        protected void AddPeers(IEnumerable<IPEndPoint> peers)
        {
            if (UnconnectedPeers.Count < UnconnectedMax)
            {
                peers = peers.Where(p => p.Port != ListenerPort || !localAddresses.Contains(p.Address));
                ImmutableInterlocked.Update(ref UnconnectedPeers, p => p.Union(peers));
            }
        }

        // <summary>
        // 指定一个Peer的IPEndPoint并进行连接.如果连接成功则加入已连接的peer表中.
        // 如果该Peer节点是可信任的,则将该节点的IP地址加入本地的可信任地址列表中.
        // </summary>
        // <param name="endPoint">需要连接的Peer</param>
        // <param name="isTrusted">该Peer节点是否是可信任的（保留）</param>
        /// <summary>
        /// Specify a Peer's IPEndPoint and try to connect. 
        /// If connect successfully, add it to the connected peer list.
        /// If the Peer node is trusted, add the node's IP address to the local trusted address list.
        /// </summary>
        /// <param name="endPoint">a specify Peer's IPEndPoint</param>
        /// <param name="isTrusted">whether the Peer node is trusted（Reserved）</param>
        protected void ConnectToPeer(IPEndPoint endPoint, bool isTrusted = false)
        {
            endPoint = endPoint.Unmap();
            if (endPoint.Port == ListenerPort && localAddresses.Contains(endPoint.Address)) return;

            if (isTrusted) TrustedIpAddresses.Add(endPoint.Address);
            if (ConnectedAddresses.TryGetValue(endPoint.Address, out int count) && count >= MaxConnectionsPerAddress)
                return;
            if (ConnectedPeers.Values.Contains(endPoint)) return;
            ImmutableInterlocked.Update(ref ConnectingPeers, p =>
            {
                if ((p.Count >= ConnectingMax && !isTrusted) || p.Contains(endPoint)) return p;
                tcp_manager.Tell(new Tcp.Connect(endPoint));
                return p.Add(endPoint);
            });
        }

        private static bool IsIntranetAddress(IPAddress address)
        {
            byte[] data = address.MapToIPv4().GetAddressBytes();
            Array.Reverse(data);
            uint value = data.ToUInt32(0);
            return (value & 0xff000000) == 0x0a000000 || (value & 0xff000000) == 0x7f000000 || (value & 0xfff00000) == 0xac100000 || (value & 0xffff0000) == 0xc0a80000 || (value & 0xffff0000) == 0xa9fe0000;
        }
        // <summary>
        // 尝试获取更多的节点
        // </summary>
        // <param name="count">需求的数量</param>
        /// <summary>
        /// Try to get more nodes
        /// </summary>
        /// <param name="count">demand count</param>
        protected abstract void NeedMorePeers(int count);

        // <summary>
        // 用来Akka消息的回调方法
        // </summary>
        // <param name="message">接收到的消息</param>
        /// <summary>
        /// Callback method for handling Akka messages
        /// </summary>
        /// <param name="message">message</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Start start:
                    OnStart(start.Port, start.WsPort, start.MinDesiredConnections, start.MaxConnections);
                    break;
                case Timer _:
                    OnTimer();
                    break;
                case Peers peers:
                    AddPeers(peers.EndPoints);
                    break;
                case Connect connect:
                    ConnectToPeer(connect.EndPoint, connect.IsTrusted);
                    break;
                case WsConnected ws:
                    OnWsConnected(ws.Socket, ws.Remote, ws.Local);
                    break;
                case Tcp.Connected connected:
                    OnTcpConnected(((IPEndPoint)connected.RemoteAddress).Unmap(), ((IPEndPoint)connected.LocalAddress).Unmap());
                    break;
                case Tcp.Bound _:
                    tcp_listener = Sender;
                    break;
                case Tcp.CommandFailed commandFailed:
                    OnTcpCommandFailed(commandFailed.Cmd);
                    break;
                case Terminated terminated:
                    OnTerminated(terminated.ActorRef);
                    break;
            }
        }

        private void OnStart(int port, int wsPort, int minDesiredConnections, int maxConnections)
        {
            ListenerPort = port;
            MinDesiredConnections = minDesiredConnections;
            MaxConnections = maxConnections;
            timer = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(0, 5000, Context.Self, new Timer(), ActorRefs.NoSender);
            if ((port > 0 || wsPort > 0)
                && localAddresses.All(p => !p.IsIPv4MappedToIPv6 || IsIntranetAddress(p))
                && UPnP.Discover())
            {
                try
                {
                    localAddresses.Add(UPnP.GetExternalIP());
                    if (port > 0)
                        UPnP.ForwardPort(port, ProtocolType.Tcp, "NEO");
                    if (wsPort > 0)
                        UPnP.ForwardPort(wsPort, ProtocolType.Tcp, "NEO WebSocket");
                }
                catch { }
            }
            if (port > 0)
            {
                tcp_manager.Tell(new Tcp.Bind(Self, new IPEndPoint(IPAddress.Any, port), options: new[] { new Inet.SO.ReuseAddress(true) }));
            }
            if (wsPort > 0)
            {
                ws_host = new WebHostBuilder().UseKestrel().UseUrls($"http://*:{wsPort}").Configure(app => app.UseWebSockets().Run(ProcessWebSocketAsync)).Build();
                ws_host.Start();
            }
        }

        private void OnTcpConnected(IPEndPoint remote, IPEndPoint local)
        {
            ImmutableInterlocked.Update(ref ConnectingPeers, p => p.Remove(remote));
            if (MaxConnections != -1 && ConnectedPeers.Count >= MaxConnections && !TrustedIpAddresses.Contains(remote.Address))
            {
                Sender.Tell(Tcp.Abort.Instance);
                return;
            }
            
            ConnectedAddresses.TryGetValue(remote.Address, out int count);
            if (count >= MaxConnectionsPerAddress)
            {
                Sender.Tell(Tcp.Abort.Instance);
            }
            else
            {
                ConnectedAddresses[remote.Address] = count + 1;
                IActorRef connection = Context.ActorOf(ProtocolProps(Sender, remote, local), $"connection_{Guid.NewGuid()}");
                Context.Watch(connection);
                Sender.Tell(new Tcp.Register(connection));
                ConnectedPeers.TryAdd(connection, remote);
            }
        }

        private void OnTcpCommandFailed(Tcp.Command cmd)
        {
            switch (cmd)
            {
                case Tcp.Connect connect:
                    ImmutableInterlocked.Update(ref ConnectingPeers, p => p.Remove(((IPEndPoint)connect.RemoteAddress).Unmap()));
                    break;
            }
        }

        private void OnTerminated(IActorRef actorRef)
        {
            if (ConnectedPeers.TryRemove(actorRef, out IPEndPoint endPoint))
            {
                ConnectedAddresses.TryGetValue(endPoint.Address, out int count);
                if (count > 0) count--;
                if (count == 0)
                    ConnectedAddresses.Remove(endPoint.Address);
                else
                    ConnectedAddresses[endPoint.Address] = count;
            }
        }

        private void OnTimer()
        {
            if (ConnectedPeers.Count >= MinDesiredConnections) return;
            if (UnconnectedPeers.Count == 0)
                NeedMorePeers(MinDesiredConnections - ConnectedPeers.Count);
            IPEndPoint[] endpoints = UnconnectedPeers.Take(MinDesiredConnections - ConnectedPeers.Count).ToArray();
            ImmutableInterlocked.Update(ref UnconnectedPeers, p => p.Except(endpoints));
            foreach (IPEndPoint endpoint in endpoints)
            {
                ConnectToPeer(endpoint);
            }
        }

        private void OnWsConnected(WebSocket ws, IPEndPoint remote, IPEndPoint local)
        {
            ConnectedAddresses.TryGetValue(remote.Address, out int count);
            if (count >= MaxConnectionsPerAddress)
            {
                ws.Abort();
            }
            else
            {
                ConnectedAddresses[remote.Address] = count + 1;
                Context.ActorOf(ProtocolProps(ws, remote, local), $"connection_{Guid.NewGuid()}");
            }
        }

        // <summary>
        //停止传递消息
        // </summary>
        /// <summary>
        ///  Stop delivering messages
        /// </summary>
        protected override void PostStop()
        {
            timer.CancelIfNotNull();
            ws_host?.Dispose();
            tcp_listener?.Tell(Tcp.Unbind.Instance);
            base.PostStop();
        }

        private async Task ProcessWebSocketAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest) return;
            WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
            Self.Tell(new WsConnected
            {
                Socket = ws,
                Remote = new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort),
                Local = new IPEndPoint(context.Connection.LocalIpAddress, context.Connection.LocalPort)
            });
        }
        // <summary>
        // 通过一个活动的TCP/IP连接来创建一个RemoteNode对象，并返回它对应的Props对象
        // </summary>
        // <param name="connection">活动的连接</param>
        // <param name="remote">连接的远端IP地址和端口</param>
        // <param name="local">连接的本地地址和端口</param>
        // <returns>RemoteNode对象对应的Props</returns>
        /// <summary>
        /// Create a RemoteNode object by an active TCP/IP connection and return its corresponding Props object
        /// </summary>
        /// <param name="connection">active connection</param>
        /// <param name="remote">IP address of remote node</param>
        /// <param name="local">IP address of local node</param>
        /// <returns>corresponding Props object</returns>
        protected abstract Props ProtocolProps(object connection, IPEndPoint remote, IPEndPoint local);
    }
}
