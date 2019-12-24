using System;
using Tcp.NET.Core.SSL.Events.Args;

namespace Tcp.NET.Server.SSL.Auth.Events.Args
{
    public class TcpSSLMessageAuthEventArgs : TcpSSLMessageEventArgs
    {
        public Guid UserId { get; set; }
    }
}
