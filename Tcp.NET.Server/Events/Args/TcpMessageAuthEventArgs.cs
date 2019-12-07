using System;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpMessageAuthEventArgs : TcpMessageEventArgs
    {
        public ConnectionSocketDTO ConnectionSocket { get; set; }
        public Guid UserId { get; set; }
    }
}
