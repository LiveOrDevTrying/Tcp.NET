using PHS.Networking.Events.Args;
using PHS.Networking.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpErrorEventArgs<T> : ErrorEventArgs<T> where T : ConnectionTcp
    {
    }
}
