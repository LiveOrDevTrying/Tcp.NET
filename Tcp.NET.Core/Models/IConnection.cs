using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public interface IConnection
    {
        TcpClient Client { get; set; }
        StreamReader Reader { get; set; }
        StreamWriter Writer { get; set; }
    }
}