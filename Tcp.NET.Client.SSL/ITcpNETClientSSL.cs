using PHS.Core.Models;
using PHS.Core.Networking;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Core.SSL.Events.Args;

namespace Tcp.NET.Client.SSL
{
    public interface ITcpNETClientSSL : 
        ICoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs>, 
        INetworkClient
    {
        void Connect(string url, int port, string endOfLineCharacters);
        void Connect(string url, int port, string endOfLineCharacters, string certificateIssuedTo);
        bool Disconnect();

        Task<bool> SendToServerAsync(PacketDTO packet);
        Task<bool> SendToServerAsync(string message);
        TcpClient Client { get; }
    }
}