using Tcp.NET.Server.Events.Args;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpConnectionServerAuthEventArgs<T> : TcpConnectionServerEventArgs
    {
        public T UserId { get; set; }
    }
}

