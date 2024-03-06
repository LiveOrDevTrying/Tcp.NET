using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public delegate void AuthorizeEvent<Z, X, A>(object sender, X args) 
        where X : TcpAuthorizeBaseEventArgs<Z, A> 
        where Z : IdentityTcpServer<A>;

    public abstract class TcpHandlerServerAuthBase<T, U, V, W, X, Z, A> : 
        TcpHandlerServerBase<T, U, V, W, Z>
        where T : TcpConnectionServerAuthBaseEventArgs<Z, A>
        where U : TcpMessageServerAuthBaseEventArgs<Z, A>
        where V : TcpErrorServerAuthBaseEventArgs<Z, A>
        where W : IParamsTcpServerAuth
        where X : TcpAuthorizeBaseEventArgs<Z, A>
        where Z : IdentityTcpServer<A>
    {
        protected event AuthorizeEvent<Z, X, A> _authorizeEvent;

        public TcpHandlerServerAuthBase(W parameters) : base(parameters)
        {
        }

        public TcpHandlerServerAuthBase(W parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        public abstract Task AuthorizeCallbackAsync(TcpAuthorizeBaseEventArgs<Z, A> args, CancellationToken cancellationToken);

        protected override void FireEvent(object sender, U args)
        {
            switch (args.MessageEventType)
            {
                case PHS.Networking.Enums.MessageEventType.Sent:
                    break;
                case PHS.Networking.Enums.MessageEventType.Receive:
                    if (!args.Connection.IsAuthorized)
                    {
                        FireEvent(this, CreateAuthorizeEventArgs(new TcpAuthorizeBaseEventArgs<Z, A>
                        {
                            Connection = args.Connection,
                            Token = args.Bytes,
                            CancellationToken = args.CancellationToken
                        }));

                        return;
                    }
                    break;
                default:
                    break;
            }

            base.FireEvent(sender, args);
        }
        protected override void FireEvent(object sender, T args)
        {
            if (args.Connection.IsAuthorized)
            {
                base.FireEvent(sender, args);
            }
        }
        protected virtual void FireEvent(object sender, X args)
        {
            _authorizeEvent?.Invoke(sender, args);
        }

        protected abstract X CreateAuthorizeEventArgs(TcpAuthorizeBaseEventArgs<Z, A> args);

        public event AuthorizeEvent<Z, X, A> AuthorizeEvent
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
