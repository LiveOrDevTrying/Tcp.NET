using Newtonsoft.Json;
using PHS.Networking.Enums;
using PHS.Networking.Events;
using PHS.Networking.Models;
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
    public abstract class TcpHandlerBase<T> : 
        CoreNetworkingGeneric<TcpConnectionServerBaseEventArgs<T>, TcpMessageServerBaseEventArgs<T>, TcpErrorServerBaseEventArgs<T>>, 
        ICoreNetworkingGeneric<TcpConnectionServerBaseEventArgs<T>, TcpMessageServerBaseEventArgs<T>, TcpErrorServerBaseEventArgs<T>> 
        where T : ConnectionTcpServer
    {
        protected readonly byte[] _certificate;
        protected readonly string _certificatePassword;
        protected readonly ParamsTcpServer _parameters;
        protected TcpListener _server;
        protected bool _isRunning;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpHandlerBase(ParamsTcpServer parameters)
        {
            _parameters = parameters;
        }
        public TcpHandlerBase(ParamsTcpServer parameters, byte[] certificate, string certificatePassword)
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
                    _ = Task.Run(async () => { await ListenForConnectionsAsync(cancellationToken); });
                }
                else
                {
                    _ = Task.Run(async () => { await ListenForConnectionsSSLAsync(cancellationToken); });
                }
                return;
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                {
                    Exception = ex,
                    Message = ex.Message,
                });
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

        protected abstract T CreateConnection(ConnectionTcpServer connection);
        protected virtual async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync(cancellationToken);
                    var stream = client.GetStream();
                    var reader = new StreamReader(stream);
                    var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true,
                        NewLine = _parameters.EndOfLineCharacters
                    };

                    var connection = CreateConnection(new ConnectionTcpServer
                    {
                        Client = client,
                        Reader = reader,
                        Writer = writer,
                        ConnectionId = Guid.NewGuid().ToString()
                    });

                    FireEvent(this, new TcpConnectionServerBaseEventArgs<T>
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        Connection = connection,
                    });

                    _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken); });
                }
                catch (Exception ex)
                {
                    FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                    {
                        Exception = ex,
                        Message = ex.Message,
                    });
                }

            }
        }
        protected virtual async Task ListenForConnectionsSSLAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync(cancellationToken);
                    var sslStream = new SslStream(client.GetStream());
                    await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificate = new X509Certificate2(_certificate, _certificatePassword)
                    }, cancellationToken);

                    if (sslStream.IsAuthenticated && sslStream.IsEncrypted)
                    {
                        var reader = new StreamReader(sslStream);
                        var writer = new StreamWriter(sslStream)
                        {
                            AutoFlush = true,
                            NewLine = _parameters.EndOfLineCharacters
                        };

                        var connection = CreateConnection(new ConnectionTcpServer
                        {
                            Client = client,
                            Reader = reader,
                            Writer = writer,
                            ConnectionId = Guid.NewGuid().ToString()
                        });

                        FireEvent(this, new TcpConnectionServerBaseEventArgs<T>
                        {
                            ConnectionEventType = ConnectionEventType.Connected,
                            Connection = connection,
                        });

                        _ = Task.Run(async () => { await ReceiveAsync(connection, cancellationToken); });
                    }
                    else
                    {
                        var certStatus = $"IsAuthenticated = {sslStream.IsAuthenticated} && IsEncrypted == {sslStream.IsEncrypted}";
                        FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                        {
                            Exception = new Exception(certStatus),
                            Message = certStatus
                        });

                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                    {
                        Exception = ex,
                        Message = ex.Message,
                    });
                }

            }
        }
        protected virtual async Task ReceiveAsync(T connection, CancellationToken cancellationToken)
        {
            try
            {
                while (connection.Client.Connected && !cancellationToken.IsCancellationRequested)
                {
                    if (connection.Client.Available <= 0)
                    {
                        await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    var message = await connection.Reader.ReadLineAsync();
                           
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        if (message.Trim().ToLower() == "pong")
                        {
                            connection.HasBeenPinged = false;
                        }
                        else
                        {
                            FireEvent(this, new TcpMessageServerBaseEventArgs<T>
                            {
                                Connection = connection,
                                Message = message,
                                MessageEventType = MessageEventType.Receive
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                });
            }

            FireEvent(this, new TcpConnectionServerBaseEventArgs<T>
            {
                ConnectionEventType = ConnectionEventType.Disconnect,
                Connection = connection
            });
        }

        public virtual async Task<bool> SendAsync(string message, T connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.Client.Connected && _isRunning)
                {
                    await connection.Writer.WriteLineAsync(message.AsMemory(), cancellationToken);

                    FireEvent(this, new TcpMessageServerBaseEventArgs<T>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = message
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                });
            }

            return false;
        }

        public virtual Task<bool> DisconnectConnectionAsync(T connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.Client != null)
                    {
                        connection.Client.Close();
                        connection.Client.Dispose();
                    }

                    if (connection.Writer != null)
                    {
                        connection.Writer.Dispose();
                    }

                    if (connection.Reader != null)
                    {
                        connection.Reader.Dispose();
                    }

                    FireEvent(this, new TcpConnectionServerBaseEventArgs<T>
                    {
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        Connection = connection
                    });
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerBaseEventArgs<T>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                });
            }

            return Task.FromResult(false);
        }

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