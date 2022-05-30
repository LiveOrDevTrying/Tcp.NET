using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpErrorServerAuthEventArgs<T> : TcpErrorServerBaseEventArgs<IdentityTcpServer<T>>
    {
    }
}
