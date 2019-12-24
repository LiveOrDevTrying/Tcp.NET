using PHS.Core.Events.Args.NetworkEventArgs;
using System.IO;
using System.Net.Sockets;
using Tcp.NET.Core.SSL.Enums;

namespace Tcp.NET.Core.SSL.Events.Args
{
    public class TcpSSLConnectionEventArgs : ConnectionEventArgs
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
        public TcpSSLConnectionType ConnectionType { get; set; }
    }
}
