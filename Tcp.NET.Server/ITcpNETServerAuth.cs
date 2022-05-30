using PHS.Networking.Server.Services;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServerAuth<T> :
         ICoreNetworkingServer<
            TcpConnectionServerAuthEventArgs<T>,
            TcpMessageServerAuthEventArgs<T>,
            TcpErrorServerAuthEventArgs<T>,
            IdentityTcpServer<T>>
    {
        Task SendToUserAsync(string message, T userId, CancellationToken cancellationToken = default);
        Task SendToUserAsync(byte[] message, T userId, CancellationToken cancellationToken = default);

        TcpListener Server { get; }
    }
}