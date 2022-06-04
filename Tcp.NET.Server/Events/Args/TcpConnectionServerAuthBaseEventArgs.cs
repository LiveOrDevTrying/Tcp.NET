using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpConnectionServerAuthBaseEventArgs<Z, A> : TcpConnectionServerBaseEventArgs<Z> where Z : IdentityTcpServer<A>
    {
    }
}

