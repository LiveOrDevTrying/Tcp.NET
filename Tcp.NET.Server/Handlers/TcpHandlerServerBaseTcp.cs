using PHS.Networking.Enums;
using PHS.Networking.Events;
using PHS.Networking.Events.Args;
using PHS.Networking.Models;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Core.Models;

namespace PHS.Networking.Server.Handlers
{
    public abstract class TcpHandlerServerBaseTcp<T, U, V, W, Z> :
        HandlerServerBase<T, U, V, W, Z>
        where T : ConnectionEventArgs<Z>
        where U : MessageEventArgs<Z>
        where V : ErrorEventArgs<Z>
        where W : ParamsPort
        where Z : IConnection
    {
        protected TcpListener _server;

        public TcpHandlerServerBaseTcp(W parameters) : base(parameters)
        {
        }
        public TcpHandlerServerBaseTcp(W parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        public override void Start(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_server != null)
                {
                    Stop();
                }

                _isRunning = true;

                _server = new TcpListener(IPAddress.Any, _parameters.Port);
                _server.Server.ReceiveTimeout = 60000;
                _server.Start();

                FireEvent(this, new ServerEventArgs
                {
                    ServerEventType = ServerEventType.Start
                });

                if (_certificate == null)
                {
                    _ = Task.Run(async () => { await ListenForConnectionsAsync(cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _ = Task.Run(async () => { await ListenForConnectionsSSLAsync(cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                }
                return;
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                }));
            }
        }
        public override void Stop()
        {
            _isRunning = false;

            try
            {
                if (_server != null)
                {
                    _server.Stop();
                    _server = null;
                }

                FireEvent(this, new ServerEventArgs
                {
                    ServerEventType = ServerEventType.Stop
                });
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                }));
            }
        }

        protected virtual async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync().ConfigureAwait(false);

                    var connection = CreateConnection(new ConnectionTcp
                    {
                        TcpClient = client
                    });

                    FireEvent(this, CreateConnectionEventArgs(new ConnectionEventArgs<Z>
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        Connection = connection,
                    }));

                    _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                    {
                        Exception = ex,
                        Message = ex.Message,
                    }));
                }
            }
        }
        protected virtual async Task ListenForConnectionsSSLAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync().ConfigureAwait(false);
                    var sslStream = new SslStream(client.GetStream());
                    await sslStream.AuthenticateAsServerAsync(new X509Certificate2(_certificate, _certificatePassword)).ConfigureAwait(false);

                    if (sslStream.IsAuthenticated && sslStream.IsEncrypted)
                    {
                        var connection = CreateConnection(new ConnectionTcp
                        {
                            TcpClient = client
                        });

                        FireEvent(this, CreateConnectionEventArgs(new ConnectionEventArgs<Z>
                        {
                            ConnectionEventType = ConnectionEventType.Connected,
                            Connection = connection,
                        }));

                        _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var certStatus = $"IsAuthenticated = {sslStream.IsAuthenticated} && IsEncrypted == {sslStream.IsEncrypted}";
                        FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                        {
                            Exception = new Exception(certStatus),
                            Message = certStatus
                        }));

                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                    {
                        Exception = ex,
                        Message = ex.Message,
                    }));
                }
            }
        }
        protected abstract Task ReceiveAsync(Z connection, CancellationToken cancellationToken);

        protected abstract Z CreateConnection(ConnectionTcp connection);
        protected abstract T CreateConnectionEventArgs(ConnectionEventArgs<Z> args);
        protected abstract V CreateErrorEventArgs(ErrorEventArgs<Z> args);

        public override void Dispose()
        {
            Stop();
        }

        public TcpListener Server
        {
            get
            {
                return _server;
            }
        }
    }
}
