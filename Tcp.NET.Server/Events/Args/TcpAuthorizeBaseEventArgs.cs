using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpAuthorizeBaseEventArgs<T, A> : AuthorizeBaseEventArgs<T>
        where T : IdentityTcpServer<A>
    {
    }
}

