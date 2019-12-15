using PHS.Core.Events.Args.NetworkEventArgs;
using PHS.Core.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpMessageEventArgs : MessageEventArgs
    {
        public ConnectionSocketDTO Connection { get; set; }
        public PacketDTO Packet { get; set; }
    }
}
