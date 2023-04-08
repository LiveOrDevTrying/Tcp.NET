using System.Collections.Concurrent;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManager : TcpConnectionManagerBase<ConnectionTcpServer>
    {
        public TcpConnectionManager() { }
        public TcpConnectionManager(ConcurrentDictionary<string, ConnectionTcpServer> connections) : base(connections) { }
    }
}
