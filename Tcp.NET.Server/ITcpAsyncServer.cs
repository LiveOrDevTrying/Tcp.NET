using PHS.Core.Models;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Tcp.NET.Server
{
    public interface ITcpAsyncServer : ICoreNetworking<TcpConnectionAuthEventArgs, TcpMessageAuthEventArgs, TcpErrorAuthEventArgs>
    {
        bool IsServerRunning { get; }
        Socket Socket { get;
        }
        ICollection<IUserConnectionTcpDTO> GetAllConnections();

        bool BroadcastToAllAuthorizedUsers(PacketDTO packet);
        bool BroadcastToAllAuthorizedUsers(PacketDTO packet, Socket socketSending);
        bool BroadcastToAllAuthorizedUsersRaw(string message);
        bool SendToSocket(PacketDTO packet, Socket socket);
        bool SendToSocketRaw(string message, Socket socket);
        bool SendToUser(PacketDTO packet, Guid userId);
        bool SendToUserRaw(string message, Guid userId);
        bool DisconnectClient(Socket socket);
    }
}