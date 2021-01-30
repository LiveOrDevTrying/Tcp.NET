using PHS.Networking.Server.Models;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public interface IIdentityTcp<T> : IIdentity<T, IConnectionTcpServer>
    {
        IConnectionTcpServer GetConnection(TcpClient client);
    }
}