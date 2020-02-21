using PHS.Networking.Events;
using PHS.Networking.Models;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServerAuth<T> : ICoreNetworking<TcpConnectionServerAuthEventArgs<T>, TcpMessageServerAuthEventArgs<T>, TcpErrorServerAuthEventArgs<T>>
    {
        Task BroadcastToAllAuthorizedUsersAsync<S>(S packet) where S : IPacket;
        Task BroadcastToAllAuthorizedUsersAsync(string message);
        Task BroadcastToAllAuthorizedUsersAsync<S>(S packet, IConnectionServer connectionSending) where S : IPacket;
        Task BroadcastToAllAuthorizedUsersAsync(string message, IConnectionServer connectionSending);
        Task BroadcastToAllAuthorizedUsersRawAsync(string message);
        Task SendToUserAsync<S>(S packet, T userId) where S : IPacket;
        Task SendToUserAsync(string message, T userId);
        Task SendToUserRawAsync(string message, T userId);

        bool IsServerRunning { get; }
        TcpListener Server { get; }
        Task<bool> SendToConnectionAsync<S>(S packet, IConnectionServer connection) where S : IPacket;
        Task<bool> SendToConnectionAsync(string message, IConnectionServer connection);
        Task<bool> SendToConnectionRawAsync(string message, IConnectionServer connection);
        bool DisconnectConnection(IConnectionServer connection);

        IConnectionServer[] Connections { get; }
        IUserConnections<T>[] UserConnections { get; }

        event NetworkingEventHandler<ServerEventArgs> ServerEvent;
    }
}