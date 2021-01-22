using PHS.Networking.Events;
using PHS.Networking.Models;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServerAuth<T> : ICoreNetworking<TcpConnectionServerAuthEventArgs<T>, TcpMessageServerAuthEventArgs<T>, TcpErrorServerAuthEventArgs<T>>
    {
        Task StartAsync();
        Task StopAsync();

        Task BroadcastToAllAuthorizedUsersAsync<S>(S packet) where S : IPacket;
        Task BroadcastToAllAuthorizedUsersAsync(string message);
        Task BroadcastToAllAuthorizedUsersAsync<S>(S packet, IConnectionTcpServer connectionSending) where S : IPacket;
        Task BroadcastToAllAuthorizedUsersAsync(string message, IConnectionTcpServer connectionSending);
        Task BroadcastToAllAuthorizedUsersRawAsync(string message);
        Task SendToUserAsync<S>(S packet, T userId) where S : IPacket;
        Task SendToUserAsync(string message, T userId);
        Task SendToUserRawAsync(string message, T userId);

        bool IsServerRunning { get; }
        TcpListener Server { get; }
        Task<bool> SendToConnectionAsync<S>(S packet, IConnectionTcpServer connection) where S : IPacket;
        Task<bool> SendToConnectionAsync(string message, IConnectionTcpServer connection);
        Task<bool> SendToConnectionRawAsync(string message, IConnectionTcpServer connection);
        Task<bool> DisconnectConnectionAsync(IConnectionTcpServer connection);

        IConnectionTcpServer[] Connections { get; }
        IUserConnectionsTcp<T>[] UserConnections { get; }
        TcpConnectionManagerAuth<T> ConnectionManager { get; }
        event NetworkingEventHandler<ServerEventArgs> ServerEvent;
    }
}