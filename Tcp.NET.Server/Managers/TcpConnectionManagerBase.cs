using PHS.Networking.Server.Managers;
using System.Collections.Concurrent;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManagerBase<T> : ConnectionManager<T>
        where T : ConnectionTcpServer
    {
        public TcpConnectionManagerBase() { }
        public TcpConnectionManagerBase(ConcurrentDictionary<string, T> connections) : base(connections) { }

    }
}
