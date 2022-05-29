using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public class TcpHandlerServer : TcpHandlerServerBase<ConnectionTcpServer>
    {
        public TcpHandlerServer(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public TcpHandlerServer(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override ConnectionTcpServer CreateConnection(ConnectionTcpServer connection)
        {
            return connection;
        }
    }
}
