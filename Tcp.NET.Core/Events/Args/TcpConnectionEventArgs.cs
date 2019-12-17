using PHS.Core.Events.Args.NetworkEventArgs;
using System.Net.Sockets;
using Tcp.NET.Core.Enums;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpConnectionEventArgs : ConnectionEventArgs
    {
        public Socket Socket { get; set; }
        public TcpConnectionType ConnectionType { get; set; }
    }
}
