using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public class ConnectionSocketDTO
    {
        public Socket Socket { get; set; }
        public bool HasBeenPinged { get; set; }
    }
}
