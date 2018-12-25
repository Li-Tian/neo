using Akka.Actor;
using Akka.IO;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace Neo.Network.P2P
{
    // <summary>
    // 一个抽象类，用于描述本地节点与远端节点建立的一个连接。
    // </summary>
    /// <summary>
    /// An abstract class that describes a connection established between the local node and the remote node.
    /// </summary>
    public abstract class Connection : UntypedActor
    {
        internal class Timer { public static Timer Instance = new Timer(); }
        internal class Ack : Tcp.Event { public static Ack Instance = new Ack(); }
        // <summary>
        // 远端节点的IP和端口
        // </summary>
        /// <summary>
        /// IP and port of remote node
        /// </summary>
        public IPEndPoint Remote { get; }
        // <summary>
        // 本地节点的IP和端口
        // </summary>
        /// <summary>
        /// IP and port of local node
        /// </summary>
        public IPEndPoint Local { get; }
        // <summary>
        // 监听的端口
        // </summary>
        /// <summary>
        /// listening port
        /// </summary>
        public abstract int ListenerPort { get; }

        private ICancelable timer;
        private readonly IActorRef tcp;
        private readonly WebSocket ws;
        private bool disconnected = false;
        // <summary>
        // 构造方法
        // </summary>
        // <param name="connection">一个TCP/IP连接对象或一个WebSocket连接对象</param>
        // <param name="remote">远端节点的IP和端口</param>
        // <param name="local">本地节点的IP和端口</param>
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">a TCP/IP connection object or a WebSocket connection object</param>
        /// <param name="remote">IP and port of remote node</param>
        /// <param name="local">IP and port of local node</param>
        protected Connection(object connection, IPEndPoint remote, IPEndPoint local)
        {
            this.Remote = remote;
            this.Local = local;
            this.timer = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(10), Self, Timer.Instance, ActorRefs.NoSender);
            switch (connection)
            {
                case IActorRef tcp:
                    this.tcp = tcp;
                    break;
                case WebSocket ws:
                    this.ws = ws;
                    WsReceive();
                    break;
            }
        }

        private void WsReceive()
        {
            byte[] buffer = new byte[512];
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
            ws.ReceiveAsync(segment, CancellationToken.None).PipeTo(Self,
                success: p =>
                {
                    switch (p.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            return new Tcp.Received(ByteString.FromBytes(buffer, 0, p.Count));
                        case WebSocketMessageType.Close:
                            return Tcp.PeerClosed.Instance;
                        default:
                            ws.Abort();
                            return Tcp.Aborted.Instance;
                    }
                },
                failure: ex => new Tcp.ErrorClosed(ex.Message));
        }
        // <summary>
        // 断开连接
        // </summary>
        // <param name="abort">是否直接停止</param>
        /// <summary>
        /// Disconnect
        /// </summary>
        /// <param name="abort">whether to stop directly</param>
        public void Disconnect(bool abort = false)
        {
            disconnected = true;
            if (tcp != null)
            {
                tcp.Tell(abort ? (Tcp.CloseCommand)Tcp.Abort.Instance : Tcp.Close.Instance);
            }
            else
            {
                ws.Abort();
            }
            Context.Stop(Self);
        }
        // <summary>
        // 接收到TCP连接传递的ACK信号时的处理方法
        // </summary>
        /// <summary>
        /// Processing method when receiving an ACK signal transmitted by a TCP connection
        /// </summary>
        protected virtual void OnAck()
        {
        }
        // <summary>
        // 处理从网络上的接收到的数据
        // </summary>
        // <param name="data">从网络上的接收到的数据</param>
        /// <summary>
        /// Processing received data from the network
        /// </summary>
        /// <param name="data">received data from the network</param>
        protected abstract void OnData(ByteString data);
        // <summary>
        // 接收到Akka框架传递的消息时的处理方法<br/>
        // 主要处理的消息类型有：<br/>
        // 1、超时<br/>
        // 2、TCP的ACK回应<br/>
        // 3、接收到TCP数据<br/>
        // 4、TCP连接关闭<br/>
        // </summary>
        // <param name="message">Akka框架传递的消息</param>
        /// <summary>
        /// Processing method when receiving a message delivered by the Akka framework<br/>
        /// The main message types are:<br/>
        /// 1、 timeout<br/>
        /// 2、TCP ACK response<br/>
        /// 3、received TCP data<br/>
        /// 4、TCP connection is closed<br/>
        /// </summary>
        /// <param name="message">a message delivered by the Akka framework</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Timer _:
                    Disconnect(true);
                    break;
                case Ack _:
                    OnAck();
                    break;
                case Tcp.Received received:
                    OnReceived(received.Data);
                    break;
                case Tcp.ConnectionClosed _:
                    Context.Stop(Self);
                    break;
            }
        }

        private void OnReceived(ByteString data)
        {
            timer.CancelIfNotNull();
            timer = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMinutes(1), Self, Timer.Instance, ActorRefs.NoSender);
            try
            {
                OnData(data);
            }
            catch
            {
                Disconnect(true);
            }
        }
        // <summary>
        // 停止连接和数据发送。AKKA框架的方法
        // </summary>
        /// <summary>
        /// Stop the connection and sending the data.(AKKA framework method)
        /// </summary>
        protected override void PostStop()
        {
            if (!disconnected)
                tcp?.Tell(Tcp.Close.Instance);
            timer.CancelIfNotNull();
            ws?.Dispose();
            base.PostStop();
        }
        // <summary>
        // 发送消息
        // </summary>
        // <param name="data">待发送的数据</param>
        /// <summary>
        /// SendData
        /// </summary>
        /// <param name="data">data</param>
        protected void SendData(ByteString data)
        {
            if (tcp != null)
            {
                tcp.Tell(Tcp.Write.Create(data, Ack.Instance));
            }
            else
            {
                ArraySegment<byte> segment = new ArraySegment<byte>(data.ToArray());
                ws.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None).PipeTo(Self,
                    success: () => Ack.Instance,
                    failure: ex => new Tcp.ErrorClosed(ex.Message));
            }
        }
    }
}
