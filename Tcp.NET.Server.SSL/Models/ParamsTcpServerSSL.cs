namespace Tcp.NET.Server.Models
{
    public struct ParamsTcpServerSSL : IParamsTcpServerSSL
    {
        public string Url { get; set; }
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public int PingIntervalSec { get; set; }
        public int IntervalReconnectSec { get; set; }
    }
}
