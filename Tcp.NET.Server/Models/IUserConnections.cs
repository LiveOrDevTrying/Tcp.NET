using System.Collections.Generic;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public interface IUserConnections<T> 
    {
        T UserId { get; set; }

        ICollection<IConnectionServer> Connections { get; set; }

        IConnectionServer GetConnection(TcpClient client);
    }
}