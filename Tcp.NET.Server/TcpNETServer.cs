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

        protected Timer _timerPing;
        protected volatile bool _isPingRunning;

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
        }

        public virtual async Task StartAsync()
        {
            await _handler.StartAsync();
        }
        public virtual async Task StopAsync()
        {
            await _handler.StopAsync();
        }

        public virtual async Task<bool> SendToConnectionAsync<S>(S packet, IConnectionTcpServer connection) where S : IPacket
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsConnectionOpen(connection))
                {
                    if (!await _handler.SendAsync(packet, connection))
                    {
                        return false;
                    }

                    await FireEventAsync(this, new TcpMessageServerEventArgs
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
                await FireEventAsync(this, new TcpErrorServerEventArgs
                {
                    Connection = connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            return false;
        }
        public virtual async Task<bool> SendToConnectionAsync(string message, IConnectionTcpServer connection)
        {
            return await SendToConnectionAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            }, connection);
        }
        public virtual async Task<bool> SendToConnectionRawAsync(string message, IConnectionTcpServer connection)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    _connectionManager.IsConnectionOpen(connection))
                {
                    if (!await _handler.SendRawAsync(message, connection))
                    {
                        return false;
                    }

                    await FireEventAsync(this, new TcpMessageServerEventArgs
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
                await FireEventAsync(this, new TcpErrorServerEventArgs
                {
                    Connection = connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            return false;
        }

        public virtual async Task<bool> DisconnectConnectionAsync(IConnectionTcpServer connection)
        {
            return await _handler.DisconnectConnectionAsync(connection);
        }

        protected virtual async Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args)
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

            await FireEventAsync(this, args);
        }
        protected virtual async Task OnServerEvent(object sender, ServerEventArgs args)
        {
            switch (args.ServerEventType)
            {
                case ServerEventType.Start:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    await FireEventAsync(sender, args);

                    _timerPing = new Timer(OnTimerPingTick, null, PING_INTERVAL_SEC * 1000, PING_INTERVAL_SEC * 1000);
                    break;
                case ServerEventType.Stop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    await StopAsync();

                    await FireEventAsync(sender, args);

                    Thread.Sleep(5000);
                    await StartAsync();
                    break;
                default:
                    break;
            }
        }
        protected virtual async Task OnMessageEventAsync(object sender, TcpMessageServerEventArgs args)
        {
            await FireEventAsync(sender, args);
        }
        protected virtual async Task OnErrorEvent(object sender, TcpErrorServerEventArgs args)
        {
            await FireEventAsync(this, args);
        }
        
        protected virtual void OnTimerPingTick(object state)
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
                                await DisconnectConnectionAsync(connection);
                            }
                            else
                            {
                                connection.HasBeenPinged = true;
                                await SendToConnectionRawAsync("Ping", connection);
                            }
                        }
                        catch (Exception ex)
                        {
                            await FireEventAsync(this, new TcpErrorServerEventArgs
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

        protected virtual async Task FireEventAsync(object sender, ServerEventArgs args)
        {
            if (_serverEvent != null)
            {
                await _serverEvent?.Invoke(sender, args);
            }
        }

        public override void Dispose()
        {
            foreach (var connection in _connectionManager.GetAllConnections())
            {
                DisconnectConnectionAsync(connection).Wait();
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
        public IConnectionTcpServer[] Connections
        {
            get
            {
                return _connectionManager.GetAllConnections();
            }
        }
        public TcpConnectionManager ConnectionManager
        {
            get
            {
                return _connectionManager;
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
