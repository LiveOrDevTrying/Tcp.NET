using PHS.Networking.Server.Models;

namespace Tcp.NET.Server.Models
{
    public class IdentityTcpServer<T> : ConnectionTcpServer, IIdentity<T>
    {
        public T UserId { get; set; }
        public bool IsAuthorized { get; set; }
    }
}
