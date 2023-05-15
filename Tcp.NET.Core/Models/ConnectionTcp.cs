using PHS.Networking.Models;
using System;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public class ConnectionTcp : IConnection
    {
        public string ConnectionId { get; set; }
        public TcpClient TcpClient { get; set; }
        public DateTime NextPing { get; set; }
        public bool Disposed { get; set; }

        public virtual void Dispose()
        {
            Disposed = true;

            try
            {
                TcpClient?.GetStream().Close();
            }
            catch { }

            try
            {
                TcpClient?.Dispose();
            }
            catch { }
        }
    }
}
