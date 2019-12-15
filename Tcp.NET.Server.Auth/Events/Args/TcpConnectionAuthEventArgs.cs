using System;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server.Auth.Enums;

namespace Tcp.NET.Server.Auth.Events.Args
{
    public class TcpConnectionAuthEventArgs : TcpConnectionEventArgs
    {
        public Guid UserId { get; set; }
        public TcpConnectionAuthType ConnectionAuthType { get; set; }
    }
}

