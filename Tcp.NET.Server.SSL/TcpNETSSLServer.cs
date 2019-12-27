using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Core.SSL.Enums;
using Tcp.NET.Core.SSL.Events.Args;
using Tcp.NET.Server.SSL.Handlers;
using Tcp.NET.Server.SSL.Models;

namespace Tcp.NET.Server.SSL
{
    public class TcpNETSSLServer : 
        CoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs>, 
        ITcpNETSSLServer
    {
        protected readonly ITcpSSLConnectionManager _connectionManager;
        protected readonly IParamsTcpServerSSL _parameters;
        protected TcpHandlerSSL _handler;

        protected readonly X509Certificate _serverCertificate;
        protected readonly string _certificateIssuedTo;
        protected readonly StoreLocation _storeLocation;

        protected Timer _timerPing;

        public TcpNETSSLServer(IParamsTcpServerSSL parameters,
            ITcpSSLConnectionManager connectionManager,
            X509Certificate serverCertificate)
        {
            _parameters = parameters;
            _connectionManager = connectionManager;
            _serverCertificate = serverCertificate;

            _handler = new TcpHandlerSSL(_parameters.Port, _parameters.EndOfLineCharacters, serverCertificate);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
        }

        public TcpNETSSLServer(IParamsTcpServerSSL parameters,
            ITcpSSLConnectionManager connectionManager,
            string certificateIssuedTo,
            StoreLocation storeLocation)
        {
            _parameters = parameters;
            _connectionManager = connectionManager;
            _certificateIssuedTo = certificateIssuedTo;
            _storeLocation = storeLocation;

            _handler = new TcpHandlerSSL(_parameters.Port, _parameters.EndOfLineCharacters,certificateIssuedTo, storeLocation);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
        }

        public virtual async Task<bool> SendToClientAsync(PacketDTO packet, TcpClient client)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    client.Connected)
                {
                    var connection = ConnectionManager.GetConnection(client);
                    await _handler.SendAsync(packet, connection);

                    FireEvent(this, new TcpSSLMessageEventArgs
                    {
                        Message = JsonConvert.SerializeObject(packet),
                        MessageEventType = MessageEventType.Sent,
                        ArgsType = ArgsType.Message,
                        Packet = packet,
                        Client = client,
                    });

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual async Task<bool> SendToClientRawAsync(string message, TcpClient client)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    client.Connected)
                {
                    var connection = ConnectionManager.GetConnection(client);
                    await _handler.SendRawAsync(message, connection);

                    FireEvent(this, new TcpSSLMessageEventArgs
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
            }
            catch
            { }

            return false;
        }

        public virtual bool DisconnectClient(TcpClient client)
        {
            return _handler.DisconnectClient(client);
        }

        protected virtual Task OnConnectionEvent(object sender, TcpSSLConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (_connectionManager.AddConnection(new ConnectionTcpClientSSLDTO
                    {
                        Client = args.Client,
                        Reader = args.Reader,
                        Writer = args.Writer
                    }))
                    {
                        FireEvent(this, new TcpSSLConnectionEventArgs
                        {
                            ConnectionType = TcpSSLConnectionType.Connected,
                            ConnectionEventType = args.ConnectionEventType,
                            Client = args.Client,
                            ArgsType = ArgsType.Connection,
                        });
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.RemoveConnection(args.Client, true);

                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        Client = args.Client,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.Disconnect,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.ServerStart:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    _timerPing = new Timer(OnTimerPingTick, null, _parameters.PingIntervalSec * 1000, _parameters.PingIntervalSec * 1000);

                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        Client = args.Client,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.ServerStart,
                        ArgsType = ArgsType.Connection
                    });
                    break;
                case ConnectionEventType.ServerStop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        Client = args.Client,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.ServerStop,
                        ArgsType = ArgsType.Connection,
                    });

                    Thread.Sleep(5000);

                    _handler = _serverCertificate != null
                        ? new TcpHandlerSSL(_parameters.Port, _parameters.EndOfLineCharacters, _serverCertificate)
                        : new TcpHandlerSSL(_parameters.Port, _parameters.EndOfLineCharacters, _certificateIssuedTo, _storeLocation);
                    break;
                case ConnectionEventType.Connecting:
                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        Client = args.Client,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpSSLConnectionType.Connecting,
                        ArgsType = ArgsType.Connection
                    });
                    break;
                default:
                    break;
            }
            return Task.CompletedTask;
        }
        protected virtual Task OnMessageEventAsync(object sender, TcpSSLMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (_connectionManager.IsConnectionOpen(args.Client))
                    {
                        var connection = _connectionManager.GetConnection(args.Client);

                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            connection.HasBeenPinged = false;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(args.Message))
                            {

                                try
                                {
                                    var packet = JsonConvert.DeserializeObject<PacketDTO>(args.Message);

                                    FireEvent(this, new TcpSSLMessageEventArgs
                                    {
                                        Message = packet.Data,
                                        MessageEventType = MessageEventType.Receive,
                                        ArgsType = ArgsType.Message,
                                        Client = args.Client,
                                        Packet = packet
                                    });
                                }
                                catch
                                {
                                    FireEvent(this, new TcpSSLMessageEventArgs
                                    {
                                        Message = args.Message,
                                        MessageEventType = MessageEventType.Receive,
                                        ArgsType = ArgsType.Message,
                                        Client = args.Client,
                                        Packet = new PacketDTO
                                        {
                                            Action = (int)ActionType.SendToServer,
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        }
                                    });
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        protected virtual Task OnErrorEvent(object sender, TcpSSLErrorEventArgs args)
        {
            if (_connectionManager.IsConnectionOpen(args.Client))
            {
                FireEvent(this, new TcpSSLErrorEventArgs
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    Client = args.Client,
                    ArgsType = ArgsType.Error,
                });
            }
            return Task.CompletedTask;
        }
        protected virtual void OnTimerPingTick(object state)
        {
            foreach (var connection in _connectionManager.GetAllConnections())
            {
                var connectionsToRemove = new List<ConnectionTcpClientSSLDTO>();

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
                        await _handler.SendAsync("Ping", connection);
                    });
                }

                foreach (var connectionToRemove in connectionsToRemove)
                {
                    _connectionManager.RemoveConnection(connectionToRemove.Client, true);

                    Task.Run(async () =>
                    {
                        await _handler.SendRawAsync("No ping response - disconnected.", connectionToRemove);
                        _handler.DisconnectClient(connectionToRemove.Client);
                    });
                }
            }
        }

        protected virtual int GetRateLimit()
        {
            // 15000 messages each 30000 MS with 80% of total time to buffer
            return Convert.ToInt32(Math.Ceiling(30000f / 15000f * 0.8f));
        }
        public bool IsServerRunning
        {
            get
            {
                return _handler != null ? _handler.IsServerRunning : false;
            }
        }
        public TcpHandlerSSL TcpHandler
        {
            get
            {
                return _handler;
            }
        }
        public ITcpSSLConnectionManager ConnectionManager
        {
            get
            {
                return _connectionManager;
            }
        }

        public override void Dispose()
        {
            foreach (var item in _connectionManager.GetAllConnections())
            {
                _connectionManager.RemoveConnection(item.Client, true);
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
