using PHS.Networking.Models;
using PHS.Networking.Services;
using System.Threading.Tasks;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public interface ITcpNETClient : 
        ICoreNetworking<TcpConnectionClientEventArgs, TcpMessageClientEventArgs, TcpErrorClientEventArgs>
    {
        Task ConnectAsync();
        Task<bool> DisconnectAsync();

        Task<bool> SendToServerAsync<T>(T packet) where T : IPacket;
        Task<bool> SendToServerAsync(string message);
        Task<bool> SendToServerRawAsync(string message);

        IConnectionTcp Connection { get; }
        bool IsRunning { get; }
    }
}