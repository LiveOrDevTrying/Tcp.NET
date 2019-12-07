using System;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpErrorAuthEventArgs : TcpErrorEventArgs
    {
        public Guid UserId { get; set; }
        public ConnectionSocketDTO ConnectionSocket { get; set; }
    }
}
