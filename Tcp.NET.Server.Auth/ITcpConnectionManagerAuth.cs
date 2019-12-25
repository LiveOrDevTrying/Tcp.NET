using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tcp.NET.Server.Auth.Interfaces;
using Tcp.NET.Server.Auth.Models;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Auth
{
    public interface ITcpConnectionManagerAuth : ITcpConnectionManager
    {
        IUserConnectionTcpDTO AddConnectionAuthorized(Guid userId, Socket socket);
        bool AddSocketUnauthorized(Socket socket);
        ICollection<IUserConnectionTcpDTO> GetAllIdentitiesAuthorized();
        ICollection<Socket> GetAllSocketsUnauthorized();
        ConnectionSocketDTO GetConnectionAuthorized(Socket socket);
        IUserConnectionTcpDTO GetIdentity(Guid userId);
        IUserConnectionTcpDTO GetIdentity(Socket socket);
        bool IsConnectionAuthorized(Socket socket);
        bool IsConnectionUnauthorized(Socket socket);
        bool IsUserConnected(Guid userId);
        void RemoveConnectionAuthorized(ConnectionSocketDTO connection);
        void RemoveSocketUnauthorized(Socket socket, bool isDisconnect);
    }
}