using PHS.Networking.Server.Managers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Managers
{
    public class TcpConnectionManagerAuth<T> : ConnectionManagerAuth<IdentityTcpServer<T>, T>
    {
    }
}
