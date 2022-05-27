using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public interface ITcpNETServer : 
        ITcpNETServerBase<
            TcpConnectionServerEventArgs, 
            TcpMessageServerEventArgs, 
            TcpErrorServerEventArgs, 
            ConnectionTcpServer>
    {
    }
}