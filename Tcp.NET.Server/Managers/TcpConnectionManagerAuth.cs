using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManagerAuth<T> : TcpConnectionManager
    {
        protected ConcurrentDictionary<T, IIdentityTcp<T>> _identities =
            new ConcurrentDictionary<T, IIdentityTcp<T>>();

        public IIdentityTcp<T> GetIdentity(T userId)
        {
            return _identities.TryGetValue(userId, out var clientAuthorized) ? clientAuthorized : (default);
        }
        public IIdentityTcp<T> GetIdentity(IConnectionTcpServer connection)
        {
            return _identities.Any(p => p.Value.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()))
                ? _identities.Values.FirstOrDefault(s => s.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()))
                : (default);
        }
        public IIdentityTcp<T>[] GetAllIdentities()
        {
            return _identities.Values.Where(s => s != null).ToArray();
        }

        public IIdentityTcp<T> AddIdentity(T userId, IConnectionTcpServer connection)
        {
            if (!_identities.TryGetValue(userId, out var instance))
            {
                instance = new IdentityTcp<T>
                {
                    UserId = userId,
                    Connections = new List<IConnectionTcpServer>()
                };
                _identities.TryAdd(userId, instance);
            }

            if (!instance.Connections.Any(s => s != null && s.Client.GetHashCode() == instance.GetHashCode()))
            {
                instance.Connections.Add(connection);
                return instance;
            }

            return null;
        }
        public void RemoveIdentity(IConnectionTcpServer connection)
        {
            var identity = _identities.Values.FirstOrDefault(s => s.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()));

            if (identity != null)
            {
                var instance = identity.Connections.FirstOrDefault(s => s != null && s.Client.GetHashCode() == connection.Client.GetHashCode());

                if (instance != null)
                {
                    identity.Connections.Remove(instance);

                    if (!identity.Connections.Where(s => s != null).Any())
                    {
                        _identities.TryRemove(identity.UserId, out identity);
                    }
                }
            }
        }

        public bool IsConnectionAuthorized(IConnectionTcpServer connection)
        {
            return _identities.Values.Any(s => s.Connections.Any(t => t != null && t.Client.GetHashCode() == connection.Client.GetHashCode()));
        }
        public bool IsUserConnected(T userId)
        {
            return _identities.ContainsKey(userId);
        }
    }
}
