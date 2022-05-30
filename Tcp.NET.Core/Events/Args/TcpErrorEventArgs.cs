using PHS.Networking.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpErrorEventArgs<T> : ErrorEventArgs<T> where T : ConnectionTcp
    {
    }
}
