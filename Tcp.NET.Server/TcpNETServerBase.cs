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
        where T : TcpConnectionServerBaseEventArgs<Z>
        where U : TcpMessageServerBaseEventArgs<Z>
        where V : TcpErrorServerBaseEventArgs<Z>
        where W : ParamsTcpServer
        where X : TcpHandlerServerBase<T, U, V, W, Z>
        where Y : TcpConnectionManager<Z>
        where Z : ConnectionTcpServer
    {
        protected readonly X _handler;
        protected readonly W _parameters;
        protected readonly Y _connectionManager;
        protected Timer _timerPing;
        protected bool _isPingRunning;
        protected CancellationToken _cancellationToken;
        
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
            _cancellationToken = cancellationToken;
            _handler.Start(cancellationToken);
        }
        public virtual void Stop()
        {
            _handler.Stop();
        }

        public virtual async Task<bool> BroadcastToAllConnectionsAsync(string message, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                foreach (var connection in _connectionManager.GetAll())
                {
                    await SendToConnectionAsync(message, connection, cancellationToken).ConfigureAwait(false);
                }

                return true;
            }

            return false;
        }
        public virtual async Task<bool> BroadcastToAllConnectionsAsync(byte[] message, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                foreach (var connection in _connectionManager.GetAll())
                {
                    await SendToConnectionAsync(message, connection, cancellationToken).ConfigureAwait(false);
                }

                return true;
            }

            return false;
        }
        public virtual async Task<bool> SendToConnectionAsync(string message, Z connection, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                return await _handler.SendAsync(message, connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public virtual async Task<bool> SendToConnectionAsync(byte[] message, Z connection, CancellationToken cancellationToken = default)
        {
            if (IsServerRunning)
            {
                return await _handler.SendAsync(message, connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public virtual async Task<bool> DisconnectConnectionAsync(Z connection, CancellationToken cancellationToken = default)
        {
            return await _handler.DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        }

        protected abstract void OnConnectionEvent(object sender, T args);
        protected virtual void OnServerEvent(object sender, ServerEventArgs args)
        {
            if (_timerPing != null)
            {
                _timerPing.Dispose();
                _timerPing = null;
            }

            switch (args.ServerEventType)
            {
                case ServerEventType.Start:
                    _timerPing = new Timer(OnTimerPingTick, null, _parameters.PingIntervalSec * 1000, _parameters.PingIntervalSec * 1000);
                    break;
                case ServerEventType.Stop:
                    break;
                default:
                    break;
            }

            FireEvent(sender, args);
        }
        protected abstract void OnMessageEvent(object sender, U args);
        protected abstract void OnErrorEvent(object sender, V args);
        
        protected virtual void OnTimerPingTick(object state)
        {
            if (!_isPingRunning)
            {
                _isPingRunning = true;

                Task.Run(async () =>
                {
                    foreach (var connection in _connectionManager.GetAll())
                    {
                        if (connection.HasBeenPinged)
                        {
                            // Already been pinged, no response, disconnect
                            await SendToConnectionAsync("No ping response - disconnected.", connection, _cancellationToken).ConfigureAwait(false);
                            await DisconnectConnectionAsync(connection, _cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            connection.HasBeenPinged = true;
                            await SendToConnectionAsync(_parameters.PingBytes, connection, _cancellationToken).ConfigureAwait(false);
                        }
                    }

                    _isPingRunning = false;
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
    }
}
