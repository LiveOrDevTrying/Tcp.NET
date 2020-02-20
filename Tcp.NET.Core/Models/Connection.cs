using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public struct Connection : IConnection
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
    }
}
