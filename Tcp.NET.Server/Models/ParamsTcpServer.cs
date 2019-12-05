namespace Tcp.NET.Server.Models
{
    public struct ParamsTcpServer
    {
        public string Url { get; set; }
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public int PingIntervalSec { get; set; }
        public int IntervalReconnectSec { get; set; }
        public string ConnectionSuccessString { get; set; }
        public string UnauthorizedString { get; set; }
    }
}
