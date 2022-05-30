using PHS.Networking.Server.Managers;
using System.Collections.Concurrent;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManager<T> : ConnectionManager<T>
        where T : ConnectionTcpServer
    {
        public TcpConnectionManager() { }
        public TcpConnectionManager(ConcurrentDictionary<string, T> connections) : base(connections) { }

    }
}
