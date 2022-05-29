using PHS.Networking.Services;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Handlers;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public abstract class TcpNETClientBase<T, U, V, W, X, Y> : 
        CoreNetworkingGeneric<T, U, V>,
        ICoreNetworkingGeneric<T, U, V>
        where T : TcpConnectionClientEventArgs
        where U : TcpMessageClientEventArgs
        where V : TcpErrorClientEventArgs
        where W : ParamsTcpClient
        where X : TcpClientHandlerBase<Y>
        where Y : ConnectionTcp
    {
        protected readonly X _handler;
        protected readonly W _parameters;

        public TcpNETClientBase(W parameters)
        {
            _parameters = parameters;

            _handler = CreateTcpClientHandler();
            _handler.ConnectionEvent += OnConnectionEvent;
            _handler.MessageEvent += OnMessageEvent;
            _handler.ErrorEvent += OnErrorEvent;
        }

        public virtual async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            return await _handler.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual async Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
        {
            return await _handler.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> SendAsync(string message, CancellationToken cancellationToken = default)
        {
            return await _handler.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }
        public virtual async Task<bool> SendAsync(byte[] message, CancellationToken cancellationToken = default)
        {
            return await _handler.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }

        protected abstract void OnConnectionEvent(object sender, TcpConnectionClientEventArgs args);
        protected abstract void OnMessageEvent(object sender, TcpMessageClientEventArgs args);
        protected abstract void OnErrorEvent(object sender, TcpErrorClientEventArgs args);

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
