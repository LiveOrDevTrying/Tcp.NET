using PHS.Core.Models;
using System.Net.Sockets;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServer : ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>
    {
        bool IsServerRunning { get; }
        Socket Socket { get; }
        TcpHandler TcpHandler { get; }
        bool SendToConnection(PacketDTO packet, Socket socket);
        bool SendToConnectionRaw(string message, Socket socket);
        bool DisconnectClient(Socket socket);
    }
}