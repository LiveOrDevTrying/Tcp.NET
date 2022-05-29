using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public class TcpHandlerServerAuth<T> : TcpHandlerServerBase<IdentityTcpServer<T>>
    {
        public TcpHandlerServerAuth(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public TcpHandlerServerAuth(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override IdentityTcpServer<T> CreateConnection(ConnectionTcpServer connection)
        {
            return new IdentityTcpServer<T>
            {
                TcpClient = connection.TcpClient,
                ConnectionId = connection.ConnectionId
            };
        }
    }
}
