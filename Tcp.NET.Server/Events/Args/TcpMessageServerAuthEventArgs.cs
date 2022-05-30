using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpMessageServerAuthEventArgs<T> : TcpMessageServerBaseEventArgs<IdentityTcpServer<T>>
    {
    }
}
