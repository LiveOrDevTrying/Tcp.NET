namespace Tcp.NET.Server.SSL.Models
{
    public interface IParamsTcpServerSSL
    {
        string EndOfLineCharacters { get; set; }
        int IntervalReconnectSec { get; set; }
        int PingIntervalSec { get; set; }
        int Port { get; set; }
    }
}