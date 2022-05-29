using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public class TcpHandlerServer : 
        TcpHandlerServerBase<
            TcpConnectionServerEventArgs,
            TcpMessageServerEventArgs,
            TcpErrorServerEventArgs,
            ParamsTcpServer,
            ConnectionTcpServer>
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

        protected override TcpConnectionServerEventArgs CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<ConnectionTcpServer> args)
        {
            return new TcpConnectionServerEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            };
        }

        protected override TcpErrorServerEventArgs CreateErrorEventArgs(TcpErrorServerBaseEventArgs<ConnectionTcpServer> args)
        {
            return new TcpErrorServerEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        protected override TcpMessageServerEventArgs CreateMessageEventArgs(TcpMessageServerBaseEventArgs<ConnectionTcpServer> args)
        {
            return new TcpMessageServerEventArgs
            {
                Connection = args.Connection,
                Bytes = args.Bytes,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
    }
}
