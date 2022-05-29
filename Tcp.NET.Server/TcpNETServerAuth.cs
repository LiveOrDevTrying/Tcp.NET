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
        TcpNETServerBase<
            TcpConnectionServerAuthEventArgs<T>, 
            TcpMessageServerAuthEventArgs<T>, 
            TcpErrorServerAuthEventArgs<T>,
            ParamsTcpServerAuth,
            TcpHandlerServerAuth<T>,
            TcpConnectionManagerAuth<T>,
            IdentityTcpServer<T>>,
        ITcpNETServerAuth<T>
    {
        protected readonly IUserService<T> _userService;

        public TcpNETServerAuth(ParamsTcpServerAuth parameters,
            IUserService<T> userService) : base(parameters)
        { 
            _userService = userService;

            _handler.AuthorizeEvent += OnAuthorizeEvent;
        }
        public TcpNETServerAuth(ParamsTcpServerAuth parameters,
            IUserService<T> userService,
            byte[] certificate,
            string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
            _userService = userService;

            _handler.AuthorizeEvent += OnAuthorizeEvent;
        }

        protected override TcpConnectionManagerAuth<T> CreateTcpConnectionManager()
        {
            return new TcpConnectionManagerAuth<T>();
        }
        protected override TcpHandlerServerAuth<T> CreateTcpHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate == null
                ? new TcpHandlerServerAuth<T>(_parameters)
                : new TcpHandlerServerAuth<T>(_parameters, certificate, certificatePassword);
        }

        public virtual async Task SendToUserAsync(string message, T userId, IdentityTcpServer<T> connectionSending = null, CancellationToken cancellationToken = default)
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
        public virtual async Task SendToUserAsync(byte[] message, T userId, IdentityTcpServer<T> connectionSending = null, CancellationToken cancellationToken = default)
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
        protected virtual void OnAuthorizeEvent(object sender, TcpAuthorizeEventArgs<IdentityTcpServer<T>, T> args)
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
                        if (args.Token.Length <= 0)
                        {
                            if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionUnauthorizedString))
                            {
                                await SendToConnectionAsync(_parameters.ConnectionUnauthorizedString, args.Connection, _cancellationToken).ConfigureAwait(false);
                            }

                            await DisconnectConnectionAsync(args.Connection, _cancellationToken).ConfigureAwait(false);
                            return;
                        }

                        if (await _userService.IsValidTokenAsync(args.Token, _cancellationToken).ConfigureAwait(false))
                        {
                            args.Connection.UserId = await _userService.GetIdAsync(args.Token, _cancellationToken).ConfigureAwait(false);
                            args.Connection.IsAuthorized = true;

                            if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionSuccessString))
                            {
                                await SendToConnectionAsync(_parameters.ConnectionSuccessString, args.Connection, _cancellationToken).ConfigureAwait(false);
                            }

                            await _handler.AuthorizeCallbackAsync(args, _cancellationToken).ConfigureAwait(false);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                    {
                        Connection = args.Connection,
                        Exception = ex,
                        Message = ex.Message
                    });
                }

                if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionUnauthorizedString))
                {
                    await SendToConnectionAsync(_parameters.ConnectionUnauthorizedString, args.Connection, _cancellationToken).ConfigureAwait(false);
                }

                await DisconnectConnectionAsync(args.Connection, _cancellationToken).ConfigureAwait(false);
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
