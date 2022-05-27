using Tcp.NET.Client.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client.Handlers
{
    public class TcpClientHandler : TcpClientHandlerBase<ConnectionTcp>
    {
        public TcpClientHandler(ParamsTcpClient parameters, string token = "") : base(parameters, token)
        {
        }

        protected override ConnectionTcp CreateConnection(ConnectionTcp connection)
        {
            return connection;
        }
    }
}
