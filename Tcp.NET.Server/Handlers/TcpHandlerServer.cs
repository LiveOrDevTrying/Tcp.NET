using PHS.Networking.Events.Args;
using PHS.Networking.Models;
using System;
using Tcp.NET.Core.Models;
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

        protected override ConnectionTcpServer CreateConnection(ConnectionTcp connection)
        {
            return new ConnectionTcpServer
            {
                TcpClient = connection.TcpClient,
                ConnectionId = Guid.NewGuid().ToString()
            };
        }

        protected override TcpConnectionServerEventArgs CreateConnectionEventArgs(ConnectionEventArgs<ConnectionTcpServer> args)
        {
            return new TcpConnectionServerEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
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
        protected override TcpErrorServerEventArgs CreateErrorEventArgs(ErrorEventArgs<ConnectionTcpServer> args)
        {
            return new TcpErrorServerEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }
    }
}
