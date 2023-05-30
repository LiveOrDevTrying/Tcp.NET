using Tcp.NET.Server.Models;
using PHS.Core.Events.Args;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpAuthorizeBaseEventArgs<Z, A> : BaseArgs
        where Z : IdentityTcpServer<A>
    {
        public Z Connection { get; set; }
        public string Token { get; set; }
    }
}

