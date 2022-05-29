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
        }
        public TcpNETServerAuth(ParamsTcpServerAuth parameters,
            IUserService<T> userService,
            byte[] certificate,
            string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
            _userService = userService;
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
                    if (connection.TcpClient.Connected)
                    {
                        await SendToConnectionAsync(message, connection, cancellationToken);

                    }
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
                    if (connection.TcpClient.Connected)
                    {
                        await SendToConnectionAsync(message, connection, cancellationToken);

                    }
                }
            }
        }

        protected override void OnConnectionEvent(object sender, TcpConnectionServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionManager.Add(args.Connection.ConnectionId, args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.Remove(args.Connection.ConnectionId);

                    FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                    {
                        Connection = args.Connection,
                        ConnectionEventType = args.ConnectionEventType,
                    });
                    break;
                default:
                    break;
            }
        }
        protected override void OnMessageEvent(object sender, TcpMessageServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = args.Connection,
                        Message = args.Message,
                        Bytes = args.Bytes
                        
                    });
                    break;
                case MessageEventType.Receive:
                    if (!args.Connection.IsAuthorized)
                    {
                        Task.Run(async () =>
                        {
                            await CheckIfAuthorizedAsync(args);
                        });
                    }
                    else
                    {
                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            MessageEventType = MessageEventType.Receive,
                            Message = args.Message,
                            Connection = args.Connection,
                            Bytes = args.Bytes
                        });
                    }
                    break;
                default:
                    break;
            }
        }
        protected override void OnErrorEvent(object sender, TcpErrorServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            FireEvent(this, new TcpErrorServerAuthEventArgs<T>
            {
                Exception = args.Exception,
                Message = args.Message,
                Connection = args.Connection
            });
        }
        
        protected virtual async Task<bool> CheckIfAuthorizedAsync(TcpMessageServerBaseEventArgs<IdentityTcpServer<T>> args)
        {
            try
            {
                // Check for token here
                if (args.Connection != null &&
                    args.Connection.TcpClient.Connected &&
                    !args.Connection.IsAuthorized)
                {
                    var message = Encoding.UTF8.GetString(args.Bytes);
                    if (message.Length < "oauth:".Length ||
                        !message.ToLower().StartsWith("oauth:"))
                    {
                        if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionUnauthorizedString))
                        {
                            await SendToConnectionAsync(_parameters.ConnectionUnauthorizedString, args.Connection);
                        }

                        await DisconnectConnectionAsync(args.Connection);

                        return false;
                    }

                    var token = message.Substring("oauth:".Length);

                    if (await _userService.IsValidTokenAsync(token))
                    {
                        args.Connection.UserId = await _userService.GetIdAsync(token);
                        args.Connection.IsAuthorized = true;

                        if (!_parameters.OnlyEmitBytes || !string.IsNullOrWhiteSpace(_parameters.ConnectionSuccessString))
                        {
                            await SendToConnectionAsync(_parameters.ConnectionSuccessString, args.Connection);
                        }

                        FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                        {
                            ConnectionEventType = ConnectionEventType.Connected,
                            Connection = args.Connection
                        });
                        return true;
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

            await SendToConnectionAsync("Error connecting to server", args.Connection);
            await DisconnectConnectionAsync(args.Connection);

            return false;
        }
    }
}
