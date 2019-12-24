using Tcp.NET.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.Auth.Models;
using Tcp.NET.Server.Auth.Interfaces;

namespace Tcp.NET.Server.SSL.Auth
{
    public class TcpConnectionManagerAuth : TcpConnectionManager, ITcpConnectionManagerAuth
    {
        protected ConcurrentDictionary<int, Socket> _clientsUnauthorized =
            new ConcurrentDictionary<int, Socket>();

        protected ConcurrentDictionary<Guid, IUserConnectionTcpDTO> _clientsAuthorized =
            new ConcurrentDictionary<Guid, IUserConnectionTcpDTO>();

        public IUserConnectionTcpDTO GetIdentity(Guid userId)
        {
            return _clientsAuthorized.TryGetValue(userId, out var clientAuthorized) ? clientAuthorized : (default);
        }
        public IUserConnectionTcpDTO GetIdentity(Socket socket)
        {
            if (_clientsAuthorized.Any(p => p.Value.Connections.Any(t => t.Socket.GetHashCode() == socket.GetHashCode())))
            {
                var client = _clientsAuthorized.Values.FirstOrDefault(s => s.Connections.Any(t => t.Socket.GetHashCode() == socket.GetHashCode()));
                return client;
            }

            return default;
        }
        public ConnectionSocketDTO GetConnectionAuthorized(Socket socket)
        {
            if (_clientsAuthorized.Any(p => p.Value.Connections.Any(t => t.Socket.GetHashCode() == socket.GetHashCode())))
            {
                var client = _clientsAuthorized.Values.FirstOrDefault(s => s.Connections.Any(t => t.Socket.GetHashCode() == socket.GetHashCode()));
                return client.Connections.First(s => s.Socket.GetHashCode() == socket.GetHashCode());
            }

            return default;
        }
        public ICollection<Socket> GetAllSocketsUnauthorized()
        {
            return _clientsUnauthorized.Values.ToList();
        }
        public ICollection<IUserConnectionTcpDTO> GetAllIdentitiesAuthorized()
        {
            return _clientsAuthorized.Values.ToList();
        }

        public bool AddSocketUnauthorized(Socket socket)
        {
            return !_clientsUnauthorized.ContainsKey(socket.GetHashCode()) ? _clientsUnauthorized.TryAdd(socket.GetHashCode(), socket) : false;
        }
        public IUserConnectionTcpDTO AddConnectionAuthorized(Guid userId, Socket socket)
        {
            IUserConnectionTcpDTO client;

            if (_clientsAuthorized.ContainsKey(userId))
            {
                client = _clientsAuthorized.First(s => s.Key == userId).Value;
            }
            else
            {
                client = new UserConnectionTcpDTO
                {
                    UserId = userId,
                    Connections = new List<ConnectionSocketDTO>()
                };
                _clientsAuthorized.TryAdd(userId, client);
            }

            if (!client.Connections.Any(s => s.Socket.GetHashCode() == socket.GetHashCode()))
            {
                client.Connections.Add(new ConnectionSocketDTO
                {
                    Socket = socket
                });

                return client;
            }

            return null;
        }
        public void RemoveSocketUnauthorized(Socket socket, bool isDisconnect)
        {
            if (_clientsUnauthorized.TryGetValue(socket.GetHashCode(), out _))
            {
                _clientsUnauthorized.TryRemove(socket.GetHashCode(), out _);
            }

            if (isDisconnect)
            {
                socket.Close();
                socket.Dispose();
            }
        }
        public void RemoveConnectionAuthorized(ConnectionSocketDTO connection)
        {
            if (_clientsAuthorized.Any(p => p.Value.Connections.Any(t => t.GetHashCode() == connection.GetHashCode())))
            {
                var client = _clientsAuthorized.First(s => s.Value.Connections.Any(t => t.GetHashCode() == connection.GetHashCode())).Value;
                var instance = client.Connections.First(s => s.GetHashCode() == connection.GetHashCode());
                client.Connections.Remove(instance);

                if (!client.Connections.Any())
                {
                    _clientsAuthorized.TryRemove(client.UserId, out client);
                }

                instance.Socket.Close();
                instance.Socket.Dispose();
            }
        }

        public bool IsConnectionUnauthorized(Socket socket)
        {
            return !IsConnectionAuthorized(socket) ? _clientsUnauthorized.ContainsKey(socket.GetHashCode()) : false;
        }
        public bool IsConnectionAuthorized(Socket socket)
        {
            return _clientsAuthorized.Values.Any(s => s.Connections.Any(t => t.Socket.GetHashCode() == socket.GetHashCode()));
        }
        public bool IsUserConnected(Guid userId)
        {
            return _clientsAuthorized.ContainsKey(userId);
        }
    }
}
