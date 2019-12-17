using PHS.Core.Models;
using System.Net.Sockets;
using Tcp.NET.Core.Events.Args;

namespace Tcp.NET.Client
{
    public interface ITcpNETClient : 
        ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>
    {
        void Connect(string url, int port, string endOfLineCharacters);
        void Disconnect();

        bool SendToServer(PacketDTO packet);
        bool SendToServer(string message);
        bool IsConnected { get; }
        Socket Socket { get; }
    }
}