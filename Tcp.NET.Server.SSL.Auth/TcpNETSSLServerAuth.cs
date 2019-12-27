using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using PHS.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.SSL.Auth.Interfaces;
using Tcp.NET.Server.SSL.Auth.Events.Args;
using Tcp.NET.Core.SSL.Events.Args;
using Tcp.NET.Server.SSL.Auth.Enums;
using Tcp.NET.Core.SSL.Enums;
using System.Security.Cryptography.X509Certificates;
using Tcp.NET.Server.SSL.Models;
using Tcp.NET.Server.SSL.Handlers;
using System.Linq;

namespace Tcp.NET.Server.SSL.Auth
{
    public class TcpNETSSLServerAuth : TcpNETSSLServer, ITcpNETSSLServerAuth
    {
        protected readonly IUserService _userService;
        protected new readonly ITcpSSLConnectionManagerAuth _connectionManager;
        protected new readonly IParamsTcpSSLAuthServer _parameters;

        public TcpNETSSLServerAuth(IParamsTcpSSLAuthServer parameters,
            IUserService userService,
            ITcpSSLConnectionManagerAuth connectionManager,
            X509Certificate certificate)
            : base(parameters, connectionManager, certificate)
        {
            _userService = userService;
        }

        public TcpNETSSLServerAuth(IParamsTcpSSLAuthServer parameters,
           IUserService userService,
           ITcpSSLConnectionManagerAuth connectionManager,
           string certificateIssuedTo,
           StoreLocation storeLocation)
           : base(parameters, connectionManager, certificateIssuedTo, storeLocation)
        {
            _userService = userService;
        }

        public virtual async Task<bool> BroadcastToAllAuthorizedUsersAsync(PacketDTO packet)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections.ToList())
                        {
                            await SendToClientAsync(packet, connection.Client);
                        }
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual async Task<bool> BroadcastToAllAuthorizedUsersAsync(PacketDTO packet, TcpClient client)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections.ToList())
                        {
                            if (connection.Client.GetHashCode() != client.GetHashCode())
                            {
                                await SendToClientAsync(packet, connection.Client);
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
        public virtual async Task<bool> BroadcastToAllAuthorizedUsersRawAsync(string message)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning)
                {
                    foreach (var authorizedUser in _connectionManager.GetAllIdentitiesAuthorized())
                    {
                        foreach (var connection in authorizedUser.Connections.ToList())
                        {
                            await SendToClientRawAsync(message, connection.Client);
                        }
                    }
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        public virtual ICollection<IUserConnectionTcpClientSSLDTO> GetAllConnections()
        {
            return _connectionManager.GetAllIdentitiesAuthorized();
        }

        public virtual async Task<bool> SendToUserAsync(PacketDTO packet, Guid userId)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsUserConnected(userId))
                {
                    var user = _connectionManager.GetIdentity(userId);

                    foreach (var connection in user.Connections.ToList())
                    {
                        await _handler.SendAsync(packet, connection);

                        FireEvent(this, new TcpSSLMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                            UserId = user.UserId,
                            Client = connection.Client,
                        });
                    }

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual async Task<bool> SendToUserRawAsync(string message, Guid userId)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsUserConnected(userId))
                {
                    var user = _connectionManager.GetIdentity(userId);

                    foreach (var connection in user.Connections.ToList())
                    {
                        await _handler.SendRawAsync(message, connection);

                        FireEvent(this, new TcpSSLMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Client = connection.Client,
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

        public override async Task<bool> SendToClientAsync(PacketDTO packet, TcpClient client)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    client.Connected)
                {
                    if (_connectionManager.IsClientUnauthorized(client))
                    {
                        var connection = _connectionManager.GetConnection(client);

                        if (connection != null)
                        {
                            await _handler.SendAsync(packet, connection);

                            FireEvent(this, new TcpSSLMessageAuthEventArgs
                            {
                                Message = JsonConvert.SerializeObject(packet),
                                MessageEventType = MessageEventType.Sent,
                                Client = connection.Client,
                                ArgsType = ArgsType.Message,
                                Packet = packet,
                            });

                            return true;
                        }
                    }

                    if (_connectionManager.IsClientAuthorized(client))
                    {
                        var identity = _connectionManager.GetIdentity(client);
                        var connection = _connectionManager.GetClientAuthorized(client);
                        await _handler.SendAsync(packet, connection.GetConnection(client));

                        FireEvent(this, new TcpSSLMessageAuthEventArgs
                        {
                            Message = JsonConvert.SerializeObject(packet),
                            MessageEventType = MessageEventType.Sent,
                            ArgsType = ArgsType.Message,
                            Packet = packet,
                            UserId = identity.UserId,
                            Client = client,
                        });

                        return true;
                    }
                }
            }
            catch
            { }

            return false;
        }
        public override async Task<bool> SendToClientRawAsync(string message, TcpClient client)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    client.Connected)
                {
                    if (_connectionManager.IsClientUnauthorized(client))
                    {
                        var connection = _connectionManager.GetConnection(client);
                        await _handler.SendRawAsync(message, connection);

                        FireEvent(this, new TcpSSLMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Client = client,
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

                    if (_connectionManager.IsClientAuthorized(client))
                    {
                        var identity = _connectionManager.GetIdentity(client);
                        var connection = _connectionManager.GetClientAuthorized(client);
                        await _handler.SendAsync(message, connection.GetConnection(client));

                        FireEvent(this, new TcpSSLMessageAuthEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Client = client,
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

        protected override Task OnConnectionEvent(object sender, TcpSSLConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (!_connectionManager.IsClientUnauthorized(args.Client))
                    {
                        if (_connectionManager.AddClientUnauthorized(args.Client, args.Reader, args.Writer))
                        {
                            FireEvent(this, new TcpSSLConnectionAuthEventArgs
                            {
                                Client = args.Client,
                                Reader = args.Reader,
                                Writer = args.Writer,
                                ConnectionAuthType = TcpSSLConnectionAuthType.Unauthorized,
                                ConnectionEventType = args.ConnectionEventType,
                                ConnectionType = TcpSSLConnectionType.Connected,
                                ArgsType = ArgsType.Connection,
                            });
                        }
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    if (_connectionManager.IsClientUnauthorized(args.Client))
                    {
                        var connection = _connectionManager.GetClientUnauthorized(args.Client);
                        _connectionManager.RemoveClientUnauthorized(connection, true);

                        FireEvent(this, new TcpSSLConnectionAuthEventArgs
                        {
                            Client = args.Client,
                            Reader = args.Reader,
                            Writer = args.Writer,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpSSLConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            ConnectionAuthType = TcpSSLConnectionAuthType.Unauthorized,
                        });
                    }

                    if (_connectionManager.IsClientAuthorized(args.Client))
                    {
                        var identity = _connectionManager.GetIdentity(args.Client);
                        _connectionManager.RemoveConnectionAuthorized(identity.GetConnection(args.Client));

                        FireEvent(this, new TcpSSLConnectionAuthEventArgs
                        {
                            Client = args.Client,
                            Reader = args.Reader,
                            Writer = args.Writer,
                            ConnectionEventType = args.ConnectionEventType,
                            ConnectionType = TcpSSLConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                            UserId = identity.UserId,
                            ConnectionAuthType = TcpSSLConnectionAuthType.Authorized,
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

                    FireEvent(this, new TcpSSLConnectionAuthEventArgs
                    {
                        Client = args.Client,
                        Reader = args.Reader,
                        Writer = args.Writer,
                        ConnectionAuthType = TcpSSLConnectionAuthType.Authorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.ServerStart,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.ServerStop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(this, new TcpSSLConnectionAuthEventArgs
                    {
                        Client = args.Client,
                        Reader = args.Reader,
                        Writer = args.Writer,
                        ConnectionAuthType = TcpSSLConnectionAuthType.Authorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.ServerStop,
                        ArgsType = ArgsType.Connection
                    });


                    _handler.ConnectionEvent -= OnConnectionEvent;
                    _handler.MessageEvent -= OnMessageEventAsync;
                    _handler.ErrorEvent -= OnErrorEvent;
                    _handler.Dispose();

                    Thread.Sleep(5000);
                    _handler = _serverCertificate != null
                        ? new TcpHandlerSSL(_parameters.Port, _parameters.EndOfLineCharacters, _serverCertificate)
                        : new TcpHandlerSSL(_parameters.Port, _parameters.EndOfLineCharacters, _certificateIssuedTo, _storeLocation);
                    _handler.ConnectionEvent += OnConnectionEvent;
                    _handler.MessageEvent += OnMessageEventAsync;
                    _handler.ErrorEvent += OnErrorEvent;
                    break;
                case ConnectionEventType.Connecting:
                    FireEvent(this, new TcpSSLConnectionAuthEventArgs
                    {
                        Client = args.Client,
                        Reader = args.Reader,
                        Writer = args.Writer,
                        ConnectionAuthType = TcpSSLConnectionAuthType.Unauthorized,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.Connecting,
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
        protected override async Task OnMessageEventAsync(object sender, TcpSSLMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (_connectionManager.IsClientUnauthorized(args.Client))
                    {
                        await CheckIfAuthorizedAsync(args);
                    }
                    else if (_connectionManager.IsClientAuthorized(args.Client))
                    {
                        var identity = _connectionManager.GetIdentity(args.Client);

                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            var connection = _connectionManager.GetClientAuthorized(args.Client);
                            connection.GetConnection(args.Client).HasBeenPinged = false;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(args.Message))
                            {

                                try
                                {
                                    var packet = JsonConvert.DeserializeObject<PacketDTO>(args.Message);

                                    FireEvent(this, new TcpSSLMessageAuthEventArgs
                                    {
                                        Message = packet.Data,
                                        MessageEventType = MessageEventType.Receive,
                                        Client = args.Client,
                                        ArgsType = ArgsType.Message,
                                        Packet = packet,
                                        UserId = identity.UserId
                                    });
                                }
                                catch
                                {
                                    FireEvent(this, new TcpSSLMessageAuthEventArgs
                                    {
                                        Message = args.Message,
                                        MessageEventType = MessageEventType.Receive,
                                        Client = args.Client,
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
        protected override Task OnErrorEvent(object sender, TcpSSLErrorEventArgs args)
        {
            if (_connectionManager.IsClientAuthorized(args.Client))
            {
                var identity = _connectionManager.GetIdentity(args.Client);

                FireEvent(this, new TcpSSLErrorAuthEventArgs
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    ArgsType = ArgsType.Error,
                    UserId = identity.UserId,
                    Client = args.Client
                });
            }
            return Task.CompletedTask;
        }

        protected override void OnTimerPingTick(object state)
        {
            foreach (var identity in _connectionManager.GetAllIdentitiesAuthorized())
            {
                var connectionsToRemove = new List<ConnectionTcpClientSSLDTO>();

                foreach (var connection in identity.Connections.ToList())
                {
                    if (connection.HasBeenPinged)
                    {
                        // Already been pinged, no response, disconnect
                        connectionsToRemove.Add(connection);
                    }
                    else
                    {
                        connection.HasBeenPinged = true;
                        Task.Run(async () =>
                        {
                            await _handler.SendRawAsync("Ping", connection);
                        });
                    }
                }

                foreach (var connectionToRemove in connectionsToRemove)
                {
                    _connectionManager.RemoveConnectionAuthorized(connectionToRemove);

                    Task.Run(async () =>
                    {
                        await _handler.SendRawAsync("No ping response - disconnected.", connectionToRemove);
                        _handler.DisconnectClient(connectionToRemove.Client);
                    });
                }
            }
        }

        protected virtual async Task<bool> CheckIfAuthorizedAsync(TcpSSLMessageEventArgs args)
        {
            try
            {
                // Check for token here
                if (_connectionManager.IsClientUnauthorized(args.Client))
                {
                    var connectionUnauth = _connectionManager.GetClientUnauthorized(args.Client);
                    _connectionManager.RemoveClientUnauthorized(connectionUnauth, false);

                    if (args.Message.Length < "oauth:".Length ||
                        !args.Message.ToLower().StartsWith("oauth:"))
                    {
                        await _handler.SendRawAsync(_parameters.UnauthorizedString, connectionUnauth);
                        args.Client.Close();

                        FireEvent(this, new TcpSSLConnectionAuthEventArgs
                        {
                            ConnectionType = TcpSSLConnectionType.Disconnect,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpSSLConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection,
                            Client = args.Client,
                            Reader = connectionUnauth.Reader,
                            Writer = connectionUnauth.Writer
                        });
                        return false;
                    }

                    var token = args.Message.Substring("oauth:".Length);

                    var userId = await _userService.GetUserIdAsync(token);

                    if (userId == null ||
                        userId == Guid.Empty)
                    {
                        await _handler.SendRawAsync(_parameters.UnauthorizedString, connectionUnauth);
                        args.Client.Close();

                        FireEvent(this, new TcpSSLConnectionAuthEventArgs
                        {
                            Client = args.Client,
                            ConnectionType = TcpSSLConnectionType.Disconnect,
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            ConnectionAuthType = TcpSSLConnectionAuthType.Unauthorized,
                            ArgsType = ArgsType.Connection,
                            Reader = connectionUnauth.Reader,
                            Writer = connectionUnauth.Writer,
                        });
                        return false;
                    }

                    var identity = _connectionManager.AddConnectionAuthorized(userId, connectionUnauth.Client, connectionUnauth.Reader, connectionUnauth.Writer);

                    await _handler.SendRawAsync(_parameters.ConnectionSuccessString, identity.GetConnection(args.Client));

                    FireEvent(this, new TcpSSLConnectionAuthEventArgs
                    {
                        Client = args.Client,
                        Reader = connectionUnauth.Reader,
                        Writer = connectionUnauth.Writer,
                        ConnectionType = TcpSSLConnectionType.Connected,
                        ConnectionEventType = ConnectionEventType.Connected,
                        ConnectionAuthType = TcpSSLConnectionAuthType.Authorized,
                        ArgsType = ArgsType.Connection,
                        UserId = identity.UserId
                    });
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        public override void Dispose()
        {
            foreach (var item in _connectionManager.GetAllClientsUnauthorized())
            {
                _connectionManager.RemoveClientUnauthorized(item, true);
            }

            foreach (var item in _connectionManager.GetAllIdentitiesAuthorized())
            {
                foreach (var connection in item.Connections.ToList())
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

        public new ITcpSSLConnectionManagerAuth ConnectionManager
        {
            get
            {
                return _connectionManager;
            }
        }
    }
}
