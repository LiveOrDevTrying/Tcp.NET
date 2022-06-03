using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpAuthorizeBaseEventArgs<Z, A> : AuthorizeBaseEventArgs<Z>
        where Z : IdentityTcpServer<A>
    {
    }
}

