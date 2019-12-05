using PHS.Core.Models;
using System.Net.Sockets;
using Tcp.NET.Core.Events.Args;

namespace Tcp.NET.Client
{
    public interface ITcpAsyncClient : 
        ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>
    {
        void Start(string url, int port, string endOfLineCharacters);
        void Stop();

        bool SendToServer(PacketDTO packet);
        bool SendToServer(string message);
        bool SendToServerRaw(string message);
        bool IsConnected { get; }
        Socket Socket { get; }
    }
}