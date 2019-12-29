using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Core.SSL.Enums;
using Tcp.NET.Core.SSL.Events.Args;

namespace Tcp.NET.Client.SSL
{
    public class TcpNETClientSSL : CoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs>,
        ITcpNETClientSSL
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _isClientRunning;

        public virtual void Connect(string host, int port, string endOfLineCharacters)
        {
            _endOfLineCharacters = endOfLineCharacters;

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var client = new TcpClient(host, port);

                var sslStream = new SslStream(client.GetStream());

                sslStream.AuthenticateAsClient(host);

                var reader = new StreamReader(sslStream);
                var writer = new StreamWriter(sslStream)
                {
                    AutoFlush = true,
                    NewLine = endOfLineCharacters
                };

                _client = client;
                _reader = reader;
                _writer = writer;

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
                if (_client != null &&
                    _client.Connected)
                {
                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        ArgsType = ArgsType.Connection,
                        Client = _client,
                        ConnectionType = TcpSSLConnectionType.Disconnect,
                        Reader = _reader,
                        Writer = _writer
                    });

                    _writer.Dispose();
                    _reader.Dispose();
                    _client.Close();
                    _client = null;
                    _reader = null;
                    _writer = null;
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
                    _client != null)
                {
                    try
                    {
                        var content = await _reader.ReadLineAsync();

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
                                        Client = _client,
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
                                        Client = _client,
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
                if (_client != null &&
                    _client.Connected)
                {
                    FireEvent(this, new TcpSSLMessageEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Client = _client,
                        Message = message,
                        Packet = new PacketDTO
                        {
                            Data = message,
                            Action = (int)ActionType.SendToServer,
                            Timestamp = DateTime.UtcNow
                        },
                        ArgsType = ArgsType.Message,
                    });

                    await _writer.WriteLineAsync(message);
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
                if (_client != null &&
                    _client.Connected)
                {
                    var message = JsonConvert.SerializeObject(packet);

                    FireEvent(this, new TcpSSLMessageEventArgs
                    {
                        MessageEventType = MessageEventType.Sent,
                        Client = _client,
                        Packet = packet,
                        Message = packet.Data,
                        ArgsType = ArgsType.Message,
                    });

                    await _writer.WriteLineAsync(message);
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
                return _client != null && _client.Connected;
            }
        }

        public TcpClient Client
        {
            get
            {
                return _client;
            }
        }
    }
}
