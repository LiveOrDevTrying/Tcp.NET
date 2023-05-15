﻿using PHS.Networking.Enums;
using PHS.Networking.Handlers;
using PHS.Networking.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client.Handlers
{
    public abstract class TcpClientHandlerBase<T, U, V, W, Y> : 
        HandlerClientBase<T, U, V, W, Y>
        where T : TcpConnectionEventArgs<Y>
        where U : TcpMessageEventArgs<Y>
        where V : TcpErrorEventArgs<Y>
        where W : ParamsTcpClient
        where Y : ConnectionTcp
    {
        protected bool _isRunning;

        public TcpClientHandlerBase(W parameters) : base(parameters)
        {
            _isRunning = true;
        }
        
        public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (_connection != null)
                    {
                        await DisconnectAsync(cancellationToken).ConfigureAwait(false);
                    }

                    _isRunning = true;

                    if (_parameters.IsSSL)
                    {
                        await CreateSSLConnectionAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await CreateNonSSLConnectionAsync(cancellationToken).ConfigureAwait(false);
                    }
                    
                    if (_connection != null && _connection.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                    {
                        FireEvent(this, CreateConnectionEventArgs(new TcpConnectionEventArgs<Y>
                        {
                            Connection = _connection,
                            ConnectionEventType = ConnectionEventType.Connected,
                            CancellationToken = cancellationToken
                        }));

                        _ = Task.Run(async () => { await ReceiveAsync(cancellationToken).ConfigureAwait(false); }, cancellationToken).ConfigureAwait(false);

                        if (_parameters.Token != null && !cancellationToken.IsCancellationRequested)
                        {
                            await SendAsync(_parameters.Token, cancellationToken).ConfigureAwait(false);
                        }

                        return true;
                    };
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorEventArgs<Y>
                {
                    Exception = ex,
                    Connection = _connection,
                    Message = ex.Message,
                    CancellationToken = cancellationToken
                }));
            }

            await DisconnectAsync(cancellationToken).ConfigureAwait(false);

            return false;
        }
        public override async Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_connection != null)
                {
                    if (!_connection.Disposed)
                    {
                        _connection.Disposed = true;

                        if (_parameters.UseDisconnectBytes)
                        {
                            await SendAsync(_parameters.DisconnectBytes, cancellationToken).ConfigureAwait(false);
                        }

                        _connection?.Dispose();

                        FireEvent(this, CreateConnectionEventArgs(new TcpConnectionEventArgs<Y>
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = _connection,
                            CancellationToken = cancellationToken
                        }));

                        _connection = null;

                        _isRunning = false;

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorEventArgs<Y>
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message,
                    CancellationToken = cancellationToken
                }));
            }

            _connection?.Dispose();
            _connection = null;

            _isRunning = false;

            return false;
        }

        public override async Task<bool> SendAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_connection != null &&
                    _connection.TcpClient != null &&
                    _connection.TcpClient.Connected &&
                    !cancellationToken.IsCancellationRequested)
                {
                    var bytes = Statics.ByteArrayAppend(Encoding.UTF8.GetBytes($"{message}"), _parameters.EndOfLineBytes);
                    await _connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageEventArgs<Y>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Message = message,
                        Bytes = bytes,
                        CancellationToken = cancellationToken
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorEventArgs<Y>
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message,
                    CancellationToken = cancellationToken
                }));
            }

            await DisconnectAsync(cancellationToken).ConfigureAwait(false);

            return false;
        }
        public override async Task<bool> SendAsync(byte[] message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_connection != null &&
                    _connection.TcpClient != null &&
                    _connection.TcpClient.Connected &&
                    !cancellationToken.IsCancellationRequested)
                {
                    var bytes = Statics.ByteArrayAppend(message, _parameters.EndOfLineBytes);
                    await _connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageEventArgs<Y>
                    { 
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Message = null,
                        Bytes = bytes,
                        CancellationToken = cancellationToken
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorEventArgs<Y>
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message,
                    CancellationToken = cancellationToken
                }));
            }

            await DisconnectAsync(cancellationToken).ConfigureAwait(false);

            return false;
        }

        protected virtual async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _connection != null && _connection.TcpClient.Connected)
                {
                    using (var ms = new MemoryStream())
                    {
                        if (_connection.TcpClient.Available > 0)
                        {
                            var buffer = new ArraySegment<byte>(new byte[_connection.TcpClient.Available]);
                            var result = await _connection.TcpClient.Client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                            await ms.WriteAsync(buffer.Array.AsMemory(buffer.Offset, result), cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                        
                        var endOfMessage = Statics.ByteArrayContainsSequence(ms.ToArray(), _parameters.EndOfLineBytes) > -1;

                        if (endOfMessage)
                        {
                            var parts = Statics.ByteArraySeparate(ms.ToArray(), _parameters.EndOfLineBytes);

                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (_parameters.UseDisconnectBytes && Statics.ByteArrayEquals(parts[i], _parameters.DisconnectBytes))
                                {
                                    _connection?.Dispose();

                                    FireEvent(this, CreateConnectionEventArgs(new TcpConnectionEventArgs<Y>
                                    {
                                        ConnectionEventType = ConnectionEventType.Disconnect,
                                        Connection = _connection,
                                        CancellationToken = cancellationToken
                                    }));

                                    _connection = null;

                                    _isRunning = false;
                                    return;
                                }
                                else if (_parameters.UsePingPong && Statics.ByteArrayEquals(parts[i], _parameters.PingBytes))
                                {
                                    await SendAsync(_parameters.PongBytes, cancellationToken).ConfigureAwait(false);
                                }
                                else
                                {
                                    FireEvent(this, CreateMessageEventArgs(new TcpMessageEventArgs<Y>
                                    {
                                        MessageEventType = MessageEventType.Receive,
                                        Connection = _connection,
                                        Message = !_parameters.OnlyEmitBytes ? Encoding.UTF8.GetString(parts[i]) : null,
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
                FireEvent(this, CreateErrorEventArgs(new TcpErrorEventArgs<Y>
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message,
                    CancellationToken = cancellationToken
                }));
            }

            await DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task CreateNonSSLConnectionAsync(CancellationToken cancellationToken)
        {
            // Establish the remote endpoint for the socket.  
            _connection?.Dispose();
            _connection = null;

            var client = new TcpClient()
            {
                ReceiveTimeout = 60000
            };

            await client.ConnectAsync(_parameters.Host, _parameters.Port, cancellationToken).ConfigureAwait(false);

            _connection = CreateConnection(new ConnectionTcp
            {
                TcpClient = client,
                ConnectionId = Guid.NewGuid().ToString()
            });
        }
        protected virtual async Task CreateSSLConnectionAsync(CancellationToken cancellationToken)
        {
            // Establish the remote endpoint for the socket.  
            _connection?.Dispose();
            _connection = null;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var client = new TcpClient()
            {
                ReceiveTimeout = 60000,
            };

            await client.ConnectAsync(_parameters.Host, _parameters.Port, cancellationToken).ConfigureAwait(false);

            var sslStream = new SslStream(client.GetStream());

            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = _parameters.Host
            }, cancellationToken).ConfigureAwait(false);

            if (sslStream.IsAuthenticated && sslStream.IsEncrypted && !cancellationToken.IsCancellationRequested)
            {
                _connection = CreateConnection(new ConnectionTcp
                {
                    TcpClient = client,
                    ConnectionId = Guid.NewGuid().ToString()
                });
            }
            else
            {
                throw new Exception("Could not create connection - SSL cert has validation problem.");
            }
        }
        protected abstract Y CreateConnection(ConnectionTcp connection);
        protected abstract T CreateConnectionEventArgs(TcpConnectionEventArgs<Y> args);
        protected abstract U CreateMessageEventArgs(TcpMessageEventArgs<Y> args);
        protected abstract V CreateErrorEventArgs(TcpErrorEventArgs<Y> args);

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }
    }
}
