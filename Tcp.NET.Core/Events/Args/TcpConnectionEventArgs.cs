using PHS.Core.Events.Args.NetworkEventArgs;
using Tcp.NET.Core.Enums;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpConnectionEventArgs : ConnectionEventArgs
    {
        public ConnectionSocketDTO Connection { get; set; }
        public TcpConnectionType ConnectionType { get; set; }
    }
}
