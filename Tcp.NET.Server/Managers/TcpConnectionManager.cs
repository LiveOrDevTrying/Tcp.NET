﻿using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    internal class TcpConnectionManager
    {
        protected ConcurrentDictionary<int, IConnectionServer> _connections =
            new ConcurrentDictionary<int, IConnectionServer>();

        public virtual IConnectionServer[] GetAllConnections()
        {
            return _connections.Values.ToArray();
        }
        public virtual IConnectionServer GetConnection(TcpClient client)
        {
            return _connections.TryGetValue(client.GetHashCode(), out var connection) ? connection : null;
        }
        public virtual bool AddConnection(IConnectionServer connection)
        {
            return !_connections.ContainsKey(connection.Client.GetHashCode()) ? _connections.TryAdd(connection.Client.GetHashCode(), connection) : false;
        }
        public virtual void RemoveConnection(IConnectionServer connection)
        {
            _connections.TryRemove(connection.Client.GetHashCode(), out var instance);
        }
        public virtual bool IsConnectionOpen(IConnectionServer connection)
        {
            return _connections.TryGetValue(connection.Client.GetHashCode(), out var instance) ? instance.Client.Connected : false;
        }
    }
}
