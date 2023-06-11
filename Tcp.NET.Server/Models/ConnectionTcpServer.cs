using PHS.Networking.Server.Models;
using System;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Models
{
    public class ConnectionTcpServer : ConnectionTcp, IConnectionServer
    {
        public string ConnectionId { get; set; }
        public DateTime NextPing { get; set; }
        public bool HasBeenPinged { get; set; }
    }
}
