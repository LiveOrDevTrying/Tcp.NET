using Tcp.NET.Server.Models;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Events.Args;

namespace Tcp.NET.Server
{
    public class TcpNETServer :
        TcpNETServerBase<
            TcpConnectionServerEventArgs,
            TcpMessageServerEventArgs,
            TcpErrorServerEventArgs,
            ParamsTcpServer,
            TcpHandlerServer,
            TcpConnectionManager,
            ConnectionTcpServer>,
        ITcpNETServer
    {
        public TcpNETServer(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public TcpNETServer(ParamsTcpServer parameters, 
            byte[] certificate, 
            string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override TcpConnectionManager CreateConnectionManager()
        {
            return new TcpConnectionManager();
        }

        protected override TcpHandlerServer CreateHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate != null
                ? new TcpHandlerServer(_parameters, certificate, certificatePassword)
                : new TcpHandlerServer(_parameters);
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

        protected override TcpConnectionServerEventArgs CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<ConnectionTcpServer> args)
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
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
     }
}
