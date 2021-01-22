using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManager
    {
        protected ConcurrentDictionary<int, IConnectionTcpServer> _connections =
            new ConcurrentDictionary<int, IConnectionTcpServer>();

        public IConnectionTcpServer[] GetAllConnections()
        {
            return _connections.Values.ToArray();
        }
        public IConnectionTcpServer GetConnection(TcpClient client)
        {
            return _connections.TryGetValue(client.GetHashCode(), out var connection) ? connection : null;
        }
        public bool AddConnection(IConnectionTcpServer connection)
        {
            return !_connections.ContainsKey(connection.Client.GetHashCode()) ? _connections.TryAdd(connection.Client.GetHashCode(), connection) : false;
        }
        public void RemoveConnection(IConnectionTcpServer connection)
        {
            _connections.TryRemove(connection.Client.GetHashCode(), out var instance);
        }
        public bool IsConnectionOpen(IConnectionTcpServer connection)
        {
            return _connections.TryGetValue(connection.Client.GetHashCode(), out var instance) ? instance.Client.Connected : false;
        }
    }
}
