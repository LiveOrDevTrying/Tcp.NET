using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client.Handlers
{
    public class TcpClientHandler : 
        TcpClientHandlerBase<
            TcpConnectionClientEventArgs,
            TcpMessageClientEventArgs,
            TcpErrorClientEventArgs,
            ParamsTcpClient,
            ConnectionTcp>
    {
        public TcpClientHandler(ParamsTcpClient parameters) : base(parameters)
        {
        }

        protected override ConnectionTcp CreateConnection(ConnectionTcp connection)
        {
            return connection;
        }

        protected override TcpConnectionClientEventArgs CreateConnectionEventArgs(TcpConnectionEventArgs<ConnectionTcp> args)
        {
            return new TcpConnectionClientEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType,
                CancellationToken = args.CancellationToken
            };
        }

        protected override TcpErrorClientEventArgs CreateErrorEventArgs(TcpErrorEventArgs<ConnectionTcp> args)
        {
            return new TcpErrorClientEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message,
                CancellationToken = args.CancellationToken
            };
        }

        protected override TcpMessageClientEventArgs CreateMessageEventArgs(TcpMessageEventArgs<ConnectionTcp> args)
        {
            return new TcpMessageClientEventArgs
            {
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType,
                CancellationToken = args.CancellationToken
            };
        }
    }
}
