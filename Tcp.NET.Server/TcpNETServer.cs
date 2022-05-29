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

        protected override void OnConnectionEvent(object sender, TcpConnectionServerEventArgs args)
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

            FireEvent(this, args);
        }
        protected override void OnErrorEvent(object sender, TcpErrorServerEventArgs args)
        {
            FireEvent(this, args);
        }
        protected override void OnMessageEvent(object sender, TcpMessageServerEventArgs args)
        {
            FireEvent(this, args);
        }
    }
}
