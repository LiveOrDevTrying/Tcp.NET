using System;
using Tcp.NET.Core.SSL.Events.Args;

namespace Tcp.NET.Server.SSL.Auth.Events.Args
{
    public class TcpSSLErrorAuthEventArgs : TcpSSLErrorEventArgs
    {
        public Guid UserId { get; set; }
    }
}
