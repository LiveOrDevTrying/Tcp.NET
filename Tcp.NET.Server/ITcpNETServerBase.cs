using PHS.Networking.Server.Services;
using System.Net.Sockets;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServerBase<T, U, V, W> : ICoreNetworkingServer<T, U, V, W>
        where T : TcpConnectionEventArgs<W>
        where U : TcpMessageEventArgs<W>
        where V : TcpErrorEventArgs<W>
        where W : ConnectionTcpServer
    {
        TcpListener Server { get; }
    }
}