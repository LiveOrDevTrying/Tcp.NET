using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Models
{
    public class ConnectionTcpServer : ConnectionTcp
    {
        public bool HasBeenPinged { get; set; }
        public string ConnectionId { get; set; }
    }
}
