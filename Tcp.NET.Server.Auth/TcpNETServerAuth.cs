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

namespace Tcp.NET.Server.Auth
{
    public class TcpNETServerAuth : TcpNETServer, ITcpNETServerAuth
    {
        protected readonly IUserService _userService;

        public TcpNETServerAuth(ParamsTcpServer parameters,
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
                            SendToConnection(packet, connection);
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
                                SendToConnection(packet, connection);
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
                            SendToConnectionRaw(message, connection);
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
                        _handler.Send(packet, connection);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
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
                        _handler.SendRaw(message, connection);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
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

        public override bool SendToConnection(PacketDTO packet, ConnectionSocketDTO connection)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    connection.Socket.Connected)
                {
                    if (ConnectionManager.IsConnectionUnauthorized(connection.Socket))
                    {
                        _handler.Send(packet, connection);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                        });

                        return true;
                    }

                    if (ConnectionManager.IsConnectionAuthorized(connection.Socket))
                    {
                        var identity = ConnectionManager.GetIdentity(connection.Socket);
                        _handler.Send(packet, connection);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
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
        public override bool SendToConnectionRaw(string message, ConnectionSocketDTO connection)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    connection.Socket.Connected)
                {
                    if (ConnectionManager.IsConnectionUnauthorized(connection.Socket))
                    {
                        _handler.SendRaw(message, connection);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
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

                    if (ConnectionManager.IsConnectionAuthorized(connection.Socket))
                    {
                        var identity = ConnectionManager.GetIdentity(connection.Socket);
                        _handler.Send(message, connection);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
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
                    if (!ConnectionManager.IsConnectionUnauthorized(args.Connection.Socket))
                    {
                        if (ConnectionManager.AddSocketUnauthorized(args.Connection.Socket))
                        {
                            FireEvent(this, new TcpConnectionAuthEventArgs
                            {
                                Connection = args.Connection,
                                ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                                ConnectionEventType = args.ConnectionEventType,
                                ConnectionType = TcpConnectionType.Connected,
                                ArgsType = ArgsType.Connection,
                            });
                        }
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    if (ConnectionManager.IsConnectionUnauthorized(args.Connection.Socket))
                    {
                        ConnectionManager.RemoveSocketUnauthorized(args.Connection.Socket, true);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Connection = args.Connection,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized
                        });
                    }

                    if (ConnectionManager.IsConnectionAuthorized(args.Connection.Socket))
                    {
                        var identity = ConnectionManager.GetIdentity(args.Connection.Socket);
                        ConnectionManager.RemoveConnectionAuthorized(identity.GetConnection(args.Connection.Socket));

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Connection = args.Connection,
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
                        Connection = args.Connection,
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
                        Connection = args.Connection,
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
                        Connection = args.Connection,
                        ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.Connecting,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.MaxConnectionsReached:
                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Connection = args.Connection,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.MaxConnectionsReached,
                        ArgsType = ArgsType.Connection,
                        ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                    });
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
                    if (ConnectionManager.IsConnectionUnauthorized(args.Connection.Socket))
                    {
                        await CheckIfAuthorizedAsync(args);
                    }
                    else if (ConnectionManager.IsConnectionAuthorized(args.Connection.Socket))
                    {
                        var identity = ConnectionManager.GetIdentity(args.Connection.Socket);

                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            var connection = ConnectionManager.GetConnectionAuthorized(args.Connection.Socket);
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
                                        Connection = args.Connection,
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
                                        Connection = args.Connection,
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
            if (ConnectionManager.IsConnectionAuthorized(args.Connection.Socket))
            {
                var identity = ConnectionManager.GetIdentity(args.Connection.Socket);

                FireEvent(this, new TcpErrorAuthEventArgs
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    Connection = args.Connection,
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
                        _handler.Send("Ping", connection);
                    }
                }

                foreach (var connectionToRemove in connectionsToRemove)
                {
                    ConnectionManager.RemoveConnectionAuthorized(connectionToRemove);
                    _handler.SendRaw("No ping response - disconnected.", connectionToRemove);
                    _handler.DisconnectClient(connectionToRemove);
                }
            }
        }

        protected virtual async Task<bool> CheckIfAuthorizedAsync(TcpMessageEventArgs args)
        {
            try
            {
                // Check for token here
                if (ConnectionManager.IsConnectionUnauthorized(args.Connection.Socket))
                {
                    ConnectionManager.RemoveSocketUnauthorized(args.Connection.Socket, false);

                    if (args.Message.Length < "oauth:".Length ||
                        !args.Message.ToLower().StartsWith("oauth:"))
                    {
                        _handler.SendRaw(_parameters.UnauthorizedString, args.Connection);
                        args.Connection.Socket.Disconnect(false);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            ConnectionType = TcpConnectionType.Disconnect,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection,
                            Connection = args.Connection,
                        });
                        return false;
                    }

                    var token = args.Message.Substring("oauth:".Length);

                    var userId = await _userService.GetUserIdAsync(token);

                    if (userId == null ||
                        userId == Guid.Empty)
                    {
                        _handler.SendRaw(_parameters.UnauthorizedString, args.Connection);
                        args.Connection.Socket.Disconnect(false);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Connection = args.Connection,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection
                        });
                        return false;
                    }

                    var identity = ConnectionManager.AddConnectionAuthorized(userId, args.Connection.Socket);

                    _handler.SendRaw(_parameters.ConnectionSuccessString, args.Connection);

                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Connection = args.Connection,
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

            _handler.SendRaw(_parameters.UnauthorizedString, args.Connection);
            args.Connection.Socket.Disconnect(false);

            FireEvent(this, new TcpConnectionAuthEventArgs
            {
                Connection = args.Connection,
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
    }
}
