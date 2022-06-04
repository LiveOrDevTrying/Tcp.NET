using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpConnectionServerAuthEventArgs<T> : TcpConnectionServerAuthBaseEventArgs<IdentityTcpServer<T>, T>
    {
    }
}

