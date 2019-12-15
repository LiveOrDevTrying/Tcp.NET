using System;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Auth.Events.Args
{
    public class TcpMessageAuthEventArgs : TcpMessageEventArgs
    {
        public Guid UserId { get; set; }
    }
}
