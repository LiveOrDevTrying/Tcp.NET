namespace Tcp.NET.Server.Models
{
    public interface IParamsTcpServer
    {
        string EndOfLineCharacters { get; set; }
        int IntervalReconnectSec { get; set; }
        int PingIntervalSec { get; set; }
        int Port { get; set; }
    }
}