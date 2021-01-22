using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Models
{
    public interface IConnectionTcpServer : IConnectionTcp
    {
        bool HasBeenPinged { get; set; }
        string ConnectionId { get; set; }
    }
}