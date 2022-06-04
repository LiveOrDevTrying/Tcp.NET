namespace Tcp.NET.Server.Models
{
    public class IdentityTcpServer<T> : ConnectionTcpServer
    {
        public T UserId { get; set; }
        public bool IsAuthorized { get; set; }
    }
}
