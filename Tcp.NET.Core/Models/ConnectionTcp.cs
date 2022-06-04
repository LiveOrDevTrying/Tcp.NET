using PHS.Networking.Models;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public class ConnectionTcp : IConnection
    {
        public string ConnectionId { get; set; }
        public TcpClient TcpClient { get; set; }
    }
}
