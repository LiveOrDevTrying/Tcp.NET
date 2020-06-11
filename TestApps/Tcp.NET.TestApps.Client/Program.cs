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
        private static List<ITcpNETClient> _clients =
            new List<ITcpNETClient>();
        private static int _temp;

        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                Task.Run(async () =>
                {
                    for (int k = 0; k < 1000; k++)
                    {
                        using (var client = new TcpNETClient(new ParamsTcpClient
                        {
                            EndOfLineCharacters = "\r\n",
                            Port = 8989,
                            Uri = "localhost",
                            IsSSL = false,
                        }, oauthToken: "faketoken"))
                        {
                            client.MessageEvent += OnMessageEvent;
                            client.ConnectionEvent += OnConnectionEvent;
                            client.ErrorEvent += OnErrorEvent;
                            await client.ConnectAsync();
                            await client.SendToServerRawAsync("From client iteration: " + ++_temp);
                            _clients.Add(client);
                            Console.WriteLine("Iteration: " + _temp);
                        }
                    };
                });
            }

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static Task OnErrorEvent(object sender, TcpErrorClientEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static Task OnConnectionEvent(object sender, TcpConnectionClientEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static Task OnMessageEvent(object sender, TcpMessageClientEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.Message);
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
