using PHS.Networking.Server.Managers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public abstract class TcpConnectionManagerAuthBase<Z, A> : ConnectionManagerAuth<Z, A> where Z : IdentityTcpServer<A>
    {
    }
}
