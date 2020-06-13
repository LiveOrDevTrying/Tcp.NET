using Newtonsoft.Json;
using Tcp.NET.Server.Models;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using System.Security.Cryptography.X509Certificates;
using PHS.Networking.Models;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using PHS.Networking.Events;
using PHS.Networking.Server.Enums;
using PHS.Networking.Enums;
using Tcp.NET.Server.Events.Args;

namespace Tcp.NET.Server
{
    public class TcpNETServer : 
        CoreNetworking<TcpConnectionServerEventArgs, TcpMessageServerEventArgs, TcpErrorServerEventArgs>, 
        ITcpNETServer
    {
        private readonly TcpHandler _handler;
        private readonly IParamsTcpServer _parameters;
        private readonly TcpConnectionManager _connectionManager;

        private const int PING_INTERVAL_SEC = 120;

        private Timer _timerPing;
        private volatile bool _isPingRunning;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpNETServer(IParamsTcpServer parameters, TcpHandler handler = null)
        {
            _parameters = parameters;
            _connectionManager = new TcpConnectionManager();

            _handler = handler ?? new TcpHandler(_parameters);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
            _handler.Start();
        }
        public TcpNETServer(IParamsTcpServer parameters,
            byte[] certificate,
            string certificatePassword,
            TcpHandler handler = null)
        {
            _parameters = parameters;
            _connectionManager = new TcpConnectionManager();

            _handler = handler ?? new TcpHandler(_parameters, certificate, certificatePassword);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
            _handler.Start();
        }

        public virtual async Task<bool> SendToConnectionAsync<S>(S packet, IConnectionServer connection) where S : IPacket
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsConnectionOpen(connection))
                {
                    await _handler.SendAsync(packet, connection);

                    FireEvent(this, new TcpMessageServerEventArgs
                    {
                        Message = JsonConvert.SerializeObject(packet),
                        MessageEventType = MessageEventType.Sent,
                        Packet = packet,
                        Connection = connection,
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerEventArgs
                {
                    Connection = connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            return false;
        }
        public virtual async Task<bool> SendToConnectionAsync(string message, IConnectionServer connection)
        {
            return await SendToConnectionAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            }, connection);
        }
        public virtual async Task<bool> SendToConnectionRawAsync(string message, IConnectionServer connection)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsConnectionOpen(connection))
                {
                    await _handler.SendRawAsync(message, connection);

                    FireEvent(this, new TcpMessageServerEventArgs
                    {
                        Message = message,
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Packet = new Packet
                        {
                            Data = message,
                            Timestamp = DateTime.UtcNow
                        },
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerEventArgs
                {
                    Connection = connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            return false;
        }

        public virtual bool DisconnectConnection(IConnectionServer connection)
        {
            return _handler.DisconnectConnection(connection);
        }

        private Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionManager.AddConnection(args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.RemoveConnection(args.Connection);
                    break;
                case ConnectionEventType.Connecting:
                    break;
                default:
                    break;
            }

            FireEvent(this, args);

            return Task.CompletedTask;
        }
        private Task OnServerEvent(object sender, ServerEventArgs args)
        {
            switch (args.ServerEventType)
            {
                case ServerEventType.Start:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(sender, args);

                    _timerPing = new Timer(OnTimerPingTick, null, PING_INTERVAL_SEC * 1000, PING_INTERVAL_SEC * 1000);
                    break;
                case ServerEventType.Stop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(sender, args);

                    Thread.Sleep(5000);
                    _handler.Start();
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        private Task OnMessageEventAsync(object sender, TcpMessageServerEventArgs args)
        {
            FireEvent(sender, args);
            return Task.CompletedTask;
        }
        private Task OnErrorEvent(object sender, TcpErrorServerEventArgs args)
        {
            FireEvent(this, args);
            return Task.CompletedTask;
        }
        private void OnTimerPingTick(object state)
        {
            if (!_isPingRunning)
            {
                _isPingRunning = true;

                Task.Run(async () =>
                {
                    foreach (var connection in _connectionManager.GetAllConnections())
                    {
                        try
                        {
                            if (connection.HasBeenPinged)
                            {
                                // Already been pinged, no response, disconnect
                                await SendToConnectionRawAsync("No ping response - disconnected.", connection);
                                DisconnectConnection(connection);
                            }
                            else
                            {
                                connection.HasBeenPinged = true;
                                await SendToConnectionRawAsync("Ping", connection);
                            }
                        }
                        catch (Exception ex)
                        {
                            FireEvent(this, new TcpErrorServerEventArgs
                            {
                                Connection = connection,
                                Exception = ex,
                                Message = ex.Message,
                            });
                        }
                    }

                    _isPingRunning = false;
                });
            }
        }

        private void FireEvent(object sender, ServerEventArgs args)
        {
            _serverEvent?.Invoke(sender, args);
        }

        public override void Dispose()
        {
            foreach (var connection in _connectionManager.GetAllConnections())
            {
                DisconnectConnection(connection);
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
