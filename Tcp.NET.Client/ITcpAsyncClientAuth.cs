using PHS.Core.Models;
using Tcp.NET.Core.Events.Args;

namespace Tcp.NET.Client
{
    public interface ITcpAsyncClientAuth : ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>
    {
        bool SendToServer(string data);

        bool IsConnected { get; }
    }
}