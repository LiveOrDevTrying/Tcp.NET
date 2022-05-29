using PHS.Networking.Server.Events.Args;
using System;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Models;
using Tcp.NET.Server.Events.Args;
using PHS.Networking.Enums;

namespace Tcp.NET.TestApps.Server
{
    class Program
    {
        private static ITcpNETServerAuth<Guid> _authServer;

        static async Task Main(string[] args)
        {
            _authServer = new TcpNETServerAuth<Guid>(new ParamsTcpServerAuth(8989, "\r\n", "Connected Successfully", "Not authorized"), new MockUserService()); ;
            _authServer.MessageEvent += OnMessageEvent;
            _authServer.ServerEvent += OnServerEvent;
            _authServer.ConnectionEvent += OnConnectionEvent;
            _authServer.ErrorEvent += OnErrorEvent;
            _authServer.Start();

            while (true)
            {
                Console.ReadLine();

                foreach (var item in _authServer.Connections)
                {
                    await _authServer.DisconnectConnectionAsync(item);
                }
            }
        }

        private static void OnErrorEvent(object sender, TcpErrorServerAuthEventArgs<Guid> args)
        {
            Console.WriteLine(args.Message);
        }

        private static void OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<Guid> args)
        {
            Console.WriteLine(args.ConnectionEventType + " " + _authServer.ConnectionCount);
        }

        private static void OnServerEvent(object sender, ServerEventArgs args)
        {
            Console.WriteLine(args.ServerEventType);
        }

        private static void OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<Guid> args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.MessageEventType + ": " + args.Message);

                    Task.Run(async () =>
                    {
                        Console.WriteLine("Connections: " + _authServer.ConnectionCount);
                        await _authServer.BroadcastToAllConnectionsAsync(args.Bytes, args.Connection);
                    });
                    break;
                default:
                    break;
            }
        }
    }
}
