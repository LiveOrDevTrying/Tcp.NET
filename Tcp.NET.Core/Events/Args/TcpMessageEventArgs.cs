using PHS.Networking.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Core.Events.Args
{
    public class TcpMessageEventArgs<T> : MessageEventArgs where T : ConnectionTcp
    {
        public T Connection { get; set; }
        public string Message { get; set; }
    }
}
