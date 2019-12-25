using PHS.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.SSL.Auth.Interfaces;
using Tcp.NET.Server.SSL.Models;

namespace Tcp.NET.Server.SSL.Auth.Models
{
    public class UserConnectionTcpClientSSLDTO : UserConnectionDTO, IUserConnectionTcpClientSSLDTO
    {
        public ICollection<ConnectionTcpClientSSLDTO> Connections { get; set; }

        public ConnectionTcpClientSSLDTO GetConnection(TcpClient client)
        {
            return Connections.FirstOrDefault(s => s.Client.GetHashCode() == client.GetHashCode());
        }
    }
}
