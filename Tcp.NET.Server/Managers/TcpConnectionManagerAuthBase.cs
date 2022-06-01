using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public abstract class TcpConnectionManagerAuthBase<U, T> : TcpConnectionManagerBase<U> where U : IdentityTcpServer<T>
    {
        protected ConcurrentDictionary<T, TcpConnectionManagerBase<U>> _users =
            new ConcurrentDictionary<T, TcpConnectionManagerBase<U>>();

        public virtual bool Add(U identity)
        {
            Add(identity.ConnectionId, identity);

            if (!_users.TryGetValue(identity.UserId, out var userOriginal))
            {
                userOriginal = new TcpConnectionManagerBase<U>();
                if (!_users.TryAdd(identity.UserId, userOriginal))
                {
                    return false;
                }
            }

            var userNew = new TcpConnectionManagerBase<U>(userOriginal.GetAllDictionary());
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

        public IEnumerable<U> GetAll(T id)
        {
            if (_users.TryGetValue(id, out var user))
            {
                return user.GetAll();
            }

            return null;
        }
    }
}
