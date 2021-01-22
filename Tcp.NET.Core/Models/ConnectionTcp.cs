using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public class ConnectionTcp : IConnectionTcp
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
    }
}
