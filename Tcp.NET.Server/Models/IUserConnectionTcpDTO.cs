using PHS.Core.Models;
using Tcp.NET.Core.Models;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Tcp.NET.Server.Models
{
    public interface IUserConnectionTcpDTO : IUserConnectionDTO
    {
        ICollection<ConnectionSocketDTO> Connections { get; set; }

        ConnectionSocketDTO GetConnection(Socket socket);
    }
}