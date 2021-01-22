using PHS.Networking.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpErrorEventArgs<T> : ErrorEventArgs where T : IConnectionTcp
    {
        public T Connection { get; set; }
    }
}
