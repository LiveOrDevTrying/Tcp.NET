using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpConnectionManager
    {
        bool AddConnection(Socket socket);
        ConnectionSocketDTO[] GetAllConnections();
        ConnectionSocketDTO GetConnection(Socket socket);
        bool IsConnectionOpen(Socket socket);
        void RemoveConnection(Socket socket, bool isDisconnect);
    }
}