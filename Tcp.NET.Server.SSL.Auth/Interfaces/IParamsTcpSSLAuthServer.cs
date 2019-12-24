﻿using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.SSL.Auth.Interfaces
{
    public interface IParamsTcpSSLAuthServer : IParamsTcpServer
    {
        string ConnectionSuccessString { get; set; }
        string UnauthorizedString { get; set; }
    }
}