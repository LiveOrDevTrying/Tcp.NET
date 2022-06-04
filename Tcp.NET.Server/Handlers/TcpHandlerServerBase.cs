using PHS.Networking.Enums;
using PHS.Networking.Server.Handlers;
using PHS.Networking.Utilities;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Handlers
{
    public abstract class TcpHandlerServerBase<T, U, V, W, Z> :
        TcpHandlerServerBaseTcp<T, U, V, W, Z>
        where T : TcpConnectionServerBaseEventArgs<Z>
        where U : TcpMessageServerBaseEventArgs<Z>
        where V : TcpErrorServerBaseEventArgs<Z>
        where W : ParamsTcpServer
        where Z : ConnectionTcpServer
    {
        public TcpHandlerServerBase(W parameters) : base(parameters)
        {
        }
        public TcpHandlerServerBase(W parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        public override async Task<bool> SendAsync(string message, Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && _isRunning)
                {
                    var bytes = Statics.ByteArrayAppend(Encoding.UTF8.GetBytes(message), _parameters.EndOfLineBytes);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = message,
                        Bytes = bytes
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));

                await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public override async Task<bool> SendAsync(byte[] message, Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection.TcpClient.Connected && _isRunning)
                {
                    var bytes = Statics.ByteArrayAppend(message, _parameters.EndOfLineBytes);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                    {
                        MessageEventType = MessageEventType.Sent,
                        Connection = connection,
                        Message = null,
                        Bytes = bytes
                    }));

                    return true;
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));

                await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        public override Task<bool> DisconnectConnectionAsync(Z connection, CancellationToken cancellationToken)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.TcpClient != null &&
                        !connection.Disposed)
                    {
                        connection.Disposed = true;
                        connection.TcpClient.Close();
                        connection.TcpClient.Dispose();

                        FireEvent(this, CreateConnectionEventArgs(new TcpConnectionServerBaseEventArgs<Z>
                        {
                            ConnectionEventType = ConnectionEventType.Disconnect,
                            Connection = connection
                        }));
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));
            }

            return Task.FromResult(false);
        }

        protected override async Task ReceiveAsync(Z connection, CancellationToken cancellationToken) 
        {
            try
            {
                using (var persistantMS = new MemoryStream())
                {
                    while (connection.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                    {
                        using (var ms = new MemoryStream())
                        {
                            if (persistantMS.Length > 0)
                            {
                                persistantMS.CopyTo(ms);
                                persistantMS.SetLength(0);
                            }

                            var endOfMessage = false;
                            do
                            {
                                if (connection.TcpClient.Available <= 0)
                                {
                                    await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                                    continue;
                                }

                                var buffer = new ArraySegment<byte>(new byte[connection.TcpClient.Available]);
                                var result = await connection.TcpClient.Client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                                await ms.WriteAsync(buffer.Array, buffer.Offset, result, cancellationToken).ConfigureAwait(false);

                                endOfMessage = Statics.ByteArrayContainsSequence(ms.ToArray(), _parameters.EndOfLineBytes);
                            }
                            while (!endOfMessage && connection.TcpClient.Connected);

                            if (endOfMessage)
                            {
                                var parts = Statics.ByteArraySeparate(ms.ToArray(), _parameters.EndOfLineBytes);

                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (parts.Length > 1 && i == parts.Length - 1)
                                    {
                                        await persistantMS.WriteAsync(parts[i], cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        if (Statics.ByteArrayEquals(parts[i], _parameters.PongBytes))
                                        {
                                            connection.HasBeenPinged = false;
                                        }
                                        else
                                        {
                                            string message = null;

                                            if (!_parameters.OnlyEmitBytes)
                                            {
                                                message = Encoding.UTF8.GetString(parts[i]);
                                            }

                                            FireEvent(this, CreateMessageEventArgs(new TcpMessageServerBaseEventArgs<Z>
                                            {
                                                Connection = connection,
                                                Message = message,
                                                MessageEventType = MessageEventType.Receive,
                                                Bytes = parts[i]
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                {
                    Exception = ex,
                    Message = ex.Message,
                    Connection = connection
                }));
            }

            await DisconnectConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        }

        protected abstract U CreateMessageEventArgs(TcpMessageServerBaseEventArgs<Z> args);
    }
}