using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;
using Tcp.NET.Core.SSL.Events.Args;
using Tcp.NET.Core.SSL;
using Tcp.NET.Core.SSL.Enums;
using Tcp.NET.Server.Models;
using System.Security.Cryptography.X509Certificates;

namespace Tcp.NET.Server.Handlers
{
    public sealed class TcpHandlerSSL : 
        CoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs>, 
        ICoreNetworking<TcpSSLConnectionEventArgs, TcpSSLMessageEventArgs, TcpSSLErrorEventArgs> 
    {
        private string _url;
        private volatile bool _isServerRunning;
        private int _port;
        private int _numberOfConnections;
        private TcpListener _server;

        public TcpHandlerSSL(string url, int port, string endOfLineCharacters, X509Certificate certificate)
        {
            if (!_isServerRunning)
            {
                _isServerRunning = true;
                _endOfLineCharacters = endOfLineCharacters;
                _port = port;
                _url = url;

                StartServer(certificate);
            }
        }
        public TcpHandlerSSL(string url, int port, string endOfLineCharacters, string certificateIssuedTo, StoreLocation storeLocation)
        {
            if (!_isServerRunning)
            {
                _isServerRunning = true;
                _endOfLineCharacters = endOfLineCharacters;
                _port = port;
                _url = url;

                var serverCertificate = SSLHelper.GetServerCert(certificateIssuedTo, storeLocation);
                StartServer(serverCertificate);
            }
        }
        private void StartServer(X509Certificate cert)
        {
            var hostInfo = Dns.GetHostEntry(_url);

            foreach (var address in hostInfo.AddressList)
            {
                try
                {
                    var localEndPoint = new IPEndPoint(address, _port);

                    _server = new TcpListener(localEndPoint);
                    _server.Start();

                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.ServerStart,
                        ArgsType = ArgsType.Connection,
                        ConnectionType = TcpSSLConnectionType.ServerStart
                    });

                    Task.Run(async () =>
                    {
                        await ListenForConnectionsAsync(cert);
                    });

                    return;
                }
                catch
                { }
            }
        }


        private async Task ListenForConnectionsAsync(X509Certificate cert)
        {

            while (_isServerRunning)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    var sslStream = new SslStream(client.GetStream());
                    await sslStream.AuthenticateAsServerAsync(cert);

                    var reader = new StreamReader(sslStream);
                    var writer = new StreamWriter(sslStream)
                    {
                        AutoFlush = true,
                        NewLine = _endOfLineCharacters
                    };

                    FireEvent(this, new TcpSSLConnectionEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.Connected,
                        Client = client,
                        ArgsType = ArgsType.Connection,
                        ConnectionType = TcpSSLConnectionType.Connected,
                        Reader = reader,
                        Writer = writer
                    });

                    StartListeningForMessages(reader, client);
                }
                catch
                { }

            }
        }

        private void StartListeningForMessages(StreamReader reader, TcpClient client)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var readline = await reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(readline))
                        {
                            PacketDTO packet;

                            try
                            {
                                packet = JsonConvert.DeserializeObject<PacketDTO>(readline);
                            }
                            catch
                            {
                                packet = new PacketDTO
                                {
                                    Action = (int)ActionType.SendToServer,
                                    Data = readline,
                                    Timestamp = DateTime.UtcNow
                                };
                            }

                            FireEvent(this, new TcpSSLMessageEventArgs
                            {
                                MessageEventType = MessageEventType.Receive,
                                Client = client,
                                Message = readline,
                                ArgsType = ArgsType.Message,
                                Packet = packet,
                            });
                        }
                    }
                    catch
                    {
                        FireEvent(this, new TcpSSLConnectionEventArgs
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Client = client,
                            ConnectionType = TcpSSLConnectionType.Disconnect,
                            ArgsType = ArgsType.Connection,
                        });

                        return;
                    }
                }
            });
        }

        public async Task<bool> SendAsync(PacketDTO packet, ConnectionTcpClientSSLDTO connection)
        {
            try
            {
                if (!_isServerRunning) { return false; }

                var message = JsonConvert.SerializeObject(packet);

                // Convert the string data to byte data using UTF8  encoding.  
               // var nextMessage = string.Format("{0}{1}", message, _endOfLineCharacters);
                //var byteData = Encoding.UTF8.GetBytes(message);

                await connection.Writer.WriteLineAsync(message);

                FireEvent(this, new TcpSSLMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Message = message,
                    ArgsType = ArgsType.Message,
                    Packet = packet,
                    Client = connection.Client
                });

                
                return true;
            }
            catch
            {
                DisconnectClient(connection.Client);
            }

            return false;
        }
        public async Task<bool> SendAsync(string message, ConnectionTcpClientSSLDTO connection)
        {
            try
            {
                if (!_isServerRunning) { return false; }

                var packet = new PacketDTO
                {
                    Action = (int)ActionType.SendToClient,
                    Data = message,
                    Timestamp = DateTime.UtcNow
                };

                var payload = JsonConvert.SerializeObject(packet);

                // Convert the string data to byte data using UTF8  encoding.  
                // var nextMessage = $"{payload}{_endOfLineCharacters}";
                // var byteData = Encoding.UTF8.GetBytes(payload);

                await connection.Writer.WriteLineAsync(payload);

                FireEvent(this, new TcpSSLMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Message = payload,
                    ArgsType = ArgsType.Message,
                    Packet = packet,
                    Client = connection.Client,
                });

                return true;
            }
            catch
            {
                DisconnectClient(connection.Client);
            }

            return false;
        }
        public async Task<bool> SendRawAsync(string message, ConnectionTcpClientSSLDTO connection)
        {
            try
            {
                if (!_isServerRunning) { return false; }

                // Convert the string data to byte data using UTF8  encoding.  
                //var nextMessage = string.Format("{0}{1}", message, _endOfLineCharacters);
                //var byteData = Encoding.UTF8.GetBytes(message);

                await connection.Writer.WriteLineAsync(message);

                FireEvent(this, new TcpSSLMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Client = connection.Client,
                    Message = message,
                    ArgsType = ArgsType.Message,
                    Packet = new PacketDTO
                    {
                        Action = (int)ActionType.SendToClient,
                        Data = message,
                        Timestamp = DateTime.UtcNow
                    }
                });

                return true;
            }
            catch
            {
                DisconnectClient(connection.Client);
            }

            return false;
         }

        public bool DisconnectClient(TcpClient client)
        {
            try
            {
                FireEvent(this, new TcpSSLConnectionEventArgs
                {
                    ConnectionEventType = ConnectionEventType.Disconnect,
                    ArgsType = ArgsType.Connection,
                    ConnectionType = TcpSSLConnectionType.Disconnect,
                    Client = client
                });

                if (client.Connected)
                {
                    _numberOfConnections--;

                    client.Close();
                }
                return true;
            }
            catch
            { }

            return false;
        }
        
        public override void Dispose()
        {
            _isServerRunning = false;

            if (_server != null)
            {
                _server.Stop();
                _server = null;

                FireEvent(this, new TcpSSLConnectionEventArgs
                {
                    ConnectionEventType = ConnectionEventType.ServerStop,
                    ArgsType = ArgsType.Connection,
                });
            }

            base.Dispose();
        }

        public int NumberOfConnections
        {
            get
            {
                return _numberOfConnections;
            }
        }
        public bool IsServerRunning
        {
            get
            {
                return _isServerRunning;
            }
        }
        public TcpListener Server
        {
            get
            {
                return _server;
            }
        }

    }
}
