using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Core.SSL.Enums;
using Tcp.NET.Core.SSL.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Client.SSL
{
    public class TcpNETClientSSL : CoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs>,
        ITcpNETClientSSL
    {
        private ConnectionTcpClientSSLDTO _connection;
        private bool _isClientRunning;

        public virtual void Connect(string host, int port, string endOfLineCharacters)
        {
            Connect(host, port, endOfLineCharacters, host);
        }
        public virtual void Connect(string host, int port, string endOfLineCharacters, string certificateIssuedTo)
        {
            _endOfLineCharacters = endOfLineCharacters;

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                var client = new TcpClient(host, port);

                var sslStream = new SslStream(client.GetStream());

                sslStream.AuthenticateAsClient(certificateIssuedTo);

                var reader = new StreamReader(sslStream);
                var writer = new StreamWriter(sslStream)
                {
                    AutoFlush = true,
                    NewLine = endOfLineCharacters
                };

                _connection = new ConnectionTcpClientSSLDTO
                {
                    Client = client,
                    Reader = reader,
                    Writer = writer
                };

                FireEvent(this, new TcpSSLConnectionEventArgs
                {
                    ArgsType = ArgsType.Connection,
                    Client = client,
                    ConnectionEventType = ConnectionEventType.Connected,
                    ConnectionType = TcpSSLConnectionType.Connected,
                    Reader = reader,
                    Writer = writer,
                });

                _isClientRunning = true;

                StartListeningForMessages();
            }
            catch
            {
                //throw new ArgumentException("Start client error", ex);
            }
        }
        public virtual bool Disconnect()
        {
            try
            {
                if (_connection != null &&
                    _connection.Client.Connected)
                {
                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        ArgsType = ArgsType.Connection,
                        Client = _connection.Client,
                        ConnectionType = TcpSSLConnectionType.Disconnect,
                        Reader = _connection.Reader,
                        Writer = _connection.Writer
                    });

                    _connection.Writer.Dispose();
                    _connection.Reader.Dispose();
                    _connection.Client.Close();
                    _connection = null;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        protected virtual void StartListeningForMessages()
        {
            Task.Run(async () =>
            {
                while (_isClientRunning &&
                    _connection != null)
                {
                    try
                    {
                        var content = await _connection.Reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            // Digest the ping first
                            if (content.Trim().ToLower() == "ping")
                            {
                                await SendToServerAsync("pong");
                            }
                            else
                            {
                                try
                                {
                                    var packet = JsonConvert.DeserializeObject<PacketDTO>(content);

                                    FireEvent(this, new TcpSSLMessageEventArgs
                                    {
                                        MessageEventType = MessageEventType.Receive,
                                        Client = _connection.Client,
                                        Message = packet.Data,
                                        ArgsType = ArgsType.Message,
                                        Packet = packet
                                    });
                                }
                                catch
                                {
                                    FireEvent(this, new TcpSSLMessageEventArgs
                                    {
                                        MessageEventType = MessageEventType.Receive,
                                        Client = _connection.Client,
                                        Message = content,
                                        ArgsType = ArgsType.Message,
                                        Packet = new PacketDTO
                                        {
                                            Action = (int)ActionType.SendToClient,
                                            Data = content,
                                            Timestamp = DateTime.UtcNow
                                        }
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                }
            });
        }

        public virtual async Task<bool> SendToServerAsync(string message)
        {
            try
            {
                if (_connection != null &&
                    _connection.Client.Connected)
                {
                    FireEvent(this, new TcpSSLMessageEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Client = _connection.Client,
                        Message = message,
                        Packet = new PacketDTO
                        {
                            Data = message,
                            Action = (int)ActionType.SendToServer,
                            Timestamp = DateTime.UtcNow
                        },
                        ArgsType = ArgsType.Message,
                    });

                    await _connection.Writer.WriteLineAsync(message);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
        public virtual async Task<bool> SendToServerAsync(PacketDTO packet)
        {
            try
            {
                if (_connection != null &&
                    _connection.Client.Connected)
                {
                    var message = JsonConvert.SerializeObject(packet);

                    FireEvent(this, new TcpSSLMessageEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Client = _connection.Client,
                        Packet = packet,
                        Message = packet.Data,
                        ArgsType = ArgsType.Message,
                    });

                    await _connection.Writer.WriteLineAsync(message);
                    return true;
                }
            }
            catch
            {
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

        public TcpClient Client
        {
            get
            {
                return _connection != null ? _connection.Client : null;
            }
        }
    }
}
