using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public class TcpHandlerAuth<T> : TcpHandlerBase<IdentityTcpServer<T>>
    {
        public TcpHandlerAuth(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public TcpHandlerAuth(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override IdentityTcpServer<T> CreateConnection(ConnectionTcpServer connection)
        {
            return new IdentityTcpServer<T>
            {
                Client = connection.Client,
                ConnectionId = connection.ConnectionId,
                Reader = connection.Reader,
                Writer = connection.Writer,
            };
        }
    }
}
