using Tcp.NET.Server.SSL.Auth.Interfaces;

namespace Tcp.NET.Server.SSL.Auth.Models
{
    public struct ParamsTcpSSLAuthServer : IParamsTcpSSLAuthServer
    {
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public int PingIntervalSec { get; set; }
        public int IntervalReconnectSec { get; set; }
        public string ConnectionSuccessString { get; set; }
        public string UnauthorizedString { get; set; }
    }
}
