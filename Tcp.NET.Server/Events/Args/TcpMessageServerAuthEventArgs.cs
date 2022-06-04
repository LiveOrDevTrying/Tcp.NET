using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpMessageServerAuthEventArgs<T> : TcpMessageServerAuthBaseEventArgs<IdentityTcpServer<T>, T>
    {
    }
}
