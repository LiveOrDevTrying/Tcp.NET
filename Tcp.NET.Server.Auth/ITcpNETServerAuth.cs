using PHS.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tcp.NET.Server.Auth.Interfaces;
using Tcp.NET.Server.Auth.Models;

namespace Tcp.NET.Server.SSL.Auth
{
    public interface ITcpNETServerAuth : ITcpNETServer
    {
        ITcpConnectionManagerAuth ConnectionManager { get; }
        IParamsTcpAuthServer Parameters { get; }

        bool BroadcastToAllAuthorizedUsers(PacketDTO packet);
        bool BroadcastToAllAuthorizedUsers(PacketDTO packet, Socket socketSending);
        bool BroadcastToAllAuthorizedUsersRaw(string message);
        ICollection<IUserConnectionTcpDTO> GetAllConnections();
        bool SendToUser(PacketDTO packet, Guid userId);
        bool SendToUserRaw(string message, Guid userId);
    }
}