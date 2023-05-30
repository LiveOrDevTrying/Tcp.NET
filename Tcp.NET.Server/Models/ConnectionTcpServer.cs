using System;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Models
{
    public class ConnectionTcpServer : ConnectionTcp
    {
        public DateTime NextPing { get; set; }
        public bool HasBeenPinged { get; set; }
    }
}
