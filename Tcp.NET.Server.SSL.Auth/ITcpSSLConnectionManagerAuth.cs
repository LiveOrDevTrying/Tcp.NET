using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Tcp.NET.Server.SSL.Auth.Interfaces;
using Tcp.NET.Server.SSL.Models;

namespace Tcp.NET.Server.SSL.Auth
{
    public interface ITcpSSLConnectionManagerAuth : ITcpSSLConnectionManager
    {
        IUserConnectionTcpClientSSLDTO AddConnectionAuthorized(Guid userId, TcpClient client, StreamReader reader, StreamWriter writer);
        bool AddClientUnauthorized(TcpClient client, StreamReader reader, StreamWriter writer);
        ICollection<IUserConnectionTcpClientSSLDTO> GetAllIdentitiesAuthorized();
        ICollection<ConnectionTcpClientSSLDTO> GetAllClientsUnauthorized();
        IUserConnectionTcpClientSSLDTO GetClientAuthorized(TcpClient client);
        ConnectionTcpClientSSLDTO GetClientUnauthorized(TcpClient client);
        IUserConnectionTcpClientSSLDTO GetIdentity(Guid userId);
        IUserConnectionTcpClientSSLDTO GetIdentity(TcpClient client);
        bool IsClientAuthorized(TcpClient client);
        bool IsClientUnauthorized(TcpClient client);
        bool IsUserConnected(Guid userId);
        void RemoveConnectionAuthorized(ConnectionTcpClientSSLDTO connection);
        void RemoveClientUnauthorized(ConnectionTcpClientSSLDTO client, bool isDisconnect);
    }
}