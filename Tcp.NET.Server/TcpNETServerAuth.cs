using PHS.Networking.Enums;
using PHS.Networking.Server.Services;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public class TcpNETServerAuth<T> :
        TcpNETServerAuthBase<
            TcpConnectionServerAuthEventArgs<T>, 
            TcpMessageServerAuthEventArgs<T>, 
            TcpErrorServerAuthEventArgs<T>,
            ParamsTcpServerAuth,
            TcpHandlerServerAuth<T>,
            TcpConnectionManagerAuth<T>,
            IdentityTcpServer<T>,
            T,
            TcpAuthorizeEventArgs<T>>,
        ITcpNETServerAuth<T>
    {
        public TcpNETServerAuth(ParamsTcpServerAuth parameters,
            IUserService<T> userService) : base(parameters, userService)
        { 
            _handler.AuthorizeEvent += OnAuthorizeEvent;
        }
        public TcpNETServerAuth(ParamsTcpServerAuth parameters,
            IUserService<T> userService,
            byte[] certificate,
            string certificatePassword) : base(parameters, userService, certificate, certificatePassword)
        {
            _handler.AuthorizeEvent += OnAuthorizeEvent;
        }

        protected override TcpConnectionManagerAuth<T> CreateConnectionManager()
        {
            return new TcpConnectionManagerAuth<T>();
        }
        protected override TcpHandlerServerAuth<T> CreateHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate == null
                ? new TcpHandlerServerAuth<T>(_parameters)
                : new TcpHandlerServerAuth<T>(_parameters, certificate, certificatePassword);
        }

        protected override void OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<T> args)
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
        protected override void OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<T> args)
        {
            FireEvent(this, args);
        }
        protected override void OnErrorEvent(object sender, TcpErrorServerAuthEventArgs<T> args)
        {
            FireEvent(this, new TcpErrorServerAuthEventArgs<T>
            {
                Exception = args.Exception,
                Message = args.Message,
                Connection = args.Connection
            });
        }
        protected override TcpConnectionServerAuthEventArgs<T> CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            return new TcpConnectionServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            };
        }
        protected override TcpMessageServerAuthEventArgs<T> CreateMessageEventArgs(TcpMessageServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            return new TcpMessageServerAuthEventArgs<T>
            {
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
        protected override TcpErrorServerAuthEventArgs<T> CreateErrorEventArgs(TcpErrorServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            return new TcpErrorServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        public override void Dispose()
        {
            if (_handler != null)
            {
                _handler.AuthorizeEvent -= OnAuthorizeEvent;
            }

            base.Dispose();
        }
    }
}
