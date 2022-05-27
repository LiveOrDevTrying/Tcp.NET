using Tcp.NET.Server.Models;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using PHS.Networking.Events;
using PHS.Networking.Server.Enums;
using PHS.Networking.Enums;
using Tcp.NET.Server.Events.Args;
using System.Collections.Generic;
using Tcp.NET.Core.Events.Args;

namespace Tcp.NET.Server
{
    public abstract class TcpNETServerBase<T, U, V, W, X, Y, Z> : 
        CoreNetworkingGeneric<T, U, V>, 
        ITcpNETServerBase<T, U, V, Z>
        where T : TcpConnectionEventArgs<Z>
        where U : TcpMessageEventArgs<Z>
        where V : TcpErrorEventArgs<Z>
        where W : ParamsTcpServer
        where X : TcpHandlerBase<Z>
        where Y : TcpConnectionManager<Z>
        where Z : ConnectionTcpServer
    {
        protected readonly X _handler;
        protected readonly W _parameters;
        protected readonly Y _connectionManager;
        protected Timer _timerPing;
        protected bool _isPingRunning;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpNETServerBase(W parameters)
        {
            _parameters = parameters;
            _connectionManager = CreateTcpConnectionManager();

            _handler = CreateTcpHandler();
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEvent;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
        }
        public TcpNETServerBase(W parameters,
            byte[] certificate,
            string certificatePassword)
        {
            _parameters = parameters;
            _connectionManager = CreateTcpConnectionManager();

            _handler = CreateTcpHandler(certificate, certificatePassword);
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEvent;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.ServerEvent += OnServerEvent;
        }
        public virtual void Start(CancellationToken cancellationToken = default)
        {
            _handler.Start(cancellationToken);
        }
        public virtual void Stop()
        {
            _handler.Stop();
        }

        public virtual async Task<bool> BroadcastToAllConnectionsAsync(string message, Z connectionSending = null, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                foreach (var connection in _connectionManager.GetAll())
                {
                    if (connectionSending == null || connection.ConnectionId != connectionSending.ConnectionId)
                    {
                        await SendToConnectionAsync(message, connection, cancellationToken).ConfigureAwait(false);
                    }
                }

                return true;
            }

            return false;
        }
        public virtual async Task<bool> SendToConnectionAsync(string message, Z connection, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                return await _handler.SendAsync(message, connection, cancellationToken);
            }

            return false;
        }
        public virtual async Task<bool> DisconnectConnectionAsync(Z connection, CancellationToken cancellationToken = default)
        {
            return await _handler.DisconnectConnectionAsync(connection, cancellationToken);
        }

        protected abstract void OnConnectionEvent(object sender, TcpConnectionServerBaseEventArgs<Z> args);
        protected virtual void OnServerEvent(object sender, ServerEventArgs args)
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

                    _timerPing = new Timer(OnTimerPingTick, null, _parameters.PingIntervalSec * 1000, _parameters.PingIntervalSec * 1000);
                    break;
                case ServerEventType.Stop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(sender, args);
                    break;
                default:
                    break;
            }
        }
        protected abstract void OnMessageEvent(object sender, TcpMessageServerBaseEventArgs<Z> args);
        protected abstract void OnErrorEvent(object sender, TcpErrorServerBaseEventArgs<Z> args);
        
        protected virtual void OnTimerPingTick(object state)
        {
            if (!_isPingRunning)
            {
                _isPingRunning = true;

                Task.Run(async () =>
                {
                    Console.WriteLine("Ping");
                    foreach (var connection in _connectionManager.GetAll())
                    {
                        if (connection.HasBeenPinged)
                        {
                            // Already been pinged, no response, disconnect
                            await SendToConnectionAsync("No ping response - disconnected.", connection);
                            await DisconnectConnectionAsync(connection);
                        }
                        else
                        {
                            connection.HasBeenPinged = true;
                            await SendToConnectionAsync("ping", connection);
                        }
                    }

                    _isPingRunning = false;
                    Console.WriteLine("Pinged");
                });
            }
        }

        protected abstract X CreateTcpHandler(byte[] certificate = null, string certificatePassword = null);
        protected abstract Y CreateTcpConnectionManager();

        protected virtual void FireEvent(object sender, ServerEventArgs args)
        {
            _serverEvent?.Invoke(sender, args);
        }

        public override void Dispose()
        {
            foreach (var connection in _connectionManager.GetAll())
            {
                DisconnectConnectionAsync(connection).Wait();
            }

            if (_handler != null)
            {
                _handler.ConnectionEvent -= OnConnectionEvent;
                _handler.MessageEvent -= OnMessageEvent;
                _handler.ErrorEvent -= OnErrorEvent;
                _handler.ServerEvent -= OnServerEvent;
                _handler.Dispose();
            }

            if (_timerPing != null)
            {
                _timerPing.Dispose();
                _timerPing = null;
            }
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
        public IEnumerable<Z> Connections
        {
            get
            {
                return _connectionManager.GetAll();
            }
        }
        public int ConnectionCount
        {
            get
            {
                return _connectionManager.Count();
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
