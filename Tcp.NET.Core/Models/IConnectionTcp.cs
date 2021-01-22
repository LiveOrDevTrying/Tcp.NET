using PHS.Networking.Models;
using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public interface IConnectionTcp : IConnection
    {
        TcpClient Client { get; set; }
        StreamReader Reader { get; set; }
        StreamWriter Writer { get; set; }
    }
}