using Tcp.NET.Core.Models;
using Tcp.NET.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tcp.NET.Server
{
    public class TcpConnectionManager : ITcpConnectionManager
    {
        protected ConcurrentDictionary<int, ConnectionSocketDTO> _connections =
            new ConcurrentDictionary<int, ConnectionSocketDTO>();

        public ICollection<ConnectionSocketDTO> GetAllConnections()
        {
            return _connections.Values.ToList();
        }

        public bool AddConnection(ConnectionSocketDTO connection)
        {
            return !_connections.ContainsKey(connection.Socket.GetHashCode()) ? _connections.TryAdd(connection.Socket.GetHashCode(), connection) : false;
        }
        public void RemoveConnection(ConnectionSocketDTO connection, bool isDisconnect)
        {
            if (_connections.TryGetValue(connection.Socket.GetHashCode(), out _))
            {
                _connections.TryRemove(connection.Socket.GetHashCode(), out _);
            }

            if (isDisconnect)
            {
                connection.Socket.Close();
                connection.Socket.Dispose();
            }
        }
        public ConnectionSocketDTO GetConnection(Socket socket)
        {
            return _connections.TryGetValue(socket.GetHashCode(), out var connection) ? connection : null;
        }

        public bool IsConnectionOpen(ConnectionSocketDTO connection)
        {
            return _connections.ContainsKey(connection.Socket.GetHashCode());
        }
    }
}
