using PHS.Core.Events.Args.NetworkEventArgs;
using System.Net.Sockets;

namespace Tcp.NET.Core.SSL.Events.Args
{
    public class TcpSSLErrorEventArgs : ErrorEventArgs
    {
        public TcpClient Client { get; set; }
    }
}
