using Tcp.NET.Core.Models;

namespace Tcp.NET.Server.Models
{
    public class ConnectionTcpServer : ConnectionTcp
    {
        public bool HasBeenPinged { get; set; }
        public bool Disposed { get; set; }

        public override void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;

                base.Dispose();
            }
        }
    }
}
