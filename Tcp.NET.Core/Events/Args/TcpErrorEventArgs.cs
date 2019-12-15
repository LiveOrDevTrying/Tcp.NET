using PHS.Core.Events.Args.NetworkEventArgs;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpErrorEventArgs : ErrorEventArgs
    {
        public ConnectionSocketDTO Connection { get; set; }
    }
}
