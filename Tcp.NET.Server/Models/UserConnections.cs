using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public class UserConnections<T> : IUserConnections<T>
    {
        public T UserId { get; set; }

        public ICollection<IConnectionServer> Connections { get; set; }

        public IConnectionServer GetConnection(TcpClient client)
        {
            return Connections.FirstOrDefault(s => s.Client.GetHashCode() == client.GetHashCode());
        }
    }
}
