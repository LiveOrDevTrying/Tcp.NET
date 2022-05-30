using PHS.Networking.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpConnectionEventArgs<T> : ConnectionEventArgs<T> where T : ConnectionTcp
    {
    }
}
