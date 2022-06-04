using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpMessageServerAuthBaseEventArgs<Z, A> : TcpMessageServerBaseEventArgs<Z> where Z : IdentityTcpServer<A>
    {
    }
}
