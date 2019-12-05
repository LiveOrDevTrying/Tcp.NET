using PHS.Core.Events.Args.NetworkEventArgs;
using System.Net.Sockets;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpErrorEventArgs : ErrorEventArgs
    {
        public Socket Socket { get; set; }
    }
}
