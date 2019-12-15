using PHS.Core.Models;
using System.Net.Sockets;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;
using Tcp.NET.Server.Handlers;

namespace Tcp.NET.Server
{
    public interface ITcpNETServer : ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>
    {
        bool IsServerRunning { get; }
        Socket Socket { get; }
        TcpHandler TcpHandler { get; }
        bool SendToConnection(PacketDTO packet, ConnectionSocketDTO connection);
        bool SendToConnectionRaw(string message, ConnectionSocketDTO connection);
        bool DisconnectClient(ConnectionSocketDTO connection);
    }
}