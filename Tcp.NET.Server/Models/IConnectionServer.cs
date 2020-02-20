using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Models
{
    public interface IConnectionServer : IConnection
    {
        bool HasBeenPinged { get; set; }
        string ConnectionId { get; set; }
    }
}