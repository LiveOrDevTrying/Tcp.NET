using PHS.Core.Models;
using PHS.Core.Networking;
using System.Net.Sockets;
using Tcp.NET.Core.Events.Args;

namespace Tcp.NET.Client
{
    public interface ITcpNETClient : 
        ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>, 
        INetworkClient
    {
        void Connect(string url, int port, string endOfLineCharacters);
        bool Disconnect();

        bool SendToServer(PacketDTO packet);
        bool SendToServer(string message);
        Socket Socket { get; }
    }
}