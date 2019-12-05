using PHS.Core.Events.Args.NetworkEventArgs;
using PHS.Core.Models;
using System.Net.Sockets;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpMessageEventArgs : MessageEventArgs
    {
        public Socket Socket { get; set; }
        public PacketDTO Packet { get; set; }
    }
}
