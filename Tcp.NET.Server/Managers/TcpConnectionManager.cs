using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManager<T> where T : ConnectionTcpServer
    {
        protected ConcurrentDictionary<string, T> _connections =
            new ConcurrentDictionary<string, T>();

        public TcpConnectionManager()
        {
        }

        public TcpConnectionManager(IEnumerable<T> connections)
        {
            _connections = new ConcurrentDictionary<string, T>();
            foreach (var item in connections)
            {
                _connections.TryAdd(item.ConnectionId, item);
            }
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _connections.Values.ToArray();
        }
        public virtual T Get(string id)
        {
            return _connections.TryGetValue(id, out var connection) ? connection : null;
        }
        public bool Add(string id, T connection)
        {
            return _connections.TryAdd(id, connection);
        }
        public virtual bool Remove(string id)
        {
            return _connections.TryRemove(id, out var _);
        }
        public virtual int Count()
        {
            return _connections.Skip(0).Count();
        }
    }
}
