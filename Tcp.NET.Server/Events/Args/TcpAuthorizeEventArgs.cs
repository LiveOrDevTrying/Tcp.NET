using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpAuthorizeEventArgs<T> : TcpAuthorizeBaseEventArgs<IdentityTcpServer<T>, T>
    {
    }
}

