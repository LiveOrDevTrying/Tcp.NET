using PHS.Networking.Enums;
using PHS.Networking.Services;
using PHS.Networking.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client.Handlers
{
    public abstract class TcpClientHandlerBase<T> : 
        CoreNetworkingGeneric<TcpConnectionClientEventArgs, TcpMessageClientEventArgs, TcpErrorClientEventArgs>
        where T : ConnectionTcp
    {
        protected readonly ParamsTcpClient _parameters;
        protected readonly string _token;
        protected T _connection;

        public TcpClientHandlerBase(ParamsTcpClient parameters, string token = "")
        {
            _parameters = parameters;
            _token = token;
        }

        public virtual async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_parameters.IsSSL)
                {
                    await CreateSSLConnectionAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await CreateNonSSLConnectionAsync(cancellationToken).ConfigureAwait(false);
                }

                if (!string.IsNullOrWhiteSpace(_token) && !cancellationToken.IsCancellationRequested)
                {
                    await SendAsync($"oauth:{_token}", cancellationToken);
                }

                if (_connection != null && _connection.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    FireEvent(this, new TcpConnectionClientEventArgs
                    {
                        Connection = _connection,
                        ConnectionEventType = ConnectionEventType.Connected,
                    });

                    _ = Task.Run(async () => { await ReceiveAsync(cancellationToken).ConfigureAwait(false); }, cancellationToken);

                    return true;
                };
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorClientEventArgs
                {
                    Exception = ex,
                    Connection = _connection,
                    Message = ex.Message
                });

                await DisconnectAsync(cancellationToken);
            }

            return false;
        }
        public virtual Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_connection != null)
                {
                    if (_connection.TcpClient != null)
                    {
                        _connection.TcpClient.Close();
                        _connection.TcpClient.Dispose();

                        FireEvent(this, new TcpConnectionClientEventArgs
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = _connection
                        });
                    }

                    _connection = null;

                    return Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            return Task.FromResult(false);
        }

        public virtual async Task<bool> SendAsync(string message, CancellationToken cancellationToken = default)
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

                    FireEvent(this, new TcpMessageClientEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Message = message,
                        Bytes = bytes
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });

                await DisconnectAsync(cancellationToken);
            }

            return false;
        }
        public virtual async Task<bool> SendAsync(byte[] message, CancellationToken cancellationToken = default)
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

                    FireEvent(this, new TcpMessageClientEventArgs
                    { 
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Message = null,
                        Bytes = bytes
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });

                await DisconnectAsync(cancellationToken);
            }

            return false;
        }

        protected virtual async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var persistantMS = new MemoryStream())
                {
                    while (!cancellationToken.IsCancellationRequested && _connection != null && _connection.TcpClient.Connected)
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
                                if (_connection.TcpClient.Available <= 0)
                                {
                                    await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                                    continue;
                                }

                                var buffer = new ArraySegment<byte>(new byte[_connection.TcpClient.Available]);
                                var result = await _connection.TcpClient.Client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
                                await ms.WriteAsync(buffer.Array, buffer.Offset, result).ConfigureAwait(false);

                                endOfMessage = Statics.ByteArrayContainsSequence(ms.ToArray(), _parameters.EndOfLineBytes);
                            }
                            while (!endOfMessage && _connection != null && _connection.TcpClient.Connected);

                            if (endOfMessage)
                            {
                                var parts = Statics.ByteArraySeparate(ms.ToArray(), _parameters.EndOfLineBytes);

                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (parts.Length > 1 && i == parts.Length - 1)
                                    {
                                        await persistantMS.WriteAsync(parts[i], cancellationToken);
                                    }
                                    else
                                    {
                                        if (Statics.ByteArrayEquals(parts[i], _parameters.PingBytes))
                                        {
                                            await SendAsync(_parameters.PongBytes, cancellationToken);
                                        }
                                        else
                                        {
                                            string message = null;

                                            if (!_parameters.OnlyEmitBytes)
                                            {
                                                message = Encoding.UTF8.GetString(parts[i]);
                                            }

                                            FireEvent(this, new TcpMessageClientEventArgs
                                            {
                                                MessageEventType = MessageEventType.Receive,
                                                Connection = _connection,
                                                Message = message,
                                                Bytes = parts[i]
                                            });
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
                FireEvent(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            await DisconnectAsync(cancellationToken);
        }

        protected virtual async Task CreateNonSSLConnectionAsync(CancellationToken cancellationToken)
        {
            // Establish the remote endpoint for the socket.  
            var client = new TcpClient()
            {
                ReceiveTimeout = 60000
            };

            await client.ConnectAsync(_parameters.Host, _parameters.Port, cancellationToken).ConfigureAwait(false);

            _connection = CreateConnection(new ConnectionTcp
            {
                TcpClient = client
            });
        }
        protected virtual async Task CreateSSLConnectionAsync(CancellationToken cancellationToken)
        {
            // Establish the remote endpoint for the socket.  
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
                    TcpClient = client
                });
            }
            else
            {
                throw new Exception("Could not create connection - SSL cert has validation problem.");
            }
        }
        protected abstract T CreateConnection(ConnectionTcp connection);

        public override void Dispose()
        {
            DisconnectAsync().Wait();
        }

        public T Connection
        {
            get
            {
                return _connection;
            }
        }
    }
}
