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
            _authServer = new TcpNETServerAuth<Guid>(new ParamsTcpServerAuth
            {
                ConnectionSuccessString = "Connected Successfully",
                EndOfLineCharacters = "\r\n",
                Port = 8989,
                ConnectionUnauthorizedString = "Not authorized"
            }, new MockUserService());
            _authServer.MessageEvent += OnMessageEvent;
            _authServer.ServerEvent += OnServerEvent;
            _authServer.ConnectionEvent += OnConnectionEvent;
            await _authServer.StartAsync();

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static Task OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<Guid> args)
        {
            Console.WriteLine(args.ConnectionEventType);

            return Task.CompletedTask;
        }

        private static Task OnServerEvent(object sender, ServerEventArgs args)
        {
            Console.WriteLine(args.ServerEventType);
            return Task.CompletedTask;
        }

        private static Task OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<Guid> args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.MessageEventType + ": " + args.Message);
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
