using PHS.Core.Models;
using Tcp.NET.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Auth.Models
{
    public class UserConnectionTcpDTO : UserConnectionDTO, IUserConnectionTcpDTO
    {
        public ICollection<ConnectionSocketDTO> Connections { get; set; }

        public ConnectionSocketDTO GetConnection(Socket socket)
        {
            return Connections.FirstOrDefault(s => s.Socket.GetHashCode() == socket.GetHashCode());
        }
    }
}
