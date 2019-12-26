using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Core.Enums;
using Tcp.NET.Server.Handlers;

namespace Tcp.NET.Server
{
    public class TcpNETServer : 
        CoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>, 
        ITcpNETServer
    {
        protected readonly TcpHandler _handler;
        protected readonly ITcpConnectionManager _connectionManager;
        protected readonly IParamsTcpServer _parameters;

        protected Timer _timerPing;

        public TcpNETServer(IParamsTcpServer parameters,
            ITcpConnectionManager connectionManager)
        {
            _parameters = parameters;
            _connectionManager = connectionManager;

            _handler = new TcpHandler();
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEventAsync;
            _handler.ErrorEvent += OnErrorEvent;
            _handler.Start(_parameters.Url, _parameters.Port, _parameters.EndOfLineCharacters);
        }

        public virtual bool SendToConnection(PacketDTO packet, Socket socket)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    socket.Connected)
                {
                    _handler.Send(packet, socket);

                    FireEvent(this, new TcpMessageEventArgs
                    {
                        Message = JsonConvert.SerializeObject(packet),
                        MessageEventType = MessageEventType.Sent,
                        ArgsType = ArgsType.Message,
                        Packet = packet,
                        Socket = socket
                    });

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual bool SendToConnectionRaw(string message, Socket socket)
        {
            try
            {
                if (_handler != null &&
                    _handler.IsServerRunning &&
                    socket.Connected)
                {
                    if (_connectionManager.IsConnectionOpen(socket))
                    {
                        _handler.SendRaw(message, socket);

                        FireEvent(this, new TcpMessageEventArgs
                        {
                            Message = message,
                            MessageEventType = MessageEventType.Sent,
                            Socket = socket,
                            ArgsType = ArgsType.Message,
                            Packet = new PacketDTO
                            {
                                Action = (int)ActionType.SendToClient,
                                Data = message,
                                Timestamp = DateTime.UtcNow
                            },
                        });

                        return true;
                    }
                }
            }
            catch
            { }

            return false;
        }

        public virtual bool DisconnectClient(Socket socket)
        {
            return _handler.DisconnectClient(socket);
        }

        protected virtual Task OnConnectionEvent(object sender, TcpConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    if (_connectionManager.AddConnection(args.Socket))
                    {
                        FireEvent(this, new TcpConnectionEventArgs
                        {
                            ConnectionType = TcpConnectionType.Connected,
                            ConnectionEventType = args.ConnectionEventType,
                            Socket = args.Socket,
                            ArgsType = ArgsType.Connection,
                        });
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.RemoveConnection(args.Socket, true);

                    FireEvent(this, new TcpConnectionEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.Disconnect,
                        ArgsType = ArgsType.Connection,
                    });
                    break;
                case ConnectionEventType.ServerStart:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    _timerPing = new Timer(OnTimerPingTick, null, _parameters.PingIntervalSec * 1000, _parameters.PingIntervalSec * 1000);

                    FireEvent(this, new TcpConnectionEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.ServerStart,
                        ArgsType = ArgsType.Connection
                    });
                    break;
                case ConnectionEventType.ServerStop:
                    if (_timerPing != null)
                    {
                        _timerPing.Dispose();
                        _timerPing = null;
                    }

                    FireEvent(this, new TcpConnectionEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.ServerStop,
                        ArgsType = ArgsType.Connection,
                    });

                    Thread.Sleep(5000);
                    _handler.Start(_parameters.Url, _parameters.Port, _parameters.EndOfLineCharacters);
                    break;
                case ConnectionEventType.Connecting:
                    FireEvent(this, new TcpConnectionEventArgs
                    {
                        Socket = args.Socket,
                        ConnectionEventType = args.ConnectionEventType,
                        ConnectionType = TcpConnectionType.Connecting,
                        ArgsType = ArgsType.Connection
                    });
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        protected virtual Task OnMessageEventAsync(object sender, TcpMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (_connectionManager.IsConnectionOpen(args.Socket))
                    {
                        var connection = _connectionManager.GetConnection(args.Socket);

                        // Digest the pong first
                        if (args.Message.ToLower().Trim() == "pong" ||
                            args.Packet.Data.Trim().ToLower() == "pong")
                        {
                            connection.HasBeenPinged = false;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(args.Message))
                            {

                                try
                                {
                                    var packet = JsonConvert.DeserializeObject<PacketDTO>(args.Message);

                                    FireEvent(this, new TcpMessageEventArgs
                                    {
                                        Message = packet.Data,
                                        MessageEventType = MessageEventType.Receive,
                                        ArgsType = ArgsType.Message,
                                        Socket = args.Socket,
                                        Packet = packet
                                    });
                                }
                                catch
                                {
                                    FireEvent(this, new TcpMessageEventArgs
                                    {
                                        Message = args.Message,
                                        MessageEventType = MessageEventType.Receive,
                                        ArgsType = ArgsType.Message,
                                        Packet = new PacketDTO
                                        {
                                            Action = (int)ActionType.SendToServer,
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        },
                                        Socket = Socket
                                    });
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        protected virtual Task OnErrorEvent(object sender, TcpErrorEventArgs args)
        {
            if (_connectionManager.IsConnectionOpen(args.Socket))
            {
                var identity = _connectionManager.GetConnection(args.Socket);

                FireEvent(this, new TcpErrorEventArgs
                {
                    Exception = args.Exception,
                    Message = args.Message,
                    Socket = args.Socket,
                    ArgsType = ArgsType.Error,
                });
            }
            return Task.CompletedTask;
        }
        protected virtual void OnTimerPingTick(object state)
        {
            foreach (var connection in _connectionManager.GetAllConnections())
            {
                var connectionsToRemove = new List<ConnectionSocketDTO>();

                if (connection.HasBeenPinged)
                {
                    // Already been pinged, no response, disconnect
                    connectionsToRemove.Add(connection);
                }
                else
                {
                    connection.HasBeenPinged = true;
                    _handler.Send("Ping", connection.Socket);
                }

                foreach (var connectionToRemove in connectionsToRemove)
                {
                    _connectionManager.RemoveConnection(connectionToRemove.Socket, true);
                    _handler.SendRaw("No ping response - disconnected.", connectionToRemove.Socket);
                    _handler.DisconnectClient(connectionToRemove.Socket);
                }
            }
        }

        protected virtual int GetRateLimit()
        {
            // 15000 messages each 30000 MS with 80% of total time to buffer
            return Convert.ToInt32(Math.Ceiling(30000f / 15000f * 0.8f));
        }

        public Socket Socket
        {
            get
            {
                return _handler?.Socket;
            }
        }
        public bool IsServerRunning
        {
            get
            {
                return _handler != null ? _handler.IsServerRunning : false;
            }
        }
        public TcpHandler TcpHandler
        {
            get
            {
                return _handler;
            }
        }

        public override void Dispose()
        {
            foreach (var item in _connectionManager.GetAllConnections())
            {
                _connectionManager.RemoveConnection(item.Socket, true);
            }

            if (_handler != null)
            {
                _handler.ConnectionEvent -= OnConnectionEvent;
                _handler.MessageEvent -= OnMessageEventAsync;
                _handler.ErrorEvent -= OnErrorEvent;
                _handler.Dispose();
            }

            if (_timerPing != null)
            {
                _timerPing.Dispose();
                _timerPing = null;
            }
            base.Dispose();
        }
    }
}
