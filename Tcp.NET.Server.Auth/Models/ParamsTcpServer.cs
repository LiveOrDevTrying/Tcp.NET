using Tcp.NET.Server.Auth.Interfaces;

namespace Tcp.NET.Server.Auth.Models
{
    public struct ParamsTcpAuthServer : IParamsTcpAuthServer
    {
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public int PingIntervalSec { get; set; }
        public int IntervalReconnectSec { get; set; }
        public string ConnectionSuccessString { get; set; }
        public string UnauthorizedString { get; set; }
    }
}
