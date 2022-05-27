using PHS.Networking.Enums;
using PHS.Networking.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
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
                    await _connection.Writer.WriteLineAsync($"oauth:{_token}".AsMemory(), cancellationToken).ConfigureAwait(false);
                }

                if (_connection.Client.Connected && !cancellationToken.IsCancellationRequested)
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
            }

            return false;
        }
        public virtual Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_connection != null)
                {
                    if (_connection != null &&
                        _connection.Writer != null)
                    {
                        _connection.Writer.Dispose();
                    }

                    if (_connection != null &&
                        _connection.Reader != null)
                    {
                        _connection.Reader.Dispose();
                    }

                    if (_connection != null &&
                        _connection.Client != null)
                    {
                        _connection.Client.Close();
                        _connection.Client.Dispose();
                    }

                    FireEvent(this, new TcpConnectionClientEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        Connection = _connection
                    });

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
                    _connection.Client != null &&
                    _connection.Client.Connected &&
                    !cancellationToken.IsCancellationRequested)
                {
                    await _connection.Writer.WriteAsync($"{message}{_parameters.EndOfLineCharacters}".AsMemory(), cancellationToken).ConfigureAwait(false);

                    FireEvent(this, new TcpMessageClientEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Message = message
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
            }

            return false;
        }

        protected virtual async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _connection != null && _connection.Client.Connected)
                {
                    if (_connection.Client.Available <= 0)
                    {
                        await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    var message = await _connection.Reader.ReadLineAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        // Digest the ping first
                        if (message.Trim().ToLower() == "ping")
                        {
                            await SendAsync("pong", cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            FireEvent(this, new TcpMessageClientEventArgs
                            {
                                MessageEventType = MessageEventType.Receive,
                                Connection = _connection,
                                Message = message
                            });
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
        }

        protected virtual async Task CreateNonSSLConnectionAsync(CancellationToken cancellationToken)
        {
            // Establish the remote endpoint for the socket.  
            var client = new TcpClient()
            {
                ReceiveTimeout = 60000
            };

            await client.ConnectAsync(_parameters.Host, _parameters.Port, cancellationToken).ConfigureAwait(false);

            var reader = new StreamReader(client.GetStream());
            var writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true,
                NewLine = _parameters.EndOfLineCharacters
            };

            _connection = CreateConnection(new ConnectionTcp
            {
                Client = client,
                Reader = reader,
                Writer = writer
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
                var reader = new StreamReader(sslStream);
                var writer = new StreamWriter(sslStream)
                {
                    AutoFlush = true,
                    NewLine = _parameters.EndOfLineCharacters
                };

                _connection = CreateConnection(new ConnectionTcp
                {
                    Client = client,
                    Reader = reader,
                    Writer = writer
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
