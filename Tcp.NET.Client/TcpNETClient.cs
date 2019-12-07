using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public class TcpNETClient : 
        CoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>, 
        ITcpNETClient
    {
        // ManualResetEvent instances signal completion.  
        protected ManualResetEvent _connectDone = new ManualResetEvent(false);
        protected ManualResetEvent _sendDone = new ManualResetEvent(false);
        protected ManualResetEvent _receiveDone = new ManualResetEvent(false);

        protected Socket _connectionSocket;

        public virtual void Connect(string host, int port, string endOfLineCharacters)
        {
            _endOfLineCharacters = endOfLineCharacters;

            var hostInfo = Dns.GetHostEntry(host);
            var canBreakOut = false;

            foreach (var item in hostInfo.AddressList)
            {
                if (!canBreakOut)
                {
                    // Connect to a remote device.  
                    try
                    {
                        // Establish the remote endpoint for the socket.  
                        var remoteEP = new IPEndPoint(item, port);
                        // Create a TCP/IP socket.  
                        _connectionSocket = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);

                        // Connect to the remote endpoint.  
                        _connectionSocket.BeginConnect(remoteEP,
                            new AsyncCallback(ConnectCallback), _connectionSocket);
                        canBreakOut = true;
                        _connectDone.WaitOne();
                    }
                    catch
                    {
                        //throw new ArgumentException("Start client error", ex);
                    }
                }
            }
        }
        public virtual bool SendToServer(string message)
        {
            try
            {
                var byteData = Encoding.UTF8.GetBytes(string.Format("{0}{1}", message, _endOfLineCharacters));

                FireEvent(this, new TcpMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Socket = _connectionSocket,
                    Message = message,
                    Packet = new PacketDTO
                    {
                        Data = message,
                        Action = (int)ActionType.SendToServer,
                        Timestamp = DateTime.UtcNow
                    },
                    ArgsType = ArgsType.Message,
                });

                // Begin sending the data to the remote device.  
                _connectionSocket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), _connectionSocket);
                return true;
            }
            catch
            {
            }

            return false;
        }
        public virtual bool SendToServer(PacketDTO packet)
        {
            try
            {
                var message = JsonConvert.SerializeObject(packet);
                var byteData = Encoding.UTF8.GetBytes(string.Format("{0}{1}", message, _endOfLineCharacters));

                FireEvent(this, new TcpMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Socket = _connectionSocket,
                    Packet = packet,
                    Message = packet.Data,
                    ArgsType = ArgsType.Message,
                });

                // Begin sending the data to the remote device.  
                _connectionSocket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), _connectionSocket);

                return true;
            }
            catch
            {
            }

            return false;
        }
        public virtual bool SendToServerRaw(string message)
        {
            try
            {
                var byteData = Encoding.UTF8.GetBytes(string.Format("{0}{1}", message, _endOfLineCharacters));

                FireEvent(this, new TcpMessageEventArgs
                {
                    MessageEventType = MessageEventType.Sent,
                    Socket = _connectionSocket,
                    Packet = new PacketDTO
                    {
                        Action = (int)ActionType.SendToServer,
                        Data = message,
                        Timestamp = DateTime.UtcNow
                    },
                    Message = message,
                    ArgsType = ArgsType.Message,
                });

                // Begin sending the data to the remote device.  
                _connectionSocket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), _connectionSocket);

                return true;
            }
            catch
            {
            }

            return false;
        }
        public virtual void Disconnect()
        {
            try
            {
                FireEvent(this, new TcpConnectionEventArgs
                {
                    ConnectionEventType = ConnectionEventType.Disconnect,
                    Socket = _connectionSocket,
                    ArgsType = ArgsType.Connection,
                });

                _connectionSocket.Shutdown(SocketShutdown.Both);
                _connectionSocket.Close();
            }
            catch
            {
            }
        }

        protected virtual void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                // Signal that the connection has been made.  
                _connectDone.Set();

                FireEvent(this, new TcpConnectionEventArgs
                {
                    ConnectionEventType = ConnectionEventType.Connected,
                    ArgsType = ArgsType.Connection,
                    Socket = _connectionSocket
                });

                Receive(client);

                _receiveDone.WaitOne();
            }
            catch
            {
            }
        }
        protected virtual void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                var state = new StateObject
                {
                    WorkSocket = client
                };

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch
            {
            }
        }
        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                var state = (StateObject)ar.AsyncState;
                var handler = state.WorkSocket;

                // Read data from the remote device.  
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
                            try
                            {
                                var packet = JsonConvert.DeserializeObject<PacketDTO>(content);

                                FireEvent(this, new TcpMessageEventArgs
                                {
                                    MessageEventType = MessageEventType.Receive,
                                    Socket = handler,
                                    Message = packet.Data,
                                    ArgsType = ArgsType.Message,
                                    Packet = packet
                                });
                            }
                            catch
                            {
                                FireEvent(this, new TcpMessageEventArgs
                                {
                                    MessageEventType = MessageEventType.Receive,
                                    Socket = handler,
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

                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch
            {
                var state = (StateObject)ar.AsyncState;
                var handler = state.WorkSocket;

                FireEvent(this, new TcpConnectionEventArgs()
                {
                    ConnectionEventType = ConnectionEventType.Disconnect,
                    Socket = handler,
                    ArgsType = ArgsType.Connection
                });
            }
        }
        protected virtual void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                var bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.  
                _sendDone.Set();

                // Receive(client);
                // _receiveDone.WaitOne();
            }
            catch
            {
            }
        }

        public bool IsConnected
        {
            get
            {
                return _connectionSocket.Connected;
            }
        }

        public Socket Socket
        {
            get
            {
                return _connectionSocket;
            }
        }
    }
}