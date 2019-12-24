using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpSSLConnectionManager
    {
        bool AddConnection(ConnectionTcpClientSSLDTO connection);
        ConnectionTcpClientSSLDTO[] GetAllConnections();
        ConnectionTcpClientSSLDTO GetConnection(TcpClient client);
        bool IsConnectionOpen(TcpClient client);
        void RemoveConnection(TcpClient client, bool isDisconnect);
    }
}