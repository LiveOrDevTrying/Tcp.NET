﻿using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Events.Args
{
    public class TcpMessageServerBaseEventArgs<T> : TcpMessageEventArgs<T> where T : ConnectionTcpServer
    {
    }
}
