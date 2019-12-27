namespace Tcp.NET.Server.SSL.Models
{
    public struct ParamsTcpServerSSL : IParamsTcpServerSSL
    {
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public int PingIntervalSec { get; set; }
        public int IntervalReconnectSec { get; set; }
    }
}
