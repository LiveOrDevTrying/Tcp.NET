namespace Tcp.NET.Client.Models
{
    public struct ParamsTcpClient
    {
        public string Url { get; set; }
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public int IntervalReconnectSec { get; set; }
        public AuthConfig AuthConfig { get; set; }
    }
}
