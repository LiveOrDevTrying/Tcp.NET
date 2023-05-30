using PHS.Networking.Models;
using System;
using System.IO;
using System.Net.Sockets;

namespace Tcp.NET.Core.Models
{
    public class ConnectionTcp : IConnection
    {
        public string ConnectionId { get; set; }
        public TcpClient TcpClient { get; set; }
        public MemoryStream MemoryStream { get; set; }
        public bool Disposed { get; set; }
        public bool EndOfLine { get; set; }

        public ConnectionTcp()
        {
            MemoryStream = new MemoryStream();
        }

        public virtual void Dispose()
        {
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

            try
            {
                MemoryStream.Close();
                MemoryStream.Dispose();
            }
            catch { }
        }
    }
}
