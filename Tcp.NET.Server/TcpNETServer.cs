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
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Events.Args;

namespace Tcp.NET.Server
{
    public class TcpNETServer : 
        CoreNetworking<TcpConnectionAuthEventArgs, TcpMessageAuthEventArgs, TcpErrorAuthEventArgs>, 
        ITcpNETServer
    {
        protected readonly TcpHandler _handler;
        protected readonly ITcpConnectionManager _connectionManager;
        protected readonly ParamsTcpServer _parameters;
        protected readonly IUserService _userService;

        protected Timer _timerPing;

        public TcpNETServer(ParamsTcpServer parameters,
            IUserService userService,
            ITcpConnectionManager connectionManager)
        {
            _parameters = parameters;
            _userService = userService;
            _connectionManager = connectionManager;

            _handler = new TcpHandler();
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.Start(_parameters.Url, _parameters.Port, _parameters.EndOfLineCharacters);
        }

        public virtual bool BroadcastToAllAuthorizedUsers(PacketDTO packet)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections)
                        {
                            SendToSocket(packet, connection.Socket);
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
                    foreach (var authorizedUser in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections)
                        {
                            if (connection.Socket.GetHashCode() != socketSending.GetHashCode())
                            {
                                SendToSocket(packet, connection.Socket);
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
                    foreach (var authorizedUser in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections)
                        {
                            SendToSocketRaw(message, connection.Socket);
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
            return _connectionManager.GetAllIdentitiesAuthorized();
        }

        public virtual bool SendToUser(PacketDTO packet, Guid userId)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsUserConnected(userId))
                {
                    var user = _connectionManager.GetIdentity(userId);

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
                            ConnectionSocket = connection,
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
                    _connectionManager.IsUserConnected(userId))
                {
                    var user = _connectionManager.GetIdentity(userId);

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
                            ConnectionSocket = connection,
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

        public virtual bool SendToSocket(PacketDTO packet, Socket socket)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    socket.Connected)
                {
                    if (_connectionManager.IsConnectionUnauthorized(socket))
                    {
                        _handler.Send(packet, socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Socket = socket,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                            ConnectionSocket = new ConnectionSocketDTO
                            {
                                Socket = socket
                            },
                        });

                        return true;
                    }

                    if (_connectionManager.IsConnectionAuthorized(socket))
                    {
                        var identity = _connectionManager.GetIdentity(socket);
                        _handler.Send(packet, socket);

                        FireEvent(this, new TcpMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Socket = socket,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                            ConnectionSocket = identity.GetConnection(socket),
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
        public virtual bool SendToSocketRaw(string message, Socket socket)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    socket.Connected)
                {
                    if (_connectionManager.IsConnectionUnauthorized(socket))
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
                            ConnectionSocket = new ConnectionSocketDTO
                            {
                                Socket = socket
                            },
                        });

                        return true;
                    }

                    if (_connectionManager.IsConnectionAuthorized(socket))
                    {
                        var identity = _connectionManager.GetIdentity(socket);
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
                            ConnectionSocket = identity.GetConnection(socket),
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

        public virtual bool DisconnectClient(Socket socket)
        {
            return _handler.DisconnectClient(socket);
        }

        protected virtual Task OnConnectionEvent(object sender, TcpConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (!_connectionManager.IsConnectionUnauthorized(args.Socket))
                    {
                        if (_connectionManager.AddSocketUnauthorized(args.Socket))
                        {
                            FireEvent(this, new TcpConnectionAuthEventArgs
                            {
                                Socket = args.Socket,
                                ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                                ConnectionEventType = args.ConnectionEventType,
                                ConnectionType = TcpConnectionType.Authorization,
                                ConnectionSocket = new ConnectionSocketDTO
                                {
                                    Socket = args.Socket
                                },
                                ArgsType = ArgsType.Connection,
                            });
                        }
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    if (_connectionManager.IsConnectionUnauthorized(args.Socket))
                    {
                        _connectionManager.RemoveSocketUnauthorized(args.Socket, true);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            ConnectionSocket = new ConnectionSocketDTO
                            {
                                Socket = args.Socket
                            }
                        });
                    }

                    if (_connectionManager.IsConnectionAuthorized(args.Socket))
                    {
                        var identity = _connectionManager.GetIdentity(args.Socket);
                        _connectionManager.RemoveConnectionAuthorized(identity.GetConnection(args.Socket));

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            ConnectionSocket = identity.GetConnection(args.Socket),
                            UserId = identity.UserId
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
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.ServerStop,
                        ArgsType = ArgsType.Connection,
                    });

                    Thread.Sleep(5000);
                    _handler.Start(_parameters.Url, _parameters.Port, _parameters.EndOfLineCharacters);
                    break;
                case ConnectionEventType.Connecting:
                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.Connecting,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.MaxConnectionsReached:
                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.MaxConnectionsReached,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        protected virtual async Task OnMessageEventAsync(object sender, TcpMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (_connectionManager.IsConnectionUnauthorized(args.Socket))
                    {
                        await CheckIfAuthorizedAsync(args);
                    }
                    else if (_connectionManager.IsConnectionAuthorized(args.Socket))
                    {
                        var identity = _connectionManager.GetIdentity(args.Socket);

                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            var connection = _connectionManager.GetConnectionAuthorized(args.Socket);
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
                                        ConnectionSocket = identity.GetConnection(args.Socket),
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
                                        ConnectionSocket = identity.GetConnection(args.Socket),
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
        protected virtual Task OnErrorEvent(object sender, TcpErrorEventArgs args)
        {
            if (_connectionManager.IsConnectionAuthorized(args.Socket))
            {
                var identity = _connectionManager.GetIdentity(args.Socket);

                FireEvent(this, new TcpErrorAuthEventArgs
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    Socket = args.Socket,
                    ArgsType = ArgsType.Error,
                    ConnectionSocket = identity.GetConnection(args.Socket),
                    UserId = identity.UserId
                });
            }
            return Task.CompletedTask;
        }

        protected virtual void OnTimerPingTick(object state)
        {
            foreach (var identity in _connectionManager.GetAllIdentitiesAuthorized())
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
                    _connectionManager.RemoveConnectionAuthorized(connectionToRemove);
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
                if (_connectionManager.IsConnectionUnauthorized(args.Socket))
                {
                    _connectionManager.RemoveSocketUnauthorized(args.Socket, false);

                    if (args.Message.Length < "oauth:".Length ||
                        !args.Message.ToLower().StartsWith("oauth:"))
                    {
                        _handler.SendRaw(_parameters.UnauthorizedString, args.Socket);
                        args.Socket.Disconnect(false);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionType = TcpConnectionType.Authorization,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection,
                            ConnectionSocket = new ConnectionSocketDTO
                            {
                                Socket = args.Socket
                            },
                        });
                        return false;
                    }

                    var token = args.Message.Substring("oauth:".Length);

                    var userId = await _userService.GetUserIdAsync(token);

                    if (userId == null ||
                        userId == Guid.Empty)
                    {
                        _handler.SendRaw(_parameters.UnauthorizedString, args.Socket);
                        args.Socket.Disconnect(false);

                        FireEvent(this, new TcpConnectionAuthEventArgs
                        {
                            Socket = args.Socket,
                            ConnectionType = TcpConnectionType.Authorization,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection,
                            ConnectionSocket = new ConnectionSocketDTO
                            {
                                Socket = args.Socket
                            }
                        });
                        return false;
                    }

                    var identity = _connectionManager.AddConnectionAuthorized(userId, args.Socket);

                    _handler.SendRaw(_parameters.ConnectionSuccessString, args.Socket);

                    FireEvent(this, new TcpConnectionAuthEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionType = TcpConnectionType.Authorization,
                        ConnectionEventType = ConnectionEventType.Connected,
                        ConnectionAuthType = TcpConnectionAuthType.Authorized,
                        ArgsType = ArgsType.Connection,
                        ConnectionSocket = identity.GetConnection(args.Socket),
                        UserId = identity.UserId
                    });
                    return true;
                }
            }
            catch
            { }

            _handler.SendRaw(_parameters.UnauthorizedString, args.Socket);
            args.Socket.Disconnect(false);

            FireEvent(this, new TcpConnectionAuthEventArgs
            {
                Socket = args.Socket,
                ConnectionType = TcpConnectionType.Authorization,
                ConnectionEventType = ConnectionEventType.Disconnect,
                ConnectionAuthType = TcpConnectionAuthType.Unauthorized,
                ArgsType = ArgsType.Connection,
                ConnectionSocket = new ConnectionSocketDTO
                {
                    Socket = args.Socket
                }
            });
            return false;
        }

        protected virtual int GetRateLimit()
        {
            // 20 messages each 30000 MS with 80% of total time to buffer
            return Convert.ToInt32(Math.Ceiling(30000f / 20f * 0.8f));
        }

        public Socket Socket
        {
            get
            {
                return _handler?.Socket;
            }
        }
        public bool IsServerRunning
        {
            get
            {
                return _handler != null ? _handler.IsServerRunning : false;
            }
        }
        public TcpHandler TcpHandler
        {
            get
            {
                return _handler;
            }
        }
        public override void Dispose()
        {
            foreach (var item in _connectionManager.GetAllSocketsUnauthorized())
            {
                _connectionManager.RemoveSocketUnauthorized(item, true);
            }

            foreach (var item in _connectionManager.GetAllIdentitiesAuthorized())
            {
                foreach (var connection in item.Connections)
                {
                    _connectionManager.RemoveConnectionAuthorized(connection);
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
    }
}
