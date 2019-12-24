using System;
using Tcp.NET.Core.SSL.Events.Args;
using Tcp.NET.Server.SSL.Auth.Enums;

namespace Tcp.NET.Server.SSL.Auth.Events.Args
{
    public class TcpSSLConnectionAuthEventArgs : TcpSSLConnectionEventArgs
    {
        public Guid UserId { get; set; }
        public TcpSSLConnectionAuthType ConnectionAuthType { get; set; }
    }
}

