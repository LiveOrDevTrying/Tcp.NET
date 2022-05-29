using PHS.Networking.Enums;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public delegate void AuthorizeEvent<T, U>(object sender, TcpAuthorizeEventArgs<T, U> args) where T : IdentityTcpServer<U>;

    public class TcpHandlerServerAuth<T> : 
        TcpHandlerServerBase<
            TcpConnectionServerAuthEventArgs<T>,
            TcpMessageServerAuthEventArgs<T>,
            TcpErrorServerAuthEventArgs<T>,
            ParamsTcpServerAuth,
            IdentityTcpServer<T>>
    {
        protected event AuthorizeEvent<IdentityTcpServer<T>, T> _authorizeEvent;

        public TcpHandlerServerAuth(ParamsTcpServerAuth parameters) : base(parameters)
        {
        }

        public TcpHandlerServerAuth(ParamsTcpServerAuth parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        public virtual Task AuthorizeCallbackAsync(TcpAuthorizeEventArgs<IdentityTcpServer<T>, T> args, CancellationToken cancellationToken)
        {
            FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
            {
                ConnectionEventType = ConnectionEventType.Connected,
                Connection = args.Connection
            });

            return Task.CompletedTask;
        }

        protected override IdentityTcpServer<T> CreateConnection(ConnectionTcpServer connection)
        {
            return new IdentityTcpServer<T>
            {
                TcpClient = connection.TcpClient,
                ConnectionId = connection.ConnectionId
            };
        }

        protected override TcpConnectionServerAuthEventArgs<T> CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            return new TcpConnectionServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
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

        protected override void FireEvent(object sender, TcpMessageServerAuthEventArgs<T> args)
        {
            if (!args.Connection.IsAuthorized)
            {
                FireEvent(this, new TcpAuthorizeEventArgs<IdentityTcpServer<T>, T>
                {
                    Connection = args.Connection,
                    Token = args.Message,
                });
            }
            else
            {
                base.FireEvent(sender, args);
            }
        }
        protected override void FireEvent(object sender, TcpConnectionServerAuthEventArgs<T> args)
        {
            if (args.Connection.IsAuthorized)
            {
                base.FireEvent(sender, args);
            }
        }
        protected virtual void FireEvent(object sender, TcpAuthorizeEventArgs<IdentityTcpServer<T>, T> args)
        {
            _authorizeEvent?.Invoke(sender, args);
        }

        public event AuthorizeEvent<IdentityTcpServer<T>, T> AuthorizeEvent
        {
            add
            {
                _authorizeEvent += value;
            }
            remove
            {
                _authorizeEvent -= value;
            }
        }
    }
}
