using Newtonsoft.Json;
using PHS.Networking.Enums;
using PHS.Networking.Events;
using PHS.Networking.Models;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public class TcpHandler : 
        CoreNetworking<TcpConnectionServerEventArgs, TcpMessageServerEventArgs, TcpErrorServerEventArgs>, 
        ICoreNetworking<TcpConnectionServerEventArgs, TcpMessageServerEventArgs, TcpErrorServerEventArgs> 
    {
        protected readonly byte[] _certificate;
        protected readonly string _certificatePassword;
        protected readonly IParamsTcpServer _parameters;
        protected int _numberOfConnections;
        protected TcpListener _server;
        protected volatile bool _isRunning;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpHandler(IParamsTcpServer parameters)
        {
            _parameters = parameters;
        }
        public TcpHandler(IParamsTcpServer parameters, byte[] certificate, string certificatePassword)
        {
            _parameters = parameters;
            _certificate = certificate;
            _certificatePassword = certificatePassword;
        }

        public virtual void Start()
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
                    _ = Task.Run(async () => { await ListenForConnectionsAsync(); });
                }
                else
                {
                    _ = Task.Run(async () => { await ListenForConnectionsSSLAsync(); });
                }
                return;
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerEventArgs
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

            _numberOfConnections = 0;
            
            FireEvent(this, new ServerEventArgs
            {
                ServerEventType = ServerEventType.Stop
            });
        }
 
        protected virtual async Task ListenForConnectionsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    var stream = client.GetStream();
                    var reader = new StreamReader(stream);
                    var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true,
                        NewLine = _parameters.EndOfLineCharacters
                    };

                    var connection = new ConnectionTcpServer
                    {
                        Client = client,
                        Reader = reader,
                        Writer = writer,
                        ConnectionId = Guid.NewGuid().ToString()
                    };

                    FireEvent(this, new TcpConnectionServerEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        Connection = connection,
                    });

                    _ = Task.Run(async () => { await StartListeningForMessagesAsync(connection); });
#pragma warning restore CS4014 // Becaue this call is not awaited, execution of the current method continues before the call is completed
                    
                    _numberOfConnections++;
                }
                catch (Exception ex)
                {
                    FireEvent(this, new TcpErrorServerEventArgs
                    {
                        Exception = ex,
                        Message = ex.Message,
                    });
                }

            }
        }
        protected virtual async Task ListenForConnectionsSSLAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    var sslStream = new SslStream(client.GetStream());
                    await sslStream.AuthenticateAsServerAsync(new X509Certificate2(_certificate, _certificatePassword));

                    if (sslStream.IsAuthenticated && sslStream.IsEncrypted)
                    {
                        var reader = new StreamReader(sslStream);
                        var writer = new StreamWriter(sslStream)
                        {
                            AutoFlush = true,
                            NewLine = _parameters.EndOfLineCharacters
                        };

                        var connection = new ConnectionTcpServer
                        {
                            Client = client,
                            Reader = reader,
                            Writer = writer,
                            ConnectionId = Guid.NewGuid().ToString()
                        };

                        FireEvent(this, new TcpConnectionServerEventArgs
                        {
                            ConnectionEventType = ConnectionEventType.Connected,
                            Connection = connection,
                        });

                        _numberOfConnections++;

                        _ = Task.Run(async () => { await StartListeningForMessagesAsync(connection); });
                    }
                    else
                    {
                        var certStatus = $"IsAuthenticated = {sslStream.IsAuthenticated} && IsEncripted == {sslStream.IsEncrypted}";
                        FireEvent(this, new TcpErrorServerEventArgs
                        {
                            Exception = new Exception(certStatus),
                            Message = certStatus
                        });

                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, new TcpErrorServerEventArgs
                    {
                        Exception = ex,
                        Message = ex.Message,
                    });
                }

            }
        }
        protected virtual async Task StartListeningForMessagesAsync(IConnectionTcpServer connection)
        {
            do
            {
                try
                {
                    var line = await connection.Reader.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (line.Trim().ToLower() == "pong")
                        {
                            connection.HasBeenPinged = false;
                        }
                        else
                        {
                            MessageReceived(line, connection);
                        }
                    }
                }
                catch
                {
                    FireEvent(this, new TcpConnectionServerEventArgs
                    {
                        Connection = connection,
                        ConnectionEventType = ConnectionEventType.Disconnect,
                    });

                    DisconnectConnection(connection);
                }
            } while (connection.Client != null && connection.Client.Connected);
        }

        protected virtual void MessageReceived(string message, IConnectionTcpServer connection)
        {
            IPacket packet;

            try
            {
                packet = JsonConvert.DeserializeObject<Packet>(message);

                if (string.IsNullOrWhiteSpace(packet.Data))
                {
                    packet = new Packet
                    {
                        Data = message,
                        Timestamp = DateTime.UtcNow
                    };
                }
            }
            catch
            {
                packet = new Packet
                {
                    Data = message,
                    Timestamp = DateTime.UtcNow
                };
            }

            FireEvent(this, new TcpMessageServerEventArgs
            {
                MessageEventType = MessageEventType.Receive,
                Packet = packet,
                Connection = connection
            });
        }

        public virtual async Task<bool> SendAsync<T>(T packet, IConnectionTcpServer connection) where T : IPacket
        {
            try
            {
                if (!_isRunning) { return false; }

                var message = JsonConvert.SerializeObject(packet);

                await connection.Writer.WriteLineAsync(message);

                FireEvent(this, new TcpMessageServerEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Packet = packet,
                    Connection = connection
                });

                return true;
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerEventArgs
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                });

                DisconnectConnection(connection);
            }

            return false;
        }
        public virtual async Task<bool> SendAsync(string message, IConnectionTcpServer connection)
        {
            return await SendAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            }, connection);
        }
        public virtual async Task<bool> SendRawAsync(string message, IConnectionTcpServer connection)
        {
            try
            {
                if (!_isRunning) { return false; }

                await connection.Writer.WriteLineAsync(message);

                FireEvent(this, new TcpMessageServerEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Connection = connection,
                    Packet = new Packet
                    {
                        Data = message,
                        Timestamp = DateTime.UtcNow
                    },
                });

                return true;
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerEventArgs
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                });

                DisconnectConnection(connection);
            }

            return false;
        }
        public virtual bool DisconnectConnection(IConnectionTcpServer connection)
        {
            try
            {
                _numberOfConnections--;

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

                    FireEvent(this, new TcpConnectionServerEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        Connection = connection
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                FireEvent(this, new TcpErrorServerEventArgs
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                });
            }

            return false;
        }

        protected virtual void FireEvent(object sender, ServerEventArgs args)
        {
            _serverEvent?.Invoke(sender, args);
        }

        public override void Dispose()
        {
            Stop();

            base.Dispose();
        }

        public int NumberOfConnections
        {
            get
            {
                return _numberOfConnections;
            }
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