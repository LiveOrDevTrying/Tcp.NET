using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using Tcp.NET.Core.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Enums;

namespace Tcp.NET.Server
{
    public class TcpHandler : 
        CoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>, 
        ICoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs> 
    {
        protected Thread _tcpServerThread;
        protected string _url;
        protected volatile bool _isServerRunning;
        protected int _port;
        protected int _numberOfConnections;
        protected Socket _connectionSocket;

        protected ManualResetEvent _allDone = new ManualResetEvent(false);

        public virtual void Start(string url, int port, string endOfLineCharacters)
        {
            if (!_isServerRunning)
            {
                _isServerRunning = true;
                _endOfLineCharacters = endOfLineCharacters;
                _port = port;
                _url = url;

                if (_tcpServerThread != null)
                {
                    _tcpServerThread.Abort();
                    _tcpServerThread = null;
                }

                _tcpServerThread = new Thread(new ThreadStart(ServerThread));
                _tcpServerThread.Start();
            }
        }
        protected virtual void ServerThread()
        {
            // Establish the local endpoint for the socket.  
            var hostInfo = Dns.GetHostEntry(_url);

            foreach (var address in hostInfo.AddressList)
            {
                try
                {
                    Console.WriteLine("Trying to bind TCP on " + address.ToString() + ":" + _port);

                    var localEndPoint = new IPEndPoint(address, _port);

                    // Create a TCP/IP socket.  
                    _connectionSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Bind the socket to the local endpoint and listen for incoming connections.  
                    _connectionSocket.Bind(localEndPoint);
                    _connectionSocket.Listen(100);      // Microsoft starts backlog at 100

                    _isServerRunning = true;

                    Console.WriteLine("Bound on " + address.ToString() + ":" + _port);

                    FireEvent(this, new TcpConnectionEventArgs
                    {
                        ConnectionEventType = ConnectionEventType.ServerStart,
                        ArgsType = ArgsType.Connection,
                        Socket = _connectionSocket,
                        ConnectionType = TcpConnectionType.ServerStart
                    });

                    while (_isServerRunning)
                    {
                        // Set the event to nonsignaled state.  
                        _allDone.Reset();

                        _connectionSocket.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            _connectionSocket);

                        // Wait until a connection is made before continuing.  
                        _allDone.WaitOne();
                    }
                    break;
                }
                catch
                {
                    Console.WriteLine("Bad IP. " + address.ToString() + ":" + _port + "Trying the next ...");
                    //throw new ArgumentException("Server start error", ex);
                }
            }
        }
        public virtual void Stop()
        {
            try
            {
                if (!_isServerRunning) { return; }

                _isServerRunning = false;

                if (_connectionSocket != null &&
                    _connectionSocket.Connected)
                {
                    _connectionSocket.Shutdown(SocketShutdown.Both);
                    _connectionSocket.Close();
                }

                FireEvent(this, new TcpConnectionEventArgs()
                {
                    ConnectionEventType = ConnectionEventType.ServerStop,
                    Socket = _connectionSocket,
                    ArgsType = ArgsType.Connection
                });

                if (_tcpServerThread != null)
                {
                    _isServerRunning = false;
                }
            }
            catch
            {
                _isServerRunning = false;
                Console.WriteLine("Server disconnect error from TCP.");
            }
        }

        public bool Send(PacketDTO packet, Socket socket)
        {
            try
            {
                if (!_isServerRunning) { return false; }

                var message = JsonConvert.SerializeObject(packet);

                // Convert the string data to byte data using UTF8  encoding.  
                var nextMessage = string.Format("{0}{1}", message, _endOfLineCharacters);
                var byteData = Encoding.UTF8.GetBytes(nextMessage);

                FireEvent(this, new TcpMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Socket = socket,
                    Message = message,
                    ArgsType = ArgsType.Message,
                    Packet = packet
                });

                // Begin sending the data to the remote device.  
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), socket);
                return true;
            }
            catch
            {
                Console.WriteLine("Send error from tcp");
                DisconnectClient(socket);
            }

            return false;
        }
        public bool Send(string message, Socket socket)
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
                var nextMessage = $"{payload}{_endOfLineCharacters}";
                var byteData = Encoding.UTF8.GetBytes(nextMessage);

                // Begin sending the data to the remote device.  
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), socket);

                FireEvent(this, new TcpMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Socket = socket,
                    Message = payload,
                    ArgsType = ArgsType.Message,
                    Packet = packet
                });

                return true;
            }
            catch
            {
                Console.WriteLine("Send error from tcp");
                DisconnectClient(socket);
            }

            return false;
        }
        public bool SendRaw(string message, Socket socket)
        {
            try
            {
                if (!_isServerRunning) { return false; }

                // Convert the string data to byte data using UTF8  encoding.  
                var nextMessage = string.Format("{0}{1}", message, _endOfLineCharacters);
                var byteData = Encoding.UTF8.GetBytes(nextMessage);

                // Begin sending the data to the remote device.  
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), socket);

                FireEvent(this, new TcpMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Socket = socket,
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
                Console.WriteLine("Send raw error from tcp");
                DisconnectClient(socket);
            }

            return false;
         }

        public bool DisconnectClient(Socket socket)
        {
            try
            {
                FireEvent(this, new TcpConnectionEventArgs
                {
                    ConnectionEventType = ConnectionEventType.Disconnect,
                    Socket = socket,
                    ArgsType = ArgsType.Connection,
                    ConnectionType = TcpConnectionType.Disconnect
                });

                if (socket.Connected)
                {
                    _numberOfConnections--;

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                return true;
            }
            catch
            { }
            return false;
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.  
                _allDone.Set();

                // Get the socket that handles the client request.  
                var listener = (Socket)ar.AsyncState;
                var handler = listener.EndAccept(ar);

                _numberOfConnections++;

                // Create the state object.  
                var state = new StateObject
                {
                    WorkSocket = handler
                };

                FireEvent(this, new TcpConnectionEventArgs
                {
                    ConnectionEventType = ConnectionEventType.Connected,
                    Socket = handler,
                    ArgsType = ArgsType.Connection,
                    ConnectionType = TcpConnectionType.Connected
                });

                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch
            {
                Console.WriteLine("Accept callback from TCP error");
            }
        }
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                var state = (StateObject)ar.AsyncState;
                var handler = state.WorkSocket;

                // Read data from the client socket.   
                var bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.Sb.Append(Encoding.UTF8.GetString(
                        state.Buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  
                    while (state.Sb.ToString().IndexOf(_endOfLineCharacters) > -1)
                    {
                        var content = state.Sb.ToString().Substring(0, state.Sb.ToString().IndexOf(_endOfLineCharacters));
                        state.Sb.Remove(0, content.Length + _endOfLineCharacters.Length);
                        // All the data has been read from the   
                        // client. Display it on the console.  

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            PacketDTO packet;

                            try
                            {
                                packet = JsonConvert.DeserializeObject<PacketDTO>(content);
                            }
                            catch
                            {
                                packet = new PacketDTO
                                {
                                    Action = (int)ActionType.SendToServer,
                                    Data = content,
                                    Timestamp = DateTime.UtcNow
                                };
                            }

                            FireEvent(this, new TcpMessageEventArgs
                            {
                                MessageEventType = MessageEventType.Receive,
                                Socket = handler,
                                Message = content,
                                ArgsType = ArgsType.Message,
                                Packet = packet
                            });
                        }
                    }
                }

                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception)
            {
                var state = (StateObject)ar.AsyncState;
                var handler = state.WorkSocket;

                FireEvent(this, new TcpConnectionEventArgs()
                {
                    ConnectionEventType = ConnectionEventType.Disconnect,
                    Socket = handler,
                    ArgsType = ArgsType.Connection,
                    ConnectionType = TcpConnectionType.Disconnect
                });

                DisconnectClient(handler);
                Console.WriteLine("Read callback from Tcp error");
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var socket = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = socket.EndSend(ar);
            }
            catch
            {
                var socket = (Socket)ar.AsyncState;

                DisconnectClient(socket);
                Console.WriteLine("Send callback from Tcp error");
            }
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
        public Socket Socket
        {
            get
            {
                return _connectionSocket;
            }
        }

        public override void Dispose()
        {
            Stop();
            base.Dispose();
        }
    }
}
