using System;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Auth.Events.Args
{
    public class TcpErrorAuthEventArgs : TcpErrorEventArgs
    {
        public Guid UserId { get; set; }
    }
}
