using PHS.Core.Models;
using System.Collections.Generic;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Auth.Interfaces
{
    public interface IUserConnectionTcpDTO : IUserConnectionDTO
    {
        ICollection<ConnectionSocketDTO> Connections { get; set; }

        ConnectionSocketDTO GetConnection(Socket socket);
    }
}