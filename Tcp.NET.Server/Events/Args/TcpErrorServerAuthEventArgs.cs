using Tcp.NET.Server.Events.Args;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpErrorServerAuthEventArgs<T> : TcpErrorServerEventArgs
    {
        public T UserId { get; set; }
    }
}
