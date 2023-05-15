using PHS.Networking.Server.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Models;
using PHS.Networking.Server.Managers;
using PHS.Networking.Enums;

namespace Tcp.NET.Server
{
    public abstract class TcpNETServerAuthBase<T, U, V, W, X, Y, Z, A, B> : 
        TcpNETServerBase<T, U, V, W, X, Y, Z>
        where T : TcpConnectionServerAuthBaseEventArgs<Z, A>
        where U : TcpMessageServerAuthBaseEventArgs<Z, A>
        where V : TcpErrorServerAuthBaseEventArgs<Z, A>
        where W : ParamsTcpServerAuth
        where X : TcpHandlerServerAuthBase<T, U, V, W, B, Z, A>
        where Y : ConnectionManagerAuth<Z, A>
        where Z : IdentityTcpServer<A>
        where B : TcpAuthorizeBaseEventArgs<Z, A>
    {
        protected readonly IUserService<A> _userService;

        public TcpNETServerAuthBase(W parameters,
            IUserService<A> userService) : base(parameters)
        { 
            _userService = userService;

            _handler.AuthorizeEvent += OnAuthorizeEvent;
        }

        public TcpNETServerAuthBase(W parameters,
            IUserService<A> userService,
            byte[] certificate,
            string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
            _userService = userService;

            _handler.AuthorizeEvent += OnAuthorizeEvent;
        }

        public virtual async Task SendToUserAsync(string message, A userId, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                var connections = _connectionManager.GetAllConnectionsForUser(userId);

                foreach (var connection in connections)
                {
                    await SendToConnectionAsync(message, connection, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        public virtual async Task SendToUserAsync(byte[] message, A userId, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                var connections = _connectionManager.GetAllConnectionsForUser(userId);

                foreach (var connection in connections)
                {
                    await SendToConnectionAsync(message, connection, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected virtual void OnAuthorizeEvent(object sender, B args)
        {
            Task.Run(async () =>
            {
                try
                {
                    // Check for token here
                    if (args.Connection != null &&
                        !args.Connection.IsAuthorized)
                    {
                        if (args.Token.Length > 0 && await _userService.IsValidTokenAsync(args.Token, args.CancellationToken).ConfigureAwait(false))
                        {
                            args.Connection.UserId = await _userService.GetIdAsync(args.Token, args.CancellationToken);
                            args.Connection.IsAuthorized = true;

                            if (!string.IsNullOrWhiteSpace(_parameters.ConnectionSuccessString))
                            {
                                await SendToConnectionAsync(_parameters.ConnectionSuccessString, args.Connection, args.CancellationToken).ConfigureAwait(false);
                            }

                            await _handler.AuthorizeCallbackAsync(args, args.CancellationToken).ConfigureAwait(false);
                            return;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(_parameters.ConnectionUnauthorizedString))
                    {
                        await SendToConnectionAsync(_parameters.ConnectionUnauthorizedString, args.Connection, args.CancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                    {
                        Connection = args.Connection,
                        Exception = ex,
                        Message = ex.Message,
                        CancellationToken = args.CancellationToken
                    }));
                }

                await DisconnectConnectionAsync(args.Connection, args.CancellationToken).ConfigureAwait(false);
            });
        }

        protected override void OnConnectionEvent(object sender, T args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (!_connectionManager.AddUser(args.Connection))
                    {
                        Task.Run(async () =>
                        {
                            FireEvent(this, args);
                            await DisconnectConnectionAsync(args.Connection, args.CancellationToken).ConfigureAwait(false);
                        });
                        return;
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.RemoveConnection(args.Connection.ConnectionId);
                    break;
                default:
                    break;
            }

            FireEvent(this, args);
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
