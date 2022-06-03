using Tcp.NET.Server.Models;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Managers;
using PHS.Networking.Server.Events.Args;
using Tcp.NET.Server.Events.Args;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Services;
using PHS.Networking.Enums;
using System;

namespace Tcp.NET.Server
{
    public abstract class TcpNETServerBase<T, U, V, W, X, Y, Z> : 
        CoreNetworkingServer<T, U, V, W, X, Y, Z>, 
        ICoreNetworkingServer<T, U, V, Z>
        where T : TcpConnectionServerBaseEventArgs<Z>
        where U : TcpMessageServerBaseEventArgs<Z>
        where V : TcpErrorServerBaseEventArgs<Z>
        where W : ParamsTcpServer
        where X : TcpHandlerServerBase<T, U, V, W, Z>
        where Y : TcpConnectionManagerBase<Z>
        where Z : ConnectionTcpServer
    {
        protected Timer _timerPing;
        protected bool _isPingRunning;
        
        public TcpNETServerBase(W parameters) : base(parameters)
        {
        }
        public TcpNETServerBase(W parameters,
            byte[] certificate,
            string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override void OnServerEvent(object sender, ServerEventArgs args)
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

            base.FireEvent(sender, args);
        }

        protected virtual void OnTimerPingTick(object state)
        {
            if (!_isPingRunning)
            {
                _isPingRunning = true;

                Task.Run(async () =>
                {
                    foreach (var connection in _connectionManager.GetAll())
                    {
                        try
                        {
                            if (connection.HasBeenPinged)
                            {
                                // Already been pinged, no response, disconnect
                                if (!_parameters.OnlyEmitBytes)
                                {
                                    await SendToConnectionAsync("No ping response - disconnected.", connection, _cancellationToken).ConfigureAwait(false);
                                }
                                await DisconnectConnectionAsync(connection, _cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                connection.HasBeenPinged = true;
                                await SendToConnectionAsync(_parameters.PingBytes, connection, _cancellationToken).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                            {
                                Connection = connection,
                                Exception = ex,
                                Message = ex.Message
                            }));
                        }
                    }

                    _isPingRunning = false;
                });
            }
        }

        protected abstract T CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<Z> args);
        protected abstract U CreateMessageEventArgs(TcpMessageServerBaseEventArgs<Z> args);
        protected abstract V CreateErrorEventArgs(TcpErrorServerBaseEventArgs<Z> args);

        public override void Dispose()
        {
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
                return _handler?.Server;
            }
        }
    }
}
