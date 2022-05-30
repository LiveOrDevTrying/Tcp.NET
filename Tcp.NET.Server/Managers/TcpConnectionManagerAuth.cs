using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManagerAuth<T> : TcpConnectionManager<IdentityTcpServer<T>>
    {
        protected ConcurrentDictionary<T, TcpConnectionManager<IdentityTcpServer<T>>> _users =
            new ConcurrentDictionary<T, TcpConnectionManager<IdentityTcpServer<T>>>();

        public virtual bool Add(IdentityTcpServer<T> identity)
        {
            Add(identity.ConnectionId, identity);

            if (!_users.TryGetValue(identity.UserId, out var userOriginal))
            {
                userOriginal = new TcpConnectionManager<IdentityTcpServer<T>>();
                if (!_users.TryAdd(identity.UserId, userOriginal))
                {
                    return false;
                }
            }

            var userNew = new TcpConnectionManager<IdentityTcpServer<T>>(userOriginal.GetAllDictionary());
            userNew.Add(identity.ConnectionId, identity);
            return _users.TryUpdate(identity.UserId, userNew, userOriginal);
        }
        public override bool Remove(string id)
        {
            _connections.TryRemove(id, out var _);

            try
            {
                T userToRemove = default;
                bool removeUser = false;
                foreach (var user in _users)
                {
                    if (user.Value.Remove(id))
                    {
                        if (user.Value.Count() == 0)
                        {
                            userToRemove = user.Key;
                            removeUser = true;
                            break;
                        }

                        return true;
                    }
                }

                if (removeUser)
                {
                    _users.TryRemove(userToRemove, out var _);
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        public IEnumerable<IdentityTcpServer<T>> GetAll(T id)
        {
            if (_users.TryGetValue(id, out var user))
            {
                return user.GetAll();
            }

            return null;
        }
    }
}
