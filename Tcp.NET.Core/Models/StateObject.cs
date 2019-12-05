using System.Net.Sockets;
using System.Text;

namespace Tcp.NET.Core.Models
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket WorkSocket { get; set; } = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] Buffer { get; set; } = new byte[BufferSize];
        // Received data string.  
        public StringBuilder Sb { get; set; } = new StringBuilder();
    }
}
