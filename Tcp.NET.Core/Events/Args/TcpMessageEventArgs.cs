using PHS.Networking.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpMessageEventArgs<T> : MessageEventArgs<T> where T : ConnectionTcp
    {
        public string Message { get; set; }
    }
}
