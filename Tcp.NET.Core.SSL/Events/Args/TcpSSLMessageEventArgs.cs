using PHS.Core.Events.Args.NetworkEventArgs;
using PHS.Core.Models;
using System.Net.Sockets;

namespace Tcp.NET.Core.SSL.Events.Args
{
    public class TcpSSLMessageEventArgs : MessageEventArgs
    {
        public TcpClient Client { get; set; }
        public PacketDTO Packet { get; set; }
    }
}
