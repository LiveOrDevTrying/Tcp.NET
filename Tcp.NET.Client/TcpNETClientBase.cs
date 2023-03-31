using PHS.Networking.Services;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client.Handlers;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public abstract class TcpNETClientBase<T, U, V, W, X, Y> : 
        CoreNetworkingGeneric<T, U, V, W, Y>,
        ICoreNetworkingClient<T, U, V, Y>
        where T : TcpConnectionEventArgs<Y>
        where U : TcpMessageEventArgs<Y>
        where V : TcpErrorEventArgs<Y>
        where W : ParamsTcpClient
        where X : TcpClientHandlerBase<T, U, V, W, Y>
        where Y : ConnectionTcp
    {
        protected readonly X _handler;

        public TcpNETClientBase(W parameters) : base(parameters)
        {
            _handler = CreateTcpClientHandler();
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEvent;
            _handler.ErrorEvent += OnErrorEvent;
        }

        public virtual async Task<bool> ConnectAsync(CancellationToken? cancellationToken = default)
        {
            if (cancellationToken.HasValue)
            {
                return await _handler.ConnectAsync(cancellationToken.Value).ConfigureAwait(false);
            }

            return await _handler.ConnectAsync(default(CancellationToken)).ConfigureAwait(false);
        }
        public virtual async Task<bool> DisconnectAsync(CancellationToken? cancellationToken = default)
        {
            if (cancellationToken.HasValue)
            {
                return await _handler.DisconnectAsync(cancellationToken.Value).ConfigureAwait(false);
            }

            return await _handler.DisconnectAsync(default(CancellationToken)).ConfigureAwait(false);
        }

        public virtual async Task<bool> SendAsync(string message, CancellationToken? cancellationToken = default)
        {
            if (cancellationToken.HasValue)
            {
                return await _handler.SendAsync(message, cancellationToken.Value).ConfigureAwait(false);
            }

            return await _handler.SendAsync(message, default(CancellationToken)).ConfigureAwait(false);
        }
        public virtual async Task<bool> SendAsync(byte[] message, CancellationToken? cancellationToken = default)
        {
            if (cancellationToken.HasValue)
            {
                return await _handler.SendAsync(message, cancellationToken.Value).ConfigureAwait(false);
            }

            return await _handler.SendAsync(message, default(CancellationToken)).ConfigureAwait(false);
        }

        protected virtual void OnConnectionEvent(object sender, T args)
        {
            FireEvent(sender, args);
        }
        protected virtual void OnMessageEvent(object sender, U args)
        {
            FireEvent(sender, args);
        }
        protected virtual void OnErrorEvent(object sender, V args)
        {
            FireEvent(sender, args);
        }

        protected abstract X CreateTcpClientHandler();

        public override void Dispose()
        {
            if (_handler != null)
            {
                _handler.ConnectionEvent -= OnConnectionEvent;
                _handler.MessageEvent -= OnMessageEvent;
                _handler.ErrorEvent -= OnErrorEvent;
                _handler.Dispose();
            }
        }

        public bool IsRunning
        {
            get
            {
                return _handler.Connection != null &&
                    _handler.Connection.TcpClient != null &&
                    _handler.Connection.TcpClient.Connected;
            }
        }
        public Y Connection
        {
            get
            {
                return _handler.Connection;
            }
        }
    }
}
