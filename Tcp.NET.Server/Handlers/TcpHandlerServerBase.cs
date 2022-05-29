using PHS.Networking.Enums;
using PHS.Networking.Events;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using PHS.Networking.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public abstract class TcpHandlerServerBase<T, U, V, W, X> : 
        CoreNetworkingGeneric<T, U, V>, 
        ICoreNetworkingGeneric<T, U, V> 
        where T : TcpConnectionServerBaseEventArgs<X>
        where U : TcpMessageServerBaseEventArgs<X>
        where V : TcpErrorServerBaseEventArgs<X>
        where W : ParamsTcpServer
        where X : ConnectionTcpServer
    {
        protected readonly byte[] _certificate;
        protected readonly string _certificatePassword;
        protected readonly W _parameters;
        protected TcpListener _server;
        protected bool _isRunning;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpHandlerServerBase(W parameters)
        {
            _parameters = parameters;
        }
        public TcpHandlerServerBase(W parameters, byte[] certificate, string certificatePassword)
        {
            _parameters = parameters;
            _certificate = certificate;
            _certificatePassword = certificatePassword;
        }

        public virtual void Start(CancellationToken cancellationToken = default)
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
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                {
                    Exception = ex,
                    Message = ex.Message,
                }));
            }
        }
        public virtual void Stop()
        {
            _isRunning = false;

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

        public virtual async Task<bool> SendAsync(string message, X connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && _isRunning)
                {
                    var bytes = Statics.ByteArrayAppend(Encoding.UTF8.GetBytes(message), _parameters.EndOfLineBytes);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<X>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = message,
                        Bytes = bytes
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));

                await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public virtual async Task<bool> SendAsync(byte[] message, X connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && _isRunning)
                {
                    var bytes = Statics.ByteArrayAppend(message, _parameters.EndOfLineBytes);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<X>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = null,
                        Bytes = bytes
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));

                await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public virtual Task<bool> DisconnectConnectionAsync(X connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.TcpClient != null &&
                        !connection.Disposed)
                    {
                        connection.Disposed = true;
                        connection.TcpClient.Close();
                        connection.TcpClient.Dispose();

                        FireEvent(this, CreateConnectionEventArgs(new TcpConnectionServerBaseEventArgs<X>
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = connection
                        }));
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));
            }

            return Task.FromResult(false);
        }

        protected virtual async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                    var connection = CreateConnection(new ConnectionTcpServer
                    {
                        TcpClient = client,
                        ConnectionId = Guid.NewGuid().ToString()
                    });

                    FireEvent(this, CreateConnectionEventArgs(new TcpConnectionServerBaseEventArgs<X>
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        Connection = connection,
                    }));

                    _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
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
                    var client = await _server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                    var sslStream = new SslStream(client.GetStream());
                    await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificate = new X509Certificate2(_certificate, _certificatePassword)
                    }, cancellationToken).ConfigureAwait(false);

                    if (sslStream.IsAuthenticated && sslStream.IsEncrypted)
                    {
                        var connection = CreateConnection(new ConnectionTcpServer
                        {
                            TcpClient = client,
                            ConnectionId = Guid.NewGuid().ToString()
                        });

                        FireEvent(this, CreateConnectionEventArgs(new TcpConnectionServerBaseEventArgs<X>
                        {
                            ConnectionEventType = ConnectionEventType.Connected,
                            Connection = connection,
                        }));

                        _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var certStatus = $"IsAuthenticated = {sslStream.IsAuthenticated} && IsEncrypted == {sslStream.IsEncrypted}";
                        FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                        {
                            Exception = new Exception(certStatus),
                            Message = certStatus
                        }));

                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                    {
                        Exception = ex,
                        Message = ex.Message,
                    }));
                }
            }
        }
        protected virtual async Task ReceiveAsync(X connection, CancellationToken cancellationToken) 
        {
            try
            {
                using (var persistantMS = new MemoryStream())
                {
                    while (connection.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                    {
                        using (var ms = new MemoryStream())
                        {
                            if (persistantMS.Length > 0)
                            {
                                persistantMS.CopyTo(ms);
                                persistantMS.SetLength(0);
                            }

                            var endOfMessage = false;
                            do
                            {
                                if (connection.TcpClient.Available <= 0)
                                {
                                    await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                                    continue;
                                }

                                var buffer = new ArraySegment<byte>(new byte[connection.TcpClient.Available]);
                                var result = await connection.TcpClient.Client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                                await ms.WriteAsync(buffer.Array, buffer.Offset, result, cancellationToken).ConfigureAwait(false);

                                endOfMessage = Statics.ByteArrayContainsSequence(ms.ToArray(), _parameters.EndOfLineBytes);
                            }
                            while (!endOfMessage && connection.TcpClient.Connected);

                            if (endOfMessage)
                            {
                                var parts = Statics.ByteArraySeparate(ms.ToArray(), _parameters.EndOfLineBytes);

                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (parts.Length > 1 && i == parts.Length - 1)
                                    {
                                        await persistantMS.WriteAsync(parts[i], cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        if (Statics.ByteArrayEquals(parts[i], _parameters.PongBytes))
                                        {
                                            connection.HasBeenPinged = false;
                                        }
                                        else
                                        {
                                            string message = null;

                                            if (!_parameters.OnlyEmitBytes)
                                            {
                                                message = Encoding.UTF8.GetString(parts[i]);
                                            }

                                            FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<X>
                                            {
                                                Connection = connection,
                                                Message = message,
                                                MessageEventType = MessageEventType.Receive,
                                                Bytes = parts[i]
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<X>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));
            }

            await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        }

        protected abstract X CreateConnection(ConnectionTcpServer connection);
        protected abstract T CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<X> args);
        protected abstract U CreateMessageEventArgs(TcpMessageServerBaseEventArgs<X> args);
        protected abstract V CreateErrorEventArgs(TcpErrorServerBaseEventArgs<X> args);
        
        protected virtual void FireEvent(object sender, ServerEventArgs args)
        {
            _serverEvent?.Invoke(sender, args);
        }

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
        public bool IsServerRunning
        {
            get
            {
                return _isRunning;
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