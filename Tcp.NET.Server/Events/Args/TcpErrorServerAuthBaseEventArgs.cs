using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpErrorServerAuthBaseEventArgs<Z, A> : TcpErrorServerBaseEventArgs<Z> where Z : IdentityTcpServer<A>
    {
    }
}
