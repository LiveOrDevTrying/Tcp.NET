using PHS.Networking.Models;
using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public class ConnectionTcp : IConnection
    {
        public TcpClient TcpClient { get; set; }
    }
}
