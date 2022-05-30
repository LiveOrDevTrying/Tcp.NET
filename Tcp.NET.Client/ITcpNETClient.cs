using PHS.Networking.Services;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public interface ITcpNETClient : 
        ICoreNetworkingClient<
            TcpConnectionClientEventArgs, 
            TcpMessageClientEventArgs, 
            TcpErrorClientEventArgs, 
            ConnectionTcp>
    {
    }
}