using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public struct ConnectionServer : IConnectionServer
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
        public bool HasBeenPinged { get; set; }
        public string ConnectionId { get; set; }
    }
}
