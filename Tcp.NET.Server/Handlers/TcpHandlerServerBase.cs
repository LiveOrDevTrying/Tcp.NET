﻿using PHS.Networking.Enums;
using PHS.Networking.Events.Args;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Server.Handlers;
using PHS.Networking.Utilities;
using System;
using System.Net.Security;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;
using System.Linq;

namespace Tcp.NET.Server.Handlers
{
    public abstract class TcpHandlerServerBase<T, U, V, W, Z> :
        HandlerServerBase<T, U, V, W, Z>
        where T : TcpConnectionServerBaseEventArgs<Z>
        where U : TcpMessageServerBaseEventArgs<Z>
        where V : TcpErrorServerBaseEventArgs<Z>
        where W : IParamsTcpServer
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

            _isRunning = false;
        }

        protected virtual async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                TcpClient client = null;

                try
                {
                    client = await _server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                    var connection = CreateConnection(new ConnectionTcpServer
                    {
                        TcpClient = client
                    });

                    if (_parameters.PingIntervalSec > 0)
                    {
                        connection.NextPing = DateTime.UtcNow.AddSeconds(_parameters.PingIntervalSec);
                    }

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

                    try
                    {
                        client?.GetStream().Close();
                    }
                    catch { }

                    try
                    {
                        client?.Dispose();
                    }
                    catch { }
                }
            }
        }
        protected virtual async Task ListenForConnectionsSSLAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                TcpClient client = null;

                try
                {
                    client = await _server.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
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
                            SslStream = sslStream,
                            ReadBuffer = new byte[4096]
                        });

                        if (_parameters.PingIntervalSec > 0)
                        {
                            connection.NextPing = DateTime.UtcNow.AddSeconds(_parameters.PingIntervalSec);
                        }

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

                        client?.GetStream().Close();
                        client?.Dispose();
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

                    try
                    {
                        client?.GetStream().Close();
                    }
                    catch { }

                    try
                    {
                        client?.Dispose();
                    }
                    catch { }
                }
            }
        }
        protected virtual async Task ReceiveAsync(Z connection, CancellationToken cancellationToken)
        {
            try
            {
                while (connection.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    do
                    {
                        try
                        {
                            if (connection.SslStream != null)
                            {
                                var bytesRead = 0;
                                if ((bytesRead = connection.SslStream.Read(connection.ReadBuffer, 0, connection.ReadBuffer.Length)) > 0)
                                {
                                    await connection.MemoryStream.WriteAsync(connection.ReadBuffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                                    connection.EndOfLine = Statics.ByteArrayContainsSequence(connection.MemoryStream.ToArray(), _parameters.EndOfLineBytes) > -1;
                                    connection.ReadBuffer = new byte[4096];
                                }
                                else
                                {
                                    await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                if (connection.TcpClient.Available > 0)
                                {
                                    var buffer = new ArraySegment<byte>(new byte[connection.TcpClient.Available]);
                                    var result = await connection.TcpClient.Client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                                    await connection.MemoryStream.WriteAsync(buffer.Array.AsMemory(buffer.Offset, result), cancellationToken).ConfigureAwait(false);

                                    connection.EndOfLine = Statics.ByteArrayContainsSequence(connection.MemoryStream.ToArray(), _parameters.EndOfLineBytes) > -1;
                                }
                                else
                                {
                                    await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                        catch { }
                    }
                    while (!connection.EndOfLine && connection.TcpClient.Connected);

                    if (connection.EndOfLine)
                    {
                        connection.EndOfLine = false;

                        var bytes = connection.MemoryStream.ToArray();
                        connection.MemoryStream.SetLength(0);

                        while (Statics.ByteArrayContainsSequence(bytes, _parameters.EndOfLineBytes) > -1)
                        {
                            var index = Statics.ByteArrayContainsSequence(bytes, _parameters.EndOfLineBytes);
                            var sub = bytes.Take(index).ToArray();

                            bytes = bytes.Skip(index + _parameters.EndOfLineBytes.Length).ToArray();

                            if (_parameters.UseDisconnectBytes && Statics.ByteArrayEquals(sub, _parameters.DisconnectBytes))
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
                            else if (_parameters.PingIntervalSec > 0 && Statics.ByteArrayEquals(sub, _parameters.PongBytes))
                            {
                                connection.HasBeenPinged = false;
                            }
                            else
                            {
                                FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                                {
                                    Connection = connection,
                                    Message = !_parameters.OnlyEmitBytes ? Encoding.UTF8.GetString(sub) : null,
                                    MessageEventType = MessageEventType.Receive,
                                    Bytes = sub,
                                    CancellationToken = cancellationToken
                                }));
                            }
                        }

                        await connection.MemoryStream.WriteAsync(bytes, cancellationToken);
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
                if (connection.TcpClient.Connected && 
                    connection.TcpClient.Client != null && 
                    !cancellationToken.IsCancellationRequested && 
                    _isRunning && 
                    !string.IsNullOrWhiteSpace(message))
                {
                    var bytes = Statics.ByteArrayAppend(Encoding.UTF8.GetBytes(message), _parameters.EndOfLineBytes);

                    if (connection.SslStream != null)
                    {
                        await connection.SslStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
                        await connection.SslStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);
                    }

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = message,
                        Bytes = bytes,
                        CancellationToken = cancellationToken
                    }));
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

            await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);

            return false;
        }
        public override async Task<bool> SendAsync(byte[] message, Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && 
                    connection.TcpClient.Client != null && 
                    !cancellationToken.IsCancellationRequested && 
                    _isRunning && 
                    message.Where(x => x != 0).Any())
                {
                    var bytes = Statics.ByteArrayAppend(message, _parameters.EndOfLineBytes);

                    if (connection.SslStream != null)
                    {
                        await connection.SslStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
                        await connection.SslStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);
                    }

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = null,
                        Bytes = bytes,
                        CancellationToken = cancellationToken
                    }));
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

            await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);

            return false;
        }
        public override async Task<bool> DisconnectConnectionAsync(Z connection, CancellationToken cancellationToken, string disconnectMessage = "")
        {
            try
            {
                if (connection != null)
                {
                    if (!connection.Disposed)
                    {
                        connection.Disposed = true;

                        var bytes = new byte[0];

                        if (!string.IsNullOrWhiteSpace(disconnectMessage))
                        {
                            bytes = Encoding.UTF8.GetBytes(disconnectMessage);

                            if (_parameters.UseDisconnectBytes)
                            {
                                bytes = bytes.Concat(_parameters.EndOfLineBytes).ToArray();
                            }
                        }

                        if (_parameters.UseDisconnectBytes)
                        {
                            bytes = bytes.Concat(_parameters.DisconnectBytes).ToArray();
                        }

                        if (bytes.Length > 0)
                        {
                            await SendAsync(bytes, connection, cancellationToken).ConfigureAwait(false);
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
                    Connection = connection,
                    Exception = ex,
                    Message = $"Error in DisconnectConnection - {ex.Message}",
                    CancellationToken = cancellationToken
                }));
            }

            return false;
        }

        protected abstract Z CreateConnection(ConnectionTcpServer connection);
        protected abstract T CreateConnectionEventArgs(ConnectionEventArgs<Z> args);
        protected abstract U CreateMessageEventArgs(TcpMessageServerBaseEventArgs<Z> args);
        protected abstract V CreateErrorEventArgs(ErrorEventArgs<Z> args);

        public TcpListener Server
        {
            get
            {
                return _server;
            }
        }
    }
}