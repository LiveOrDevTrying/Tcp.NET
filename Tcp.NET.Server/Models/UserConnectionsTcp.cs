using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public class UserConnectionsTcp<T> : IUserConnectionsTcp<T>
    {
        public T UserId { get; set; }

        public ICollection<IConnectionTcpServer> Connections { get; set; }

        public IConnectionTcpServer GetConnection(TcpClient client)
        {
            return Connections.FirstOrDefault(s => s.Client.GetHashCode() == client.GetHashCode());
        }
    }
}
