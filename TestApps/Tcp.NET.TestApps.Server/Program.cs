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
        //private static ITcpNETServerAuth<Guid> _authServer;
        private static ITcpNETServer _authServer;

        static void Main(string[] args)
        {
            //_authServer = new TcpNETServerAuth<Guid>(new ParamsTcpServerAuth
            //{
            //    ConnectionSuccessString = "Connected Successfully",
            //    EndOfLineCharacters = "\r\n",
            //    Port = 8989,
            //    ConnectionUnauthorizedString = "Not authorized"
            //}, new MockUserService());
            //_authServer.MessageEvent += OnMessageEvent;
            //_authServer.ServerEvent += OnServerEvent;
            //_authServer.ConnectionEvent += OnConnectionEvent;

            _authServer = new TcpNETServer(new ParamsTcpServer
            {
                ConnectionSuccessString = "Connected Successfully",
                EndOfLineCharacters = "\r\n",
                Port = 8989,
            });
            _authServer.MessageEvent += OnMessageEvent;
            _authServer.ServerEvent += OnServerEvent;
            _authServer.ConnectionEvent += OnConnectionEvent;
            while (true)
            {
                Console.ReadLine();
            }
        }

        private static Task OnConnectionEvent(object sender, TcpConnectionServerEventArgs args)
        {
            Console.WriteLine(args.ConnectionEventType);
            return Task.CompletedTask;
        }

        private static async Task OnMessageEvent(object sender, TcpMessageServerEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.MessageEventType + ": " + args.Message);
                    await _authServer.SendToConnectionAsync(args.Packet, args.Connection);
                    break;
                default:
                    break;
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

        private static async Task OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<Guid> args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.MessageEventType + ": " + args.Message);
                    await _authServer.SendToConnectionAsync(args.Packet, args.Connection);
                    break;
                default:
                    break;
            }
        }
    }
}
