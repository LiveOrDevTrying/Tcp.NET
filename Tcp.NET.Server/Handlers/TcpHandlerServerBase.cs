using PHS.Networking.Enums;
using PHS.Networking.Events.Args;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Server.Handlers;
using PHS.Networking.Utilities;
using System;
using System.IO;
using System.Net.Security;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Core.Models;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public abstract class TcpHandlerServerBase<T, U, V, W, Z> :
        HandlerServerBase<T, U, V, W, Z>
        where T : TcpConnectionServerBaseEventArgs<Z>
        where U : TcpMessageServerBaseEventArgs<Z>
        where V : TcpErrorServerBaseEventArgs<Z>
        where W : ParamsTcpServer
        where Z : ConnectionTcpServer
    {
        protected TcpListener _server;

        protected byte[] _certificate;
        protected string _certificatePassword;

        public TcpHandlerServerBase(W parameters) : base(parameters)
        {
        }
        public TcpHandlerServerBase(W parameters, byte[] certificate, string certificatePassword) : base(parameters)
        {
            _certificate = certificate;
            _certificatePassword = certificatePassword;
        }

        public override void Start(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_server != null)
                {
                    Stop(cancellationToken);
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
                    CancellationToken = cancellationToken
                }));
            }
        }
        public override void Stop(CancellationToken cancellationToken = default)
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
                    ServerEventType = ServerEventType.Stop,
                    CancellationToken = cancellationToken
                });
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    CancellationToken = cancellationToken
                }));
            }
        }

        protected virtual async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                    var connection = CreateConnection(new ConnectionTcp
                    {
                        TcpClient = client
                    });

                    FireEvent(this, CreateConnectionEventArgs(new ConnectionEventArgs<Z>
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        Connection = connection,
                        CancellationToken = cancellationToken
                    }));

                    _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                    {
                        Exception = ex,
                        Message = ex.Message,
                        CancellationToken = cancellationToken
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
                        var connection = CreateConnection(new ConnectionTcp
                        {
                            TcpClient = client
                        });

                        FireEvent(this, CreateConnectionEventArgs(new ConnectionEventArgs<Z>
                        {
                            ConnectionEventType = ConnectionEventType.Connected,
                            Connection = connection,
                            CancellationToken = cancellationToken
                        }));

                        _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var certStatus = $"IsAuthenticated = {sslStream.IsAuthenticated} && IsEncrypted == {sslStream.IsEncrypted}";
                        FireEvent(this, CreateErrorEventArgs(new ErrorEventArgs<Z>
                        {
                            Exception = new Exception(certStatus),
                            Message = certStatus,
                            CancellationToken = cancellationToken
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
                        CancellationToken = cancellationToken
                    }));
                }
            }
        }
        protected virtual async Task ReceiveAsync(Z connection, CancellationToken cancellationToken)
        {
            try
            {
                while (connection.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    using (var ms = new MemoryStream())
                    {
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
                            await ms.WriteAsync(buffer.Array.AsMemory(buffer.Offset, result), cancellationToken).ConfigureAwait(false);

                            endOfMessage = Statics.ByteArrayContainsSequence(ms.ToArray(), _parameters.EndOfLineBytes);
                        }
                        while (!endOfMessage && connection.TcpClient.Connected);

                        if (endOfMessage)
                        {
                            var parts = Statics.ByteArraySeparate(ms.ToArray(), _parameters.EndOfLineBytes);

                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (_parameters.UseDisconnectBytes && Statics.ByteArrayEquals(parts[i], _parameters.DisconnectBytes))
                                {
                                    connection?.Dispose();

                                    FireEvent(this, CreateConnectionEventArgs(new TcpConnectionServerBaseEventArgs<Z>
                                    {
                                        ConnectionEventType = ConnectionEventType.Disconnect,
                                        Connection = connection,
                                        CancellationToken = cancellationToken
                                    }));

                                    return;
                                }
                                else if (_parameters.PingIntervalSec > 0 && Statics.ByteArrayEquals(parts[i], _parameters.PongBytes))
                                {
                                    connection.HasBeenPinged = false;
                                }
                                else
                                {
                                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                                    {
                                        Connection = connection,
                                        Message = !_parameters.OnlyEmitBytes ? Encoding.UTF8.GetString(parts[i]) : null,
                                        MessageEventType = MessageEventType.Receive,
                                        Bytes = parts[i],
                                        CancellationToken = cancellationToken
                                    }));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection,
                    CancellationToken = cancellationToken
                }));
            }

            await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<bool> SendAsync(string message, Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && _isRunning)
                {
                    var bytes = Statics.ByteArrayAppend(Encoding.UTF8.GetBytes(message), _parameters.EndOfLineBytes);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = message,
                        Bytes = bytes,
                        CancellationToken = cancellationToken
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection,
                    CancellationToken = cancellationToken
                }));

                await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public override async Task<bool> SendAsync(byte[] message, Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && _isRunning)
                {
                    var bytes = Statics.ByteArrayAppend(message, _parameters.EndOfLineBytes);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = null,
                        Bytes = bytes,
                        CancellationToken = cancellationToken
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection,
                    CancellationToken = cancellationToken
                }));

                await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public override async Task<bool> DisconnectConnectionAsync(Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.TcpClient != null &&
                        !connection.Disposed)
                    {
                        if (_parameters.UseDisconnectBytes)
                        {
                            await SendAsync(_parameters.DisconnectBytes, connection, cancellationToken);
                        }

                        connection?.Dispose();

                        FireEvent(this, CreateConnectionEventArgs(new TcpConnectionServerBaseEventArgs<Z>
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = connection,
                            CancellationToken = cancellationToken
                        }));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection,
                    CancellationToken = cancellationToken
                }));
            }

            return false;
        }


        protected abstract Z CreateConnection(ConnectionTcp connection);
        protected abstract T CreateConnectionEventArgs(ConnectionEventArgs<Z> args);
        protected abstract U CreateMessageEventArgs(TcpMessageServerBaseEventArgs<Z> args);
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