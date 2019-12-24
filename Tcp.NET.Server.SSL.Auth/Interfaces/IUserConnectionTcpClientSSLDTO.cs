using PHS.Core.Models;
using System.Collections.Generic;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.SSL.Auth.Interfaces
{
    public interface IUserConnectionTcpClientSSLDTO : IUserConnectionDTO
    {
        ICollection<ConnectionTcpClientSSLDTO> Connections { get; set; }

        ConnectionTcpClientSSLDTO GetConnection(TcpClient client);
    }
}