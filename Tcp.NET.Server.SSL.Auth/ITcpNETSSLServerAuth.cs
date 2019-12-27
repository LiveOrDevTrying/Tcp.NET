using PHS.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Server.SSL.Auth.Interfaces;

namespace Tcp.NET.Server.SSL.Auth
{
    public interface ITcpNETSSLServerAuth : ITcpNETSSLServer
    {
        new ITcpSSLConnectionManager ConnectionManager { get; }
        Task<bool> BroadcastToAllAuthorizedUsersAsync(PacketDTO packet);
        Task<bool> BroadcastToAllAuthorizedUsersAsync(PacketDTO packet, TcpClient client);
        Task<bool> BroadcastToAllAuthorizedUsersRawAsync(string message);
        ICollection<IUserConnectionTcpClientSSLDTO> GetAllConnections();
        Task<bool> SendToUserAsync(PacketDTO packet, Guid userId);
        Task<bool> SendToUserRawAsync(string message, Guid userId);
    }
}