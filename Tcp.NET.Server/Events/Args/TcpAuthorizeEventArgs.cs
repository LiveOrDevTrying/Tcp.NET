using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpAuthorizeEventArgs<T, U> : AuthorizeBaseEventArgs<T>
        where T : IdentityTcpServer<U>
    {
    }
}

