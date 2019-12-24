using Tcp.NET.Server.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;

namespace Tcp.NET.Server
{
    public class TcpSSLConnectionManager : ITcpSSLConnectionManager
    {
        protected ConcurrentDictionary<int, ConnectionTcpClientSSLDTO> _connections =
            new ConcurrentDictionary<int, ConnectionTcpClientSSLDTO>();

        public ConnectionTcpClientSSLDTO[] GetAllConnections()
        {
            return _connections.Values.ToArray();
        }
        public ConnectionTcpClientSSLDTO GetConnection(TcpClient client)
        {
            return _connections.TryGetValue(client.GetHashCode(), out var connection) ? connection : null;
        }
        public bool AddConnection(ConnectionTcpClientSSLDTO connection)
        {
            return !_connections.ContainsKey(connection.Client.GetHashCode()) ? _connections.TryAdd(connection.Client.GetHashCode(), connection) : false;
        }
        public void RemoveConnection(TcpClient client, bool isDisconnect)
        { 
            if (_connections.TryRemove(client.GetHashCode(), out var connection) &&
                isDisconnect)
            {
                connection.Client.Close();
                connection.Client.Dispose();
            }
        }
        public bool IsConnectionOpen(TcpClient client)
        {
            if (_connections.TryGetValue(client.GetHashCode(), out var connection))
            {
                return connection.Client.Connected;
            }

            return false;
        }
    }
}
