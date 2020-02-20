using Newtonsoft.Json;
using PHS.Networking.Enums;
using PHS.Networking.Events;
using PHS.Networking.Models;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Server.Services;
using PHS.Networking.Services;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server
{
    public class TcpNETServerAuth<T> : CoreNetworking<TcpConnectionServerAuthEventArgs<T>, TcpMessageServerAuthEventArgs<T>, TcpErrorServerAuthEventArgs<T>>, 
        ITcpNETServerAuth<T>
    {
        private readonly TcpHandler _handler;
        private readonly IUserService<T> _userService;
        private readonly IParamsTcpServerAuth _parameters;
        private readonly TcpConnectionManagerAuth<T> _connectionManager;
        private Timer _timerPing;
        private volatile bool _isPingRunning;

        private const int PING_INTERVAL_SEC = 120;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpNETServerAuth(IParamsTcpServerAuth parameters,
            IUserService<T> userService,
            TcpHandler handler = null)
        { 
            _parameters = parameters;
            _userService = userService;
            _connectionManager = new TcpConnectionManagerAuth<T>();

            _handler = handler ?? new TcpHandler(_parameters);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
            _handler.Start();
        }
        public TcpNETServerAuth(IParamsTcpServerAuth parameters,
            IUserService<T> userService,
            X509Certificate certificate,
            TcpHandler handler = null)
        {
            _parameters = parameters;
            _userService = userService;
            _connectionManager = new TcpConnectionManagerAuth<T>();

            _handler = handler ?? new TcpHandler(_parameters, certificate);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
            _handler.Start();
        }
        public TcpNETServerAuth(IParamsTcpServerAuth parameters,
            IUserService<T> userService,
            string certificateIssuedTo,
            StoreLocation storeLocation,
            TcpHandler handler = null)
        {
            _parameters = parameters;
            _userService = userService;
            _connectionManager = new TcpConnectionManagerAuth<T>();

            X509Certificate certificate = null;

            var store = new X509Store(StoreName.My, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            foreach (var currentCertificate
                in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name != null && currentCertificate.Subject.Equals(certificateIssuedTo))
                {
                    certificate = currentCertificate;
                    break;
                }
            }

            if (certificate == null)
            {
                throw new Exception("Could not locate certificate in the Windows Certificate store");
            }

            _handler = handler ?? new TcpHandler(_parameters, certificate);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
            _handler.Start();
        }

        public async Task BroadcastToAllAuthorizedUsersAsync<S>(S packet) where S : IPacket
        {
            if (_handler.IsServerRunning)
            {
                foreach (var identity in _connectionManager.GetAllIdentitiesAuthorized())
                {
                    foreach (var connection in identity.Connections.ToList())
                    {
                        await SendToConnectionAsync(packet, connection, identity.Id);
                    }
                }
            }
        }
        public async Task BroadcastToAllAuthorizedUsersAsync(string message)
        {
            await BroadcastToAllAuthorizedUsersAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task BroadcastToAllAuthorizedUsersAsync<S>(S packet, IConnectionServer connectionSending) where S : IPacket
        {
            if (_handler != null &&
                _handler.IsServerRunning)
            {
                foreach (var identity in _connectionManager.GetAllIdentitiesAuthorized())
                {
                    foreach (var connection in identity.Connections.ToList())
                    {
                        if (connection.Client.GetHashCode() != connection.Client.GetHashCode())
                        {
                            await SendToConnectionAsync(packet, connection, identity.Id);
                        }
                    }
                }
            }
        }
        public async Task BroadcastToAllAuthorizedUsersAsync(string message, IConnectionServer connectionSending)
        {
            await BroadcastToAllAuthorizedUsersAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            }, connectionSending);
        }

        public async Task BroadcastToAllAuthorizedUsersRawAsync(string message)
        {
            if (_handler != null &&
                _handler.IsServerRunning)
            {
                foreach (var identity in _connectionManager.GetAllIdentitiesAuthorized())
                {
                    foreach (var connection in identity.Connections.ToList())
                    {
                        await SendToConnectionRawAsync(message, connection, identity.Id);
                    }
                }
            }
        }

        public async Task SendToUserAsync<S>(S packet, T userId) where S : IPacket
        {
            if (_handler != null &&
                _handler.IsServerRunning &&
                _connectionManager.IsUserConnected(userId))
            {
                var user = _connectionManager.GetIdentity(userId);

                foreach (var connection in user.Connections.ToList())
                {
                    await SendToConnectionAsync(packet, connection, userId);
                }
            }
        }
        public async Task SendToUserAsync(string message, T userId)
        {
            await SendToUserAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            }, userId);
        }
        public async Task SendToUserRawAsync(string message, T userId)
        {
            if (_handler != null &&
                _handler.IsServerRunning &&
                _connectionManager.IsUserConnected(userId))
            {
                var user = _connectionManager.GetIdentity(userId);

                foreach (var connection in user.Connections)
                {
                    await SendToConnectionRawAsync(message, connection, userId);
                }
            }
        }

        public async Task<bool> SendToConnectionAsync<S>(S packet, IConnectionServer connection, T userId) where S : IPacket
        {
            if (_handler != null &&
                _handler.IsServerRunning)
            {
                if (_connectionManager.IsConnectionOpen(connection))
                {
                    try
                    {
                        await _handler.SendAsync(packet, connection);

                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
                            Packet = packet,
                            UserId = userId
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                        {
                            Connection = connection,
                            Exception = ex,
                            Message = ex.Message,
                            UserId = userId
                        });

                        DisconnectConnection(connection);

                        return false;
                    }
                }

                if (_connectionManager.IsConnectionAuthorized(connection))
                {
                    try
                    {
                        await _handler.SendAsync(packet, connection);

                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Packet = packet,
                            UserId = userId,
                            Connection = connection
                        });

                        return true;
                    }
                    catch (Exception ex)
                    {
                        FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                        {
                            Connection = connection,
                            Exception = ex,
                            Message = ex.Message,
                            UserId = userId
                        });

                        DisconnectConnection(connection);

                        return false;
                    }
                }
            }

            return false;
        }
        public async Task<bool> SendToConnectionAsync(string message, IConnectionServer connection, T userId)
        {
            if (_handler != null &&
                _handler.IsServerRunning)
            {
                var packet = new Packet
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow
                };

                if (_connectionManager.IsConnectionOpen(connection))
                {
                    try
                    {
                        await _handler.SendAsync(packet, connection);

                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Connection = connection,
                            Packet = packet,
                            UserId = userId,
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                        {
                            Connection = connection,
                            Exception = ex,
                            Message = ex.Message,
                            UserId = userId
                        });

                        DisconnectConnection(connection);

                        return false;
                    }
                }

                if (_connectionManager.IsConnectionAuthorized(connection))
                {
                    try
                    {
                        await _handler.SendAsync(packet, connection);

                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            Packet = packet,
                            UserId = userId,
                            Connection = connection
                        });

                        return true;
                    }
                    catch (Exception ex)
                    {
                        FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                        {
                            Connection = connection,
                            Exception = ex,
                            Message = ex.Message,
                            UserId = userId
                        });

                        DisconnectConnection(connection);

                        return false;
                    }
                }
            }

            return false;
        }
        public async Task<bool> SendToConnectionRawAsync(string message, IConnectionServer connection, T userId)
        {
            if (_handler != null &&
                _handler.IsServerRunning)
            {
                if (_connectionManager.IsConnectionOpen(connection))
                {
                    try
                    {
                        await _handler.SendRawAsync(message, connection);

                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Packet = new Packet
                            {
                                Data = message,
                                Timestamp = DateTime.UtcNow
                            },
                            Connection = connection,
                            UserId = userId
                        });

                        return true;
                    }
                    catch (Exception ex)
                    {
                        FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                        {
                            Connection = connection,
                            Exception = ex,
                            Message = ex.Message,
                            UserId = userId
                        });

                        DisconnectConnection(connection);
                    }
                }

                if (_connectionManager.IsConnectionAuthorized(connection))
                {
                    try
                    {
                        await _handler.SendAsync(message, connection);

                        FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                        {
                            Message = message,
                            Packet = new Packet
                            {
                                Data = message,
                                Timestamp = DateTime.UtcNow
                            },
                            UserId = userId,
                            Connection = connection,
                            MessageEventType = MessageEventType.Sent,
                        });

                        return true;
                    }
                    catch (Exception ex)
                    {
                        FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                        {
                            Connection = connection,
                            Exception = ex,
                            Message = ex.Message,
                            UserId = userId
                        });

                        DisconnectConnection(connection);
                    }
                }
            }

            return false;
        }
        public bool DisconnectConnection(IConnectionServer connection)
        {
            return _handler.DisconnectClient(connection);
        }

        private Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (!_connectionManager.IsConnectionOpen(args.Connection))
                    {
                        if (_connectionManager.AddConnection(args.Connection))
                        {
                            FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                            {
                                ConnectionEventType = args.ConnectionEventType,
                                Connection = args.Connection
                            });
                        }
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    if (_connectionManager.IsConnectionOpen(args.Connection))
                    {
                        _connectionManager.RemoveConnection(args.Connection, true);

                        FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                        {
                            Connection = args.Connection,
                            ConnectionEventType = args.ConnectionEventType,
                        });
                    }

                    if (_connectionManager.IsConnectionAuthorized(args.Connection))
                    {
                        var identity = _connectionManager.GetIdentity(args.Connection);
                        _connectionManager.RemoveConnectionAuthorized(args.Connection, true);

                        FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                        {
                            Connection = args.Connection,
                            ConnectionEventType = args.ConnectionEventType,
                            UserId = identity.Id,
                        });
                    }
                    break;
               
                case ConnectionEventType.Connecting:
                    FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                    {
                        ConnectionEventType = args.ConnectionEventType,
                        Connection = args.Connection,
                    });
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        private async Task OnMessageEventAsync(object sender, TcpMessageServerEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (_connectionManager.IsConnectionOpen(args.Connection))
                    {
                        await CheckIfAuthorizedAsync(args);
                    }
                    else if (_connectionManager.IsConnectionAuthorized(args.Connection))
                    {
                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            args.Connection.HasBeenPinged = false;
                        }
                        else
                        {
                            var identity = _connectionManager.GetIdentity(args.Connection);

                            FireEvent(this, new TcpMessageServerAuthEventArgs<T>
                            {
                                Message = args.Message,
                                MessageEventType = MessageEventType.Receive,
                                Packet = args.Packet,
                                UserId = identity.Id,
                                Connection = args.Connection,
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private Task OnServerEvent(object sender, ServerEventArgs args)
        {
            switch (args.ServerEventType)
            {
                case ServerEventType.Start:
                    FireEvent(this, new ServerEventArgs
                    {
                        ServerEventType = ServerEventType.Start,
                    });

                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(this, args);
                    _timerPing = new Timer(OnTimerPingTick, null, PING_INTERVAL_SEC * 1000, PING_INTERVAL_SEC * 1000);
                    break;
                case ServerEventType.Stop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(this, args);
                    Thread.Sleep(5000);
                    _handler.Start();
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        private Task OnErrorEvent(object sender, TcpErrorServerEventArgs args)
        {
            if (_connectionManager.IsConnectionAuthorized(args.Connection))
            {
                var identity = _connectionManager.GetIdentity(args.Connection);

                FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    UserId = identity.Id,
                    Connection = args.Connection
                });
            }
            return Task.CompletedTask;
        }
        private void OnTimerPingTick(object state)
        {
            if (!_isPingRunning)
            {
                _isPingRunning = true;

                Task.Run(async () =>
                {
                    foreach (var identity in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in identity.Connections)
                        {
                            try
                            {
                                if (connection.HasBeenPinged)
                                {
                                    // Already been pinged, no response, disconnect
                                    await SendToConnectionRawAsync("No ping response - disconnected.", connection, identity.Id);
                                    _connectionManager.RemoveConnectionAuthorized(connection, true);
                                    _handler.DisconnectClient(connection);
                                }
                                else
                                {
                                    connection.HasBeenPinged = true;
                                    await SendToConnectionRawAsync("Ping", connection, identity.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                                {
                                    Connection = connection,
                                    Exception = ex,
                                    Message = ex.Message,
                                    UserId = identity.Id
                                });
                            }
                        }
                    }

                    _isPingRunning = false;
                });
            }
        }

        protected virtual async Task<bool> CheckIfAuthorizedAsync(TcpMessageServerEventArgs args)
        {
            try
            {
                // Check for token here
                if (_connectionManager.IsConnectionOpen(args.Connection))
                {
                    _connectionManager.RemoveConnection(args.Connection, false);

                    if (args.Message.Length < "oauth:".Length ||
                        !args.Message.ToLower().StartsWith("oauth:"))
                    {
                        await SendToConnectionRawAsync(_parameters.ConnectionUnauthorizedString, args.Connection, default);
                        DisconnectConnection(args.Connection);

                        FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = args.Connection
                        });
                        return false;
                    }

                    var token = args.Message.Substring("oauth:".Length);

                    var userId = await _userService.GetIdAsync(token);

                    if (userId == null)
                    {
                        await SendToConnectionRawAsync(_parameters.ConnectionUnauthorizedString, args.Connection, default);
                        DisconnectConnection(args.Connection);

                        FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = args.Connection
                        });
                        return false;
                    }

                    var identity = _connectionManager.AddConnectionAuthorized(userId, args.Connection);

                    await SendToConnectionRawAsync(_parameters.ConnectionSuccessString, args.Connection, userId);

                    FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        UserId = identity.Id,
                        Connection = args.Connection,
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerAuthEventArgs<T>
                {
                    Connection = args.Connection,
                    Exception = ex,
                    Message = ex.Message,
                    UserId = default
                });
            }

            await SendToConnectionRawAsync(_parameters.ConnectionUnauthorizedString, args.Connection, default);
            DisconnectConnection(args.Connection);

            FireEvent(this, new TcpConnectionServerAuthEventArgs<T>
            {
                ConnectionEventType = ConnectionEventType.Disconnect,
                Connection = args.Connection,
            });
            return false;
        }
        
        protected virtual void FireEvent(object sender, ServerEventArgs args)
        {
            _serverEvent?.Invoke(sender, args);
        }

        public override void Dispose()
        {
            foreach (var item in _connectionManager.GetAllConnections())
            {
                _connectionManager.RemoveConnection(item, true);
            }

            foreach (var item in _connectionManager.GetAllIdentitiesAuthorized())
            {
                foreach (var connection in item.Connections)
                {
                    _connectionManager.RemoveConnectionAuthorized(connection, true);
                }
            }

            if (_handler != null)
            {
                _handler.ConnectionEvent -= OnConnectionEvent;
                _handler.MessageEvent -= OnMessageEventAsync;
                _handler.ErrorEvent -= OnErrorEvent;
                _handler.ServerEvent -= OnServerEvent;
                _handler.Dispose();
            }

            if (_timerPing != null)
            {
                _timerPing.Dispose();
                _timerPing = null;
            }
            base.Dispose();
        }

        public TcpListener Server
        {
            get
            {
                return _handler != null ? _handler.Server : null;
            }
        }
        public bool IsServerRunning
        {
            get
            {
                return _handler != null ? _handler.IsServerRunning : false;
            }
        }
        public IConnectionServer[] Connections
        {
            get
            {
                return _connectionManager.GetAllConnections();
            }
        }
        public IUserConnections<T>[] UserConnections
        {
            get
            {
                return _connectionManager.GetAllIdentitiesAuthorized();
            }
        }

        public event NetworkingEventHandler<ServerEventArgs> ServerEvent
        {
            add
            {
                _serverEvent += value;
            }
            remove
            {
                _serverEvent -= value;
            }
        }
    }
}
