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
        protected readonly X509Certificate _certificate;
        protected readonly IParamsTcpServer _parameters;
        protected int _numberOfConnections;
        protected TcpListener _server;
        protected volatile bool _isRunning;

        private event NetworkingEventHandler<ServerEventArgs> _serverEvent;

        public TcpHandler(IParamsTcpServer parameters)
        {
            _isRunning = true;
            _parameters = parameters;
        }
        public TcpHandler(IParamsTcpServer parameters, X509Certificate certificate)
        {
            _isRunning = true;
            _parameters = parameters;
            _certificate = certificate;
        }

        public void Start()
        {
            try
            {
                if (_server != null)
                {
                    Stop();
                }

                _server = new TcpListener(IPAddress.Any, _parameters.Port);
                _server.Start();

                FireEvent(this, new ServerEventArgs
                {
                    ServerEventType = ServerEventType.Start
                });

                if (_certificate == null)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    ListenForConnectionsAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    ListenForConnectionsSSLAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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
        public void Stop()
        {
            _isRunning = false;

            if (_server != null)
            {
                _server.Stop();

                FireEvent(this, new ServerEventArgs
                {
                    ServerEventType = ServerEventType.Stop
                });

                _server = null;
            }

            _numberOfConnections = 0;
        }

        private async Task ListenForConnectionsAsync()
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

                    var connection = new ConnectionServer
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    StartListeningForMessagesAsync(connection);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    
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
        private async Task ListenForConnectionsSSLAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    var sslStream = new SslStream(client.GetStream());
                    await sslStream.AuthenticateAsServerAsync(_certificate);

                    var reader = new StreamReader(sslStream);
                    var writer = new StreamWriter(sslStream)
                    {
                        AutoFlush = true,
                        NewLine = _parameters.EndOfLineCharacters
                    };

                    var connection = new ConnectionServer
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    StartListeningForMessagesAsync(connection);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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
        private async Task StartListeningForMessagesAsync(IConnectionServer connection)
        {
            var isRunning = true;

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
                            var packet = MessageReceived(line, connection);

                            if (packet != null)
                            {
                                FireEvent(this, new TcpMessageServerEventArgs
                                {
                                    MessageEventType = MessageEventType.Receive,
                                    Message = line,
                                    Packet = packet,
                                    Connection = connection
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FireEvent(this, new TcpErrorServerEventArgs
                    {
                        Connection = connection,
                        Exception = ex,
                        Message = ex.Message,
                    });

                    isRunning = false;

                    DisconnectConnection(connection);
                    return;
                }
            } while (isRunning);
        }

        protected virtual IPacket MessageReceived(string message, IConnectionServer connection)
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

            return packet;
        }

        public async Task<bool> SendAsync<T>(T packet, IConnectionServer connection) where T : IPacket
        {
            try
            {
                if (!_isRunning) { return false; }

                var message = JsonConvert.SerializeObject(packet);

                await connection.Writer.WriteLineAsync(message);

                FireEvent(this, new TcpMessageServerEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Message = message,
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
        public async Task<bool> SendAsync(string message, IConnectionServer connection)
        {
            return await SendAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            }, connection);
        }
        public async Task<bool> SendRawAsync(string message, IConnectionServer connection)
        {
            try
            {
                if (!_isRunning) { return false; }

                await connection.Writer.WriteLineAsync(message);

                FireEvent(this, new TcpMessageServerEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Message = message,
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

        public bool DisconnectConnection(IConnectionServer connection)
        {
            try
            {
                _numberOfConnections--;

                if (connection != null)
                {
                    if (connection.Reader != null)
                    {
                        connection.Reader.Dispose();
                    }

                    if (connection.Writer != null)
                    {
                        connection.Writer.Dispose();
                    }

                    if (connection.Client != null)
                    {
                        connection.Client.Close();
                        connection.Client.Dispose();
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

        protected void FireEvent(object sender, ServerEventArgs args)
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