using IdentityModel.Client;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Enums;
using Tcp.NET.Core.Events.Args;

namespace Tcp.NET.Client
{
    public class TcpAsyncClientAuth : 
        CoreNetworking<TcpConnectionEventArgs, TcpMessageEventArgs, TcpErrorEventArgs>, 
        ITcpAsyncClientAuth
    {
        protected readonly ITcpAsyncClient _client;
        protected readonly ParamsTcpClient _parameters;
        protected TokenResponse _tokenResponse;

        public TcpAsyncClientAuth(ParamsTcpClient parameters)
        {
            _parameters = parameters;
            _client = new TcpAsyncClient();
            _client.ConnectionEvent += OnConnectionEventAsync;
            _client.MessageEvent += OnMessageEvent;
            _client.ErrorEvent += OnErrorEvent;

            Task.Run(async () =>
            {
                await GetTokenAndStartClientAsync();
            });
        }

        public virtual bool SendToServer(string message)
        {
            try
            {
                if (_client.IsConnected)
                {
                    var packet = new PacketDTO
                    {
                        Action = (int)ActionType.SendToServer,
                        Data = message,
                        Timestamp = DateTime.UtcNow
                    };

                    _client.SendToServer(packet);

                    FireEvent(this, new TcpMessageEventArgs
                    {
                        Message = message,
                        MessageEventType = MessageEventType.Sent,
                        ArgsType = ArgsType.Message,
                        Packet = packet,
                        Socket = _client.Socket,
                    });

                    return true;
                }
            }
            catch
            { }

            return false;
        }
        public virtual bool SendToServerRaw(string message)
        {
            try
            {
                if (_client.IsConnected)
                {
                    _client.SendToServerRaw(message);

                    FireEvent(this, new TcpMessageEventArgs
                    {
                        Message = message,
                        MessageEventType = MessageEventType.Sent,
                        ArgsType = ArgsType.Message,
                        Packet = new PacketDTO
                        {
                            Action = (int)ActionType.SendToServer,
                            Data = message,
                            Timestamp = DateTime.UtcNow
                        }
                    });

                    return true;
                }
            }
            catch
            { }

            return false;
        }

        protected virtual async Task GetTokenAndStartClientAsync()
        {
            var isBreak = false;

            do
            {
                using (var client = new HttpClient())
                {
                    var disco = await client.GetDiscoveryDocumentAsync(_parameters.AuthConfig.AuthorityUrl);

                    _tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = _parameters.AuthConfig.ClientId,
                        ClientSecret = _parameters.AuthConfig.ClientSecret,
                        UserName = _parameters.AuthConfig.Username,
                        Password = _parameters.AuthConfig.Password,
                        Scope = _parameters.AuthConfig.Scope
                    });
                }

                if (_tokenResponse != null &&
                    !_tokenResponse.IsError)
                {
                    _client.Start(_parameters.Url, _parameters.Port, _parameters.EndOfLineCharacters);
                    isBreak = true;
                }
                else
                {
                    Thread.Sleep(_parameters.IntervalReconnectSec);
                }
            }
            while (!isBreak);
        }

        protected virtual async Task OnConnectionEventAsync(object sender, TcpConnectionEventArgs e)
        {
            switch (e.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(_tokenResponse.AccessToken));

                    FireEvent(sender, new TcpConnectionEventArgs
                    {
                        ArgsType = ArgsType.Connection,
                        ConnectionEventType = e.ConnectionEventType,
                        Socket = e.Socket,
                        ConnectionType = TcpConnectionType.AuthorizationClient,
                    });

                    _client.SendToServer($"oauth:{token}");
                    break;
                case ConnectionEventType.Disconnect:
                    FireEvent(sender, new TcpConnectionEventArgs
                    {
                        ArgsType = ArgsType.Connection,
                        ConnectionEventType = ConnectionEventType.Disconnect,
                        Socket = e.Socket,
                        ConnectionType = TcpConnectionType.Disconnect
                    });

                    Thread.Sleep(_parameters.IntervalReconnectSec);

                    FireEvent(sender, new TcpConnectionEventArgs
                    {
                        ArgsType = ArgsType.Connection,
                        ConnectionEventType = ConnectionEventType.Connecting,
                        Socket = e.Socket,
                        ConnectionType = TcpConnectionType.Connecting
                    });

                    await GetTokenAndStartClientAsync();
                    break;
                case ConnectionEventType.ServerStart:
                    break;
                case ConnectionEventType.ServerStop:
                    break;
                case ConnectionEventType.Connecting:
                    break;
                case ConnectionEventType.MaxConnectionsReached:
                    break;
                default:
                    break;
            }
        }
        protected virtual Task OnMessageEvent(object sender, TcpMessageEventArgs e)
        {
            if (e.Message.Trim().ToLower() == "ping")
            {
                SendToServerRaw("pong");
            }
            else
            {
                FireEvent(sender, new TcpMessageEventArgs
                {
                    ArgsType = ArgsType.Message,
                    Message = e.Message,
                    MessageEventType = e.MessageEventType,
                    Socket = e.Socket,
                    Packet = e.Packet,
                });
            }

            return Task.CompletedTask;
        }
        protected virtual Task OnErrorEvent(object sender, TcpErrorEventArgs e)
        {
            FireEvent(sender, new TcpErrorEventArgs
            {
                ArgsType = ArgsType.Error,
                Exception = e.Exception,
                Message = e.Message,
                Socket = e.Socket,
            });

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if (_client != null)
            {
                _client.Stop();
                _client.ConnectionEvent -= OnConnectionEventAsync;
                _client.MessageEvent -= OnMessageEvent;
                _client.ErrorEvent -= OnErrorEvent;
                _client.Dispose();
            }

            base.Dispose();
        }

        public bool IsConnected
        {
            get
            {
                return _client != null && _client.IsConnected;
            }
        }
    }
}
