using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Handlers;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace Tcp.NET.Client
{
    public class TcpNETClient :
        TcpNETClientBase<
            TcpConnectionClientEventArgs,
            TcpMessageClientEventArgs,
            TcpErrorClientEventArgs,
            ParamsTcpClient,
            TcpClientHandler,
            ConnectionTcp>,
        ITcpNETClient
    {
        public TcpNETClient(ParamsTcpClient parameters) : base(parameters)
        {
        }

        protected override void OnConnectionEvent(object sender, TcpConnectionClientEventArgs args)
        {
            FireEvent(this, new TcpConnectionClientEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            });
        }
        protected override void OnMessageEvent(object sender, TcpMessageClientEventArgs args)
        {
            FireEvent(this, new TcpMessageClientEventArgs
            {
                Connection = args.Connection,
                Bytes = args.Bytes,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            });
        }
        protected override void OnErrorEvent(object sender, TcpErrorClientEventArgs args)
        {
            FireEvent(this, new TcpErrorClientEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            });
        }

        protected override TcpClientHandler CreateTcpClientHandler()
        {
            return new TcpClientHandler(_parameters);
        }
    }
}
