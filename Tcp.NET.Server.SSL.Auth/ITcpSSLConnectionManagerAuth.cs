using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Tcp.NET.Server.Models;
using Tcp.NET.Server.SSL.Auth.Interfaces;
using Tcp.NET.Server.SSL.Auth.Models;

namespace Tcp.NET.Server.SSL.Auth
{
    public interface ITcpSSLConnectionManagerAuth : ITcpSSLConnectionManager
    {
        IUserConnectionTcpClientSSLDTO AddConnectionAuthorized(Guid userId, TcpClient client, StreamReader reader, StreamWriter writer);
        bool AddClientUnauthorized(TcpClient client);
        ICollection<IUserConnectionTcpClientSSLDTO> GetAllIdentitiesAuthorized();
        ICollection<TcpClient> GetAllClientsUnauthorized();
        IUserConnectionTcpClientSSLDTO GetConnectionAuthorized(TcpClient client);
        IUserConnectionTcpClientSSLDTO GetIdentity(Guid userId);
        IUserConnectionTcpClientSSLDTO GetIdentity(TcpClient client);
        bool IsConnectionAuthorized(TcpClient client);
        bool IsConnectionUnauthorized(TcpClient client);
        bool IsUserConnected(Guid userId);
        void RemoveConnectionAuthorized(ConnectionTcpClientSSLDTO connection);
        void RemoveClientUnauthorized(TcpClient client, bool isDisconnect);
    }
}