using PHS.Networking.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tcp.NET.Client;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Models;

namespace Tcp.NET.TestApps.Client
{
    class Program
    {
        private static ITcpNETClient _client;

        static async Task Main(string[] args)
        {
            _client = new TcpNETClient(new ParamsTcpClient
            {
                EndOfLineCharacters = "\r\n",
                Port = 8989,
                Uri = "localhost",
                IsSSL = false,
            }, oauthToken: "faketoken");

            _client.MessageEvent += OnMessageEvent;
            _client.ConnectionEvent += OnConnectionEvent;
            _client.ErrorEvent += OnErrorEvent;
            await _client.ConnectAsync();
            await _client.SendToServerRawAsync("Hello world");

            while (true)
            {
                var line = Console.ReadLine();
                await _client.SendToServerAsync(line);
            }
        }

        private static void OnErrorEvent(object sender, TcpErrorClientEventArgs args)
        {
            Console.WriteLine(args.Message);
        }

        private static void OnConnectionEvent(object sender, TcpConnectionClientEventArgs args)
        {
            Console.WriteLine(args.ConnectionEventType.ToString());
        }

        private static void OnMessageEvent(object sender, TcpMessageClientEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.Packet.Data);
                    break;
                default:
                    break;
            }
        }
    }
}
