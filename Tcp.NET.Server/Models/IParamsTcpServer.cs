using PHS.Networking.Models;

namespace Tcp.NET.Server.Models
{
    public interface IParamsTcpServer : IParams
    {
        int Port { get; }
        string ConnectionSuccessString { get; }
        byte[] DisconnectBytes { get; }
        byte[] EndOfLineBytes { get; }
        bool OnlyEmitBytes { get; }
        byte[] PingBytes { get; }
        int PingIntervalSec { get; }
        byte[] PongBytes { get; }
        bool UseDisconnectBytes { get; }
    }
}