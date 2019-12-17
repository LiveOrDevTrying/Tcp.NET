using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using PHS.Core.Services;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;
using Tcp.NET.Server.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Core.Enums;
using Tcp.NET.Server.Auth.Events.Args;
using Tcp.NET.Server.Auth.Models;
using Tcp.NET.Server.Auth.Enums;
using Tcp.NET.Server.Auth.Interfaces;

namespace Tcp.NET.Server.Auth
{
    public class TcpNETServerAuth : TcpNETServer, ITcpNETServerAuth
    {
        protected readonly IUserService _userService;

        public TcpNETServerAuth(IParamsTcpAuthServer parameters,
            IUserService userService,
            ITcpConnectionManagerAuth connectionManager)
            : base(parameters, connectionManager)
        {
            _userService = userService;
        }

        public virtual bool BroadcastToAllAuthorizedUsers(PacketDTO packet)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in ConnectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections)
                        {
                            SendToConnection(packet, connection.Socket);
                        }
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual bool BroadcastToAllAuthorizedUsers(PacketDTO packet, Socket socketSending)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in ConnectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections)
                        {
                            if (connection.Socket.GetHashCode() != socketSending.GetHashCode())
                            {
                                SendToConnection(packet, connection.Socket);
                            }
                        }
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual bool BroadcastToAllAuthorizedUsersRaw(string message)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in ConnectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections)
                        {
                            SendToConnectionRaw(message, connection.Socket);
                        }
                    }
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        public virtual ICollection<IUserConnectionTcpDTO> GetAllConnections()
        {
            return ConnectionManager.GetAllIdentitiesAuthorized();
        }

        public virtual bool SendToUser(PacketDTO packet, Guid userId)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    ConnectionManager.IsUserConnected(userId))
                {
                    var user = ConnectionManager.GetIdentity(userId);

                    foreach (var connection in user.Connections)
                    {
                        _handler.Send(packet, connection.Socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Socket = connection.Socket,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                            UserId = user.UserId,
                        });
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual bool SendToUserRaw(string message, Guid userId)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    ConnectionManager.IsUserConnected(userId))
                {
                    var user = ConnectionManager.GetIdentity(userId);

                    foreach (var connection in user.Connections)
                    {
                        _handler.SendRaw(message, connection.Socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Socket = connection.Socket,
                            ArgsType = ArgsType.Message,
                            Packet = new PacketDTO
                            {
                                Action = (int)ActionType.SendToClient,
                                Data = message,
                                Timestamp = DateTime.UtcNow
                            },
                            UserId = user.UserId,
                        });
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }

        public override bool SendToConnection(PacketDTO packet, Socket socket)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    socket.Connected)
                {
                    if (ConnectionManager.IsConnectionUnauthorized(socket))
                    {
                        _handler.Send(packet, socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Socket = socket,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                        });

                        return true;
                    }

                    if (ConnectionManager.IsConnectionAuthorized(socket))
                    {
                        var identity = ConnectionManager.GetIdentity(socket);
                        _handler.Send(packet, socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Socket = Socket,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                            UserId = identity.UserId,
                        });

                        return true;
                    }
                }
            }
            catch
            { }

            return false;
        }
        public override bool SendToConnectionRaw(string message, Socket socket)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    socket.Connected)
                {
                    if (ConnectionManager.IsConnectionUnauthorized(socket))
                    {
                        _handler.SendRaw(message, socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Socket = socket,
                            ArgsType = ArgsType.Message,
                            Packet = new PacketDTO
                            {
                                Action = (int)ActionType.SendToClient,
                                Data = message,
                                Timestamp = DateTime.UtcNow
                            },
                        });

                        return true;
                    }

                    if (ConnectionManager.IsConnectionAuthorized(socket))
                    {
                        var identity = ConnectionManager.GetIdentity(socket);
                        _handler.Send(message, socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Socket = socket,
                            ArgsType = ArgsType.Message,
                            Packet = new PacketDTO
                            {
                                Action = (int)ActionType.SendToClient,
                                Data = message,
                                Timestamp = DateTime.UtcNow
                            },
                            UserId = identity.UserId,
                        });

                        return true;
                    }
                }
            }
            catch
            { }

            return false;
        }

        protected override Task OnConnectionEvent(object sender, TcpConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (!ConnectionManager.IsConnectionUnauthorized(args.Socket))
                    {
                        if (ConnectionManager.AddSocketUnauthorized(args.Socket))
                        {
                            FireEvent(this, new TcpConnectionAuthEventArgs
                            {
                                Socket = args.Socket,
                                ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                                ConnectionEventType = args.ConnectionEventType,
                                ConnectionType = TcpConnectionType.Connected,
                                ArgsType = ArgsType.Connection,
                            });
                        }
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    if (ConnectionManager.IsConnectionUnauthorized(args.Socket))
                    {
                        ConnectionManager.RemoveSocketUnauthorized(args.Socket, true);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized
                        });
                    }

                    if (ConnectionManager.IsConnectionAuthorized(args.Socket))
                    {
                        var identity = ConnectionManager.GetIdentity(args.Socket);
                        ConnectionManager.RemoveConnectionAuthorized(identity.GetConnection(args.Socket));

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            UserId = identity.UserId,
                            ConnectionAuthType = TcpConnectionAuthType.Authorized
                        });
                    }
                    break;
                case ConnectionEventType.ServerStart:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    _timerPing = new Timer(OnTimerPingTick, null, _parameters.PingIntervalSec * 1000, _parameters.PingIntervalSec * 1000);

                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionAuthType = TcpConnectionAuthType.Authorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.ServerStart,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.ServerStop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionAuthType = TcpConnectionAuthType.Authorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.ServerStop,
                        ArgsType = ArgsType.Connection
                    });

                    Thread.Sleep(5000);
                    _handler.Start(_parameters.Url, _parameters.Port, _parameters.EndOfLineCharacters);
                    break;
                case ConnectionEventType.Connecting:
                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.Connecting,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.MaxConnectionsReached:
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        protected override async Task OnMessageEventAsync(object sender, TcpMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (ConnectionManager.IsConnectionUnauthorized(args.Socket))
                    {
                        await CheckIfAuthorizedAsync(args);
                    }
                    else if (ConnectionManager.IsConnectionAuthorized(args.Socket))
                    {
                        var identity = ConnectionManager.GetIdentity(args.Socket);

                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            var connection = ConnectionManager.GetConnectionAuthorized(args.Socket);
                            connection.HasBeenPinged = false;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(args.Message))
                            {

                                try
                                {
                                    var packet = JsonConvert.DeserializeObject<PacketDTO>(args.Message);

                                    FireEvent(this, new TcpMessageAuthEventArgs
                                    {
                                        Message = packet.Data,
                                        MessageEventType = MessageEventType.Receive,
                                        Socket = args.Socket,
                                        ArgsType = ArgsType.Message,
                                        Packet = packet,
                                        UserId = identity.UserId
                                    });
                                }
                                catch
                                {
                                    FireEvent(this, new TcpMessageAuthEventArgs
                                    {
                                        Message = args.Message,
                                        MessageEventType = MessageEventType.Receive,
                                        Socket = args.Socket,
                                        ArgsType = ArgsType.Message,
                                        Packet = new PacketDTO
                                        {
                                            Action = (int)ActionType.SendToServer,
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        },
                                        UserId = identity.UserId
                                    });
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        protected override Task OnErrorEvent(object sender, TcpErrorEventArgs args)
        {
            if (ConnectionManager.IsConnectionAuthorized(args.Socket))
            {
                var identity = ConnectionManager.GetIdentity(args.Socket);

                FireEvent(this, new TcpErrorAuthEventArgs
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    Socket = args.Socket,
                    ArgsType = ArgsType.Error,
                    UserId = identity.UserId
                });
            }
            return Task.CompletedTask;
        }

        protected override void OnTimerPingTick(object state)
        {
            foreach (var identity in ConnectionManager.GetAllIdentitiesAuthorized())
            {
                var connectionsToRemove = new List<ConnectionSocketDTO>();

                foreach (var connection in identity.Connections)
                {
                    if (connection.HasBeenPinged)
                    {
                        // Already been pinged, no response, disconnect
                        connectionsToRemove.Add(connection);
                    }
                    else
                    {
                        connection.HasBeenPinged = true;
                        _handler.Send("Ping", connection.Socket);
                    }
                }

                foreach (var connectionToRemove in connectionsToRemove)
                {
                    ConnectionManager.RemoveConnectionAuthorized(connectionToRemove);
                    _handler.SendRaw("No ping response - disconnected.", connectionToRemove.Socket);
                    _handler.DisconnectClient(connectionToRemove.Socket);
                }
            }
        }

        protected virtual async Task<bool> CheckIfAuthorizedAsync(TcpMessageEventArgs args)
        {
            try
            {
                // Check for token here
                if (ConnectionManager.IsConnectionUnauthorized(args.Socket))
                {
                    ConnectionManager.RemoveSocketUnauthorized(args.Socket, false);

                    if (args.Message.Length < "oauth:".Length ||
                        !args.Message.ToLower().StartsWith("oauth:"))
                    {
                        _handler.SendRaw(Parameters.UnauthorizedString, args.Socket);
                        args.Socket.Disconnect(false);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            ConnectionType = TcpConnectionType.Disconnect,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection,
                            Socket = args.Socket,
                        });
                        return false;
                    }

                    var token = args.Message.Substring("oauth:".Length);

                    var userId = await _userService.GetUserIdAsync(token);

                    if (userId == null ||
                        userId == Guid.Empty)
                    {
                        _handler.SendRaw(Parameters.UnauthorizedString, args.Socket);
                        args.Socket.Disconnect(false);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection
                        });
                        return false;
                    }

                    var identity = ConnectionManager.AddConnectionAuthorized(userId, args.Socket);

                    _handler.SendRaw(Parameters.ConnectionSuccessString, args.Socket);

                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionType = TcpConnectionType.Connected,
                        ConnectionEventType = ConnectionEventType.Connected,
                        ConnectionAuthType = TcpConnectionAuthType.Authorized,
                        ArgsType = ArgsType.Connection,
                        UserId = identity.UserId
                    });
                    return true;
                }
            }
            catch
            { }

            _handler.SendRaw(Parameters.UnauthorizedString, args.Socket);
            args.Socket.Disconnect(false);

            FireEvent(this, new TcpConnectionAuthEventArgs
            {
                Socket = args.Socket,
                ConnectionType = TcpConnectionType.Disconnect,
                ConnectionEventType = ConnectionEventType.Disconnect,
                ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                ArgsType = ArgsType.Connection
            });
            return false;
        }

        public override void Dispose()
        {
            foreach (var item in ConnectionManager.GetAllSocketsUnauthorized())
            {
                ConnectionManager.RemoveSocketUnauthorized(item, true);
            }

            foreach (var item in ConnectionManager.GetAllIdentitiesAuthorized())
            {
                foreach (var connection in item.Connections)
                {
                    ConnectionManager.RemoveConnectionAuthorized(connection);
                }
            }

            if (_handler != null)
            {
                _handler.ConnectionEvent -= OnConnectionEvent;
                _handler.MessageEvent -= OnMessageEventAsync;
                _handler.ErrorEvent -= OnErrorEvent;
                _handler.Dispose();
            }

            if (_timerPing != null)
            {
                _timerPing.Dispose();
                _timerPing = null;
            }
            base.Dispose();
        }

        public ITcpConnectionManagerAuth ConnectionManager
        {
            get
            {
                return _connectionManager as ITcpConnectionManagerAuth;
            }
        }

        public IParamsTcpAuthServer Parameters
        {
            get
            {
                return _parameters as IParamsTcpAuthServer;
            }
        }
    }
}
