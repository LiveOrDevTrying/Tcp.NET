using Tcp.NET.Server.Models;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using PHS.Networking.Enums;
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
            TcpConnectionManager<ConnectionTcpServer>,
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

        protected override TcpConnectionManager<ConnectionTcpServer> CreateTcpConnectionManager()
        {
            return new TcpConnectionManager<ConnectionTcpServer>();
        }
        protected override TcpHandlerServer CreateTcpHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate != null
                ? new TcpHandlerServer(_parameters, certificate, certificatePassword)
                : new TcpHandlerServer(_parameters);
        }

        protected override void OnConnectionEvent(object sender, TcpConnectionServerBaseEventArgs<ConnectionTcpServer> args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionManager.Add(args.Connection.ConnectionId, args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.Remove(args.Connection.ConnectionId);
                    break;
                default:
                    break;
            }

            FireEvent(this, new TcpConnectionServerEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            });
        }
        protected override void OnErrorEvent(object sender, TcpErrorServerBaseEventArgs<ConnectionTcpServer> args)
        {
            FireEvent(this, new TcpErrorServerEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            });
        }
        protected override void OnMessageEvent(object sender, TcpMessageServerBaseEventArgs<ConnectionTcpServer> args)
        {
            FireEvent(this, new TcpMessageServerEventArgs
            {
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType,
                Bytes = args.Bytes
            });
        }
    }
}
