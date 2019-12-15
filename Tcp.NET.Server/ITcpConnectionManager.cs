using System.Collections.Generic;
using System.Net.Sockets;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Server
{
    public interface ITcpConnectionManager
    {
        bool AddConnection(ConnectionSocketDTO connection);
        ConnectionSocketDTO GetConnection(Socket socket);
        ICollection<ConnectionSocketDTO> GetAllConnections();
        bool IsConnectionOpen(ConnectionSocketDTO socket);
        void RemoveConnection(ConnectionSocketDTO connection, bool isDisconnect);
    }
}