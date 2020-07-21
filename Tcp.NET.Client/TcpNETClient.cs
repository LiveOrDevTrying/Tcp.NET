using Newtonsoft.Json;
using PHS.Networking.Enums;
using PHS.Networking.Models;
using PHS.Networking.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public class TcpNETClient : CoreNetworking<TcpConnectionClientEventArgs, TcpMessageClientEventArgs, TcpErrorClientEventArgs>,
        ITcpNETClient
    {
        protected readonly IParamsTcpClient _parameters;
        protected readonly string _oauthToken;
        protected IConnection _connection;
        protected bool _isClientRunning;

        public TcpNETClient(IParamsTcpClient parameters, string oauthToken = "")
        {
            _parameters = parameters;
            _oauthToken = oauthToken;
        }
        public virtual async Task ConnectAsync()
        {
            // Connect to a remote device.  
            try
            {
                if (_parameters.IsSSL)
                {
                    CreateSSLConnection();
                }
                else
                {
                    CreateConnection();
                }

                _isClientRunning = true;

                if (!string.IsNullOrWhiteSpace(_oauthToken))
                {
                    await _connection.Writer.WriteLineAsync($"oauth:{_oauthToken}");
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                StartListeningForMessagesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception ex)
            {
                await FireEventAsync(this, new TcpErrorClientEventArgs
                {
                    Exception = ex,
                    Connection = _connection,
                    Message = ex.Message
                });
            }
        }
        public virtual async Task<bool> DisconnectAsync()
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

                    await FireEventAsync(this, new TcpConnectionClientEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        Connection = _connection
                    });

                    _connection = null;

                    return true;
                }
            }
            catch (Exception ex)
            {
                await FireEventAsync(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });
            }

            return false;
        }

        protected virtual void CreateConnection()
        {
            // Establish the remote endpoint for the socket.  
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var client = new TcpClient(_parameters.Uri, _parameters.Port);
            client.ReceiveTimeout = 60000;

            var reader = new StreamReader(client.GetStream());
            var writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true,
                NewLine = _parameters.EndOfLineCharacters
            };

            _connection = new Connection
            {
                Client = client,
                Reader = reader,
                Writer = writer
            };
        }
        protected virtual void CreateSSLConnection()
        {
            // Establish the remote endpoint for the socket.  
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var client = new TcpClient(_parameters.Uri, _parameters.Port);
            client.ReceiveTimeout = 60000;

            var sslStream = new SslStream(client.GetStream());

            sslStream.AuthenticateAsClient(_parameters.Uri);

            var reader = new StreamReader(sslStream);
            var writer = new StreamWriter(sslStream)
            {
                AutoFlush = true,
                NewLine = _parameters.EndOfLineCharacters
            };

            _connection = new Connection
            {
                Client = client,
                Reader = reader,
                Writer = writer
            };
        }
        protected virtual async Task StartListeningForMessagesAsync()
        {
            while (_isClientRunning &&
                _connection != null)
            {
                try
                {
                    var message = await _connection.Reader.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        // Digest the ping first
                        if (message.Trim().ToLower() == "ping")
                        {
                            await SendToServerRawAsync("pong");
                        }
                        else
                        {
                            var packet = MessageReceived(message);

                            await FireEventAsync(this, new TcpMessageClientEventArgs
                            {
                                MessageEventType = MessageEventType.Receive,
                                Message = packet.Data,
                                Packet = packet,
                                Connection = _connection
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    await FireEventAsync(this, new TcpErrorClientEventArgs
                    {
                        Connection = _connection,
                        Exception = ex,
                        Message = ex.Message
                    });

                    await DisconnectAsync();
                }
            }
        }
        protected virtual IPacket MessageReceived(string message)
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

        public virtual async Task<bool> SendToServerAsync<T>(T packet) where T : IPacket
        {
            try
            {
                if (_connection != null &&
                    _connection.Client.Connected)
                {
                    var message = JsonConvert.SerializeObject(packet);
                    await _connection.Writer.WriteLineAsync(message);

                    await FireEventAsync(this, new TcpMessageClientEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Packet = packet,
                        Message = packet.Data,
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                await FireEventAsync(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });

                await DisconnectAsync();
            }

            return false;
        }
        public virtual async Task<bool> SendToServerAsync(string message)
        {
            return await SendToServerAsync(new Packet
            {
                Data = message,
                Timestamp = DateTime.UtcNow
            });
        }
        public virtual async Task<bool> SendToServerRawAsync(string message)
        {
            try
            {
                if (_connection != null &&
                    _connection.Client.Connected)
                {
                    await _connection.Writer.WriteAsync($"{message}{_parameters.EndOfLineCharacters}");

                    await FireEventAsync(this, new TcpMessageClientEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = _connection,
                        Message = message,
                        Packet = new Packet
                        {
                            Data = message,
                            Timestamp = DateTime.UtcNow
                        },
                    }); 
                    return true;
                }
            }
            catch (Exception ex)
            {
                await FireEventAsync(this, new TcpErrorClientEventArgs
                {
                    Connection = _connection,
                    Exception = ex,
                    Message = ex.Message
                });

                await DisconnectAsync();
            }

            return false;
        }

        public bool IsRunning
        {
            get
            {
                return _connection != null && _connection.Client.Connected;
            }
        }
        public IConnection Connection
        {
            get
            {
                return _connection;
            }
        }
    }
}
