using PHS.Networking.Server.Services;
using System.Net.Sockets;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServer :
        ICoreNetworkingServer<
            TcpConnectionServerEventArgs, 
            TcpMessageServerEventArgs, 
            TcpErrorServerEventArgs,
            ConnectionTcpServer>
    {
        TcpListener Server { get; }
    }
}