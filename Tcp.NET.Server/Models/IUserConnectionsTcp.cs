using PHS.Networking.Server.Models;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public interface IUserConnectionsTcp<T> : IUserConnections<T, IConnectionTcpServer>
    {
        IConnectionTcpServer GetConnection(TcpClient client);
    }
}