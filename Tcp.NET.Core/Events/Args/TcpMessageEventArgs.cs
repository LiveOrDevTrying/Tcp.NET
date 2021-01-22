﻿using PHS.Networking.Events.Args;
using PHS.Networking.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpMessageEventArgs<T> : MessageEventArgs where T : IConnectionTcp
    {
        public T Connection { get; set; }
    }
}
