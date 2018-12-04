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
    /// <summary>
    /// NEO所用P2P网络中的peer类.
    /// </summary>
    public abstract class Peer : UntypedActor
    {
        /// <summary>
        /// 内部Start类
        /// </summary>
        public class Start {
            /// <summary>
            /// 端口号
            /// </summary>
            public int Port;
            /// <summary>
            ///  WS-RPC监听端口
            /// </summary>
            public int WsPort;
            /// <summary>
            /// 最小需要的连接数
            /// </summary>
            public int MinDesiredConnections;
            /// <summary>
            /// 最多的连接数
            /// </summary>
            public int MaxConnections;
        }
        /// <summary>
        /// 内部Peers类.
        /// </summary>
        public class Peers {
            /// <summary>
            /// 所有peer节点IPEndPoint的集合
            /// </summary>
            public IEnumerable<IPEndPoint> EndPoints;
        }
        /// <summary>
        /// 内部Connect类
        /// </summary>
        public class Connect {
            /// <summary>
            ///  连接到的Peer的IPEndPoint
            /// </summary>
            public IPEndPoint EndPoint;
            /// <summary>
            /// 判断连接的节点是否可信
            /// </summary>
            public bool IsTrusted = false;
        }
        private class Timer { }
        private class WsConnected { public WebSocket Socket; public IPEndPoint Remote; public IPEndPoint Local; }

        private const int MaxConnectionsPerAddress = 3;
        public const int DefaultMinDesiredConnections = 10;
        public const int DefaultMaxConnections = DefaultMinDesiredConnections * 4;

        private static readonly IActorRef tcp_manager = Context.System.Tcp();
        private IActorRef tcp_listener;
        private IWebHost ws_host;
        private ICancelable timer;
        protected ActorSelection Connections => Context.ActorSelection("connection_*");

        private static readonly HashSet<IPAddress> localAddresses = new HashSet<IPAddress>();
        private readonly Dictionary<IPAddress, int> ConnectedAddresses = new Dictionary<IPAddress, int>();
        protected readonly ConcurrentDictionary<IActorRef, IPEndPoint> ConnectedPeers = new ConcurrentDictionary<IActorRef, IPEndPoint>();
        protected ImmutableHashSet<IPEndPoint> UnconnectedPeers = ImmutableHashSet<IPEndPoint>.Empty;
        protected ImmutableHashSet<IPEndPoint> ConnectingPeers = ImmutableHashSet<IPEndPoint>.Empty;
        /// <summary>
        /// 可信任的IP地址集合
        /// </summary>
        /// <value>
        /// 返回可信任的IP地址集合
        /// </value>
        protected HashSet<IPAddress> TrustedIpAddresses { get; } = new HashSet<IPAddress>();
        
        /// <summary>
        /// 监听接口
        /// </summary>
        /// <value>返回这个Peer对象</value>
        public int ListenerPort { get; private set; }
        /// <summary>
        /// 当前peer需要的连接的最小值.默认值为10.
        /// </summary>
        /// <value>返回peer需要的连接的最小值</value>
        public int MinDesiredConnections { get; private set; } = DefaultMinDesiredConnections;
        public int MaxConnections { get; private set; } = DefaultMaxConnections;

        /// <summary>
        /// 未连接的peer列表中peer个数的最大值. 默认1000
        /// </summary>
        /// <value>未连接的peer列表中peer个数的最大值</value>
        protected int UnconnectedMax { get; } = 1000;
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

        static Peer()
        {
            localAddresses.UnionWith(NetworkInterface.GetAllNetworkInterfaces().SelectMany(p => p.GetIPProperties().UnicastAddresses).Select(p => p.Address.Unmap()));
        }

        /// <summary>
        /// 将一个peer集合添加到本地未连接的peer列表中.
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

        /// <summary>
        /// 指定一个Peer的IPEndPoint并进行连接.如果连接成功则加入已连接的peer表中.
        /// 如果改Peer节点是可信任的,则将该节点的IP地址加入本地的可信任地址列表中.
        /// </summary>
        /// <param name="endPoint">需要连接的Peer</param>
        /// <param name="isTrusted">该Peer节点是否是可信任的</param>
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

        protected abstract void NeedMorePeers(int count);

        /// <summary>
        /// 用来监听消息的回调方法
        /// </summary>
        /// <param name="message">接收到的消息</param>
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

        /// <summary>
        ///停止传递消息
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

        protected abstract Props ProtocolProps(object connection, IPEndPoint remote, IPEndPoint local);
    }
}
