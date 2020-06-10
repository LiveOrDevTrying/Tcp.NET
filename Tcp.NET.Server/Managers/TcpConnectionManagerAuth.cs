using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    internal class TcpConnectionManagerAuth<T> : TcpConnectionManager
    {
        protected ConcurrentDictionary<T, IUserConnections<T>> _userConnections =
            new ConcurrentDictionary<T, IUserConnections<T>>();

        public virtual IUserConnections<T> GetIdentity(T userId)
        {
            return _userConnections.TryGetValue(userId, out var clientAuthorized) ? clientAuthorized : (default);
        }
        public virtual IUserConnections<T> GetIdentity(IConnectionServer connection)
        {
            return _userConnections.Any(p => p.Value.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()))
                ? _userConnections.Values.FirstOrDefault(s => s.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()))
                : (default);
        }
        public virtual IUserConnections<T>[] GetAllIdentities()
        {
            return _userConnections.Values.ToArray();
        }

        public virtual IUserConnections<T> AddUserConnection(T userId, IConnectionServer connection)
        {
            if (!_userConnections.TryGetValue(userId, out var instance))
            {
                instance = new UserConnections<T>
                {
                    Id = userId,
                    Connections = new List<IConnectionServer>()
                };
                _userConnections.TryAdd(userId, instance);
            }

            if (!instance.Connections.Any(s => s != null && s.Client.GetHashCode() == instance.GetHashCode()))
            {
                instance.Connections.Add(connection);
                return instance;
            }

            return null;
        }
        public virtual void RemoveUserConnection(IConnectionServer connection)
        {
            var userConnection = _userConnections.Values.FirstOrDefault(s => s.Connections.Any(t => t.Client.GetHashCode() == connection.Client.GetHashCode()));

            if (userConnection != null)
            {
                var instance = userConnection.Connections.FirstOrDefault(s => s.Client.GetHashCode() == connection.Client.GetHashCode());

                if (instance != null)
                {
                    userConnection.Connections.Remove(instance);

                    if (!userConnection.Connections.Any())
                    {
                        _userConnections.TryRemove(userConnection.Id, out userConnection);
                    }
                }
            }
        }

        public virtual bool IsConnectionAuthorized(IConnectionServer connection)
        {
            return _userConnections.Values.Any(s => s.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()));
        }
        public virtual bool IsUserConnected(T userId)
        {
            return _userConnections.ContainsKey(userId);
        }
    }
}
