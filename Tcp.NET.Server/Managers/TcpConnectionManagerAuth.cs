using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManagerAuth<T> : TcpConnectionManagerAuthBase<IdentityTcpServer<T>, T>
    {
    }
}
