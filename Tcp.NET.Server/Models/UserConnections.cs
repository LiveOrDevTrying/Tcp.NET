using PHS.Networking.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Models
{
    public class UserConnections<T> : User<T>, IUserConnections<T>
    {
        public ICollection<IConnectionServer> Connections { get; set; }

        public IConnectionServer GetConnection(TcpClient client)
        {
            return Connections.FirstOrDefault(s => s.Client.GetHashCode() == client.GetHashCode());
        }
    }
}
