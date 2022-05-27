using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public class TcpHandler : TcpHandlerBase<ConnectionTcpServer>
    {
        public TcpHandler(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public TcpHandler(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override ConnectionTcpServer CreateConnection(ConnectionTcpServer connection)
        {
            return connection;
        }
    }
}
