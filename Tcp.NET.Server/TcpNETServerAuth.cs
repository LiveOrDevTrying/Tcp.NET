using PHS.Networking.Enums;
using PHS.Networking.Server.Services;
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
        }
        public TcpNETServerAuth(ParamsTcpServerAuth parameters,
            IUserService<T> userService,
            byte[] certificate,
            string certificatePassword) : base(parameters, userService, certificate, certificatePassword)
        {
        }

        protected override TcpConnectionManagerAuth<T> CreateConnectionManager()
        {
            return new TcpConnectionManagerAuth<T>();
        }
        protected override TcpHandlerServerAuth<T> CreateHandler()
        {
            return _certificate == null
                ? new TcpHandlerServerAuth<T>(_parameters)
                : new TcpHandlerServerAuth<T>(_parameters, _certificate, _certificatePassword);
        }

        protected override void OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<T> args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionManager.AddUser(args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.RemoveConnection(args.Connection.ConnectionId);
                    break;
                default:
                    break;
            }

            FireEvent(this, args);
        }

        protected override void OnErrorEvent(object sender, TcpErrorServerAuthEventArgs<T> args)
        {
            FireEvent(this, new TcpErrorServerAuthEventArgs<T>
            {
                Exception = args.Exception,
                Message = args.Message,
                Connection = args.Connection,
                CancellationToken = args.CancellationToken
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
                MessageEventType = args.MessageEventType,
                CancellationToken = args.CancellationToken
            };
        }
        protected override TcpErrorServerAuthEventArgs<T> CreateErrorEventArgs(TcpErrorServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            return new TcpErrorServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message,
                CancellationToken = args.CancellationToken
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
