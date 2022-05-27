namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServer
    {
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public string ConnectionSuccessString { get; set; }
        public int PingIntervalSec { get; set; } = 10;
    }
}
