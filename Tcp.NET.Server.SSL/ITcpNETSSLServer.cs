using PHS.Core.Models;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Core.SSL.Events.Args;
using Tcp.NET.Server.Handlers;

namespace Tcp.NET.Server
{
    public interface ITcpNETSSLServer : ICoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs>
    {
        bool IsServerRunning { get; }
        TcpHandlerSSL TcpHandler { get; }
        ITcpSSLConnectionManager ConnectionManager { get; }
        Task<bool> SendToClientAsync(PacketDTO packet, TcpClient client);
        Task<bool> SendToClientRawAsync(string message, TcpClient client);
        bool DisconnectClient(TcpClient client);
    }
}