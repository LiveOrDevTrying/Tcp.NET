using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServerAuth<T> :
         ITcpNETServerBase<
            TcpConnectionServerAuthEventArgs<T>,
            TcpMessageServerAuthEventArgs<T>,
            TcpErrorServerAuthEventArgs<T>,
            IdentityTcpServer<T>>
    {
        Task SendToUserAsync(string message, T userId, IdentityTcpServer<T> connectionSending = null, CancellationToken cancellationToken = default);
    }
}