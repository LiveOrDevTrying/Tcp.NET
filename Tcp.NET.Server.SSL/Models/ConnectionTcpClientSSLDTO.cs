using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public class ConnectionTcpClientSSLDTO
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
        public bool HasBeenPinged { get; set; }
    }
}
