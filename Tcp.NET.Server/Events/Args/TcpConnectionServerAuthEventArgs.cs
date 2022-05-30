using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpConnectionServerAuthEventArgs<T> : TcpConnectionServerBaseEventArgs<IdentityTcpServer<T>>
    {
    }
}

