using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public class ConnectionSocketDTO
    {
        public Socket Socket { get; set; }
        public bool HasBeenPinged { get; set; }
    }
}
