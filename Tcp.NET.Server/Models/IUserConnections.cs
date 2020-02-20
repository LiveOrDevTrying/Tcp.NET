using PHS.Networking.Server.Models;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public interface IUserConnections<T> : IUser<T>
    {
        ICollection<IConnectionServer> Connections { get; set; }

        IConnectionServer GetConnection(TcpClient client);
    }
}