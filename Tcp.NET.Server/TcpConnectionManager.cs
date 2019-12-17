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

        public ConnectionSocketDTO[] GetAllConnections()
        {
            return _connections.Values.ToArray();
        }
        public ConnectionSocketDTO GetConnection(Socket socket)
        {
            return _connections.TryGetValue(socket.GetHashCode(), out var connection) ? connection : null;
        }
        public bool AddConnection(Socket socket)
        {
            return !_connections.ContainsKey(socket.GetHashCode()) ? _connections.TryAdd(socket.GetHashCode(), new ConnectionSocketDTO
            {
                Socket = socket
            }) : false;
        }
        public void RemoveConnection(Socket socket, bool isDisconnect)
        { 
            if (_connections.TryRemove(socket.GetHashCode(), out var connection) &&
                isDisconnect)
            {
                connection.Socket.Close();
                connection.Socket.Dispose();
            }
        }
        public bool IsConnectionOpen(Socket socket)
        {
            if (_connections.TryGetValue(socket.GetHashCode(), out var connection))
            {
                return connection.Socket.Connected;
            }

            return false;
        }
    }
}
