using Tcp.NET.Server.Events.Args;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpMessageServerAuthEventArgs<T> : TcpMessageServerEventArgs
    {
        public T UserId { get; set; }
    }
}
