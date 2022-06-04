using PHS.Networking.Server.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public abstract class TcpNETServerAuthBase<T, U, V, W, X, Y, Z, A, B> : 
        TcpNETServerBase<T, U, V, W, X, Y, Z>
        where T : TcpConnectionServerAuthBaseEventArgs<Z, A>
        where U : TcpMessageServerAuthBaseEventArgs<Z, A>
        where V : TcpErrorServerAuthBaseEventArgs<Z, A>
        where W : ParamsTcpServerAuth
        where X : TcpHandlerServerAuthBase<T, U, V, W, B, Z, A>
        where Y : TcpConnectionManagerAuthBase<Z, A>
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
                var connections = _connectionManager.GetAll(userId);

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
                var connections = _connectionManager.GetAll(userId);

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
                        args.Connection.TcpClient.Connected &&
                        !args.Connection.IsAuthorized)
                    {
                        if (args.Token.Length <= 0 || !await _userService.IsValidTokenAsync(args.Token, _cancellationToken).ConfigureAwait(false))
                        {
                            if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionUnauthorizedString))
                            {
                                await SendToConnectionAsync(_parameters.ConnectionUnauthorizedString, args.Connection, _cancellationToken).ConfigureAwait(false);
                            }

                            await DisconnectConnectionAsync(args.Connection, _cancellationToken).ConfigureAwait(false);
                            return;
                        }

                        args.Connection.UserId = await _userService.GetIdAsync(args.Token, _cancellationToken).ConfigureAwait(false);
                        args.Connection.IsAuthorized = true;

                        if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionSuccessString))
                        {
                            await SendToConnectionAsync(_parameters.ConnectionSuccessString, args.Connection, _cancellationToken).ConfigureAwait(false);
                        }

                        await _handler.AuthorizeCallbackAsync(args, _cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionUnauthorizedString))
                    {
                        await SendToConnectionAsync(_parameters.ConnectionUnauthorizedString, args.Connection, _cancellationToken).ConfigureAwait(false);
                    }

                    await DisconnectConnectionAsync(args.Connection, _cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                    {
                        Connection = args.Connection,
                        Exception = ex,
                        Message = ex.Message
                    }));
                }
            });
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
