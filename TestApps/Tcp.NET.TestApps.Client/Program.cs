using PHS.Networking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Models;

namespace Tcp.NET.TestApps.Client
{
    class Program
    {
        private static List<ITcpNETClient> _clients = new List<ITcpNETClient>();
        private static Timer _timer;
        private static int _max;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter numbers of users per minute:");
            var line = Console.ReadLine();
            var numberUsers = 0;
            while (!int.TryParse(line, out numberUsers))
            {
                Console.WriteLine("Invalid. Input an int:");
                line = Console.ReadLine();
            }

            Console.WriteLine("Enter max number of users:");
            line = Console.ReadLine();
            _max = 0;
            while (!int.TryParse(line, out _max))
            {
                Console.WriteLine("Invalid. Input an int:");
                line = Console.ReadLine();
            }

            Console.WriteLine("Push any key to start");

            Console.ReadLine();

            _timer = new Timer(OnTimerTick, null, 0, CalculateNumberOfUsersPerMinute(numberUsers));

            while (true)
            {
                line = Console.ReadLine();

                if (line == "restart")
                {
                    var clients = _clients.ToList();
                    _clients = new List<ITcpNETClient>();
                    foreach (var item in clients)
                    {
                        await item.DisconnectAsync();
                    }
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(line + "\r\n");
                    await _clients.First().Connection.Client.Client.SendAsync(new ArraySegment<byte>(bytes), System.Net.Sockets.SocketFlags.None);
                    //await _clients.ToList().Where(x => x.IsRunning).OrderBy(x => Guid.NewGuid()).First().Connection.Client.Client.SendAsync(new ArraySegment<byte>(bytes), System.Net.Sockets.SocketFlags.Broadcast);
                }
            }
        }

        private static void OnTimerTick(object state)
        {
            if (_clients.Count < _max)
            {
                var client = new TcpNETClient(new ParamsTcpClient
                {
                    EndOfLineCharacters = "\r\n",
                    Port = 8989,
                    Host = "localhost",
                    IsSSL = false
                }, "testToken");
                client.ConnectionEvent += OnConnectionEvent;
                client.MessageEvent += OnMessageEvent;
                client.ErrorEvent += OnErrorEvent;
                _clients.Add(client);

                Task.Run(async () => await client.ConnectAsync());
            }
        }
        private static void OnErrorEvent(object sender, TcpErrorClientEventArgs args)
        {
            Console.WriteLine(args.Message);
        }
        private static void OnConnectionEvent(object sender, TcpConnectionClientEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    break;
                case ConnectionEventType.Disconnect:
                    var client = (ITcpNETClient)sender;
                    _clients.Remove(client);

                    client.ConnectionEvent -= OnConnectionEvent;
                    client.MessageEvent -= OnMessageEvent;
                    client.ErrorEvent -= OnErrorEvent;

                    client.Dispose();
                    break;
                default:
                    break;
            }
        }
        private static void OnMessageEvent(object sender, TcpMessageClientEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    Console.WriteLine(args.Message + " : " + +_clients.Where(x => x != null && x.IsRunning).Count());
                    break;
                default:
                    break;
            }
        }

        static int CalculateNumberOfUsersPerMinute(int numberUsers)
        {
            return 60000 / numberUsers;
        }
    }
}
