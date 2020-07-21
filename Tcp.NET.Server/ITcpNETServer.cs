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
    public interface ITcpNETServer : ICoreNetworking<TcpConnectionServerEventArgs, TcpMessageServerEventArgs, TcpErrorServerEventArgs>
    {
        bool IsServerRunning { get; }
        TcpListener Server { get; }

        Task<bool> SendToConnectionAsync<T>(T packet, IConnectionServer connection) where T : IPacket;
        Task<bool> SendToConnectionAsync(string message, IConnectionServer connection);
        Task<bool> SendToConnectionRawAsync(string message, IConnectionServer connection);
        Task<bool> DisconnectConnectionAsync(IConnectionServer connection);

        IConnectionServer[] Connections { get; }

        event NetworkingEventHandler<ServerEventArgs> ServerEvent;
    }
}