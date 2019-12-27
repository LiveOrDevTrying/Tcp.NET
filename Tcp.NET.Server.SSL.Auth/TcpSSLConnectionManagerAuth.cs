using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tcp.NET.Server.SSL.Auth.Interfaces;
using System.IO;
using Tcp.NET.Server.SSL.Auth.Models;
using Tcp.NET.Server.SSL.Models;

namespace Tcp.NET.Server.SSL.Auth
{
    public class TcpSSLConnectionManagerAuth : TcpSSLConnectionManager, ITcpSSLConnectionManagerAuth
    {
        protected ConcurrentDictionary<int, ConnectionTcpClientSSLDTO> _clientsUnauthorized =
            new ConcurrentDictionary<int, ConnectionTcpClientSSLDTO>();

        protected ConcurrentDictionary<Guid, IUserConnectionTcpClientSSLDTO> _clientsAuthorized =
            new ConcurrentDictionary<Guid, IUserConnectionTcpClientSSLDTO>();

        public IUserConnectionTcpClientSSLDTO GetIdentity(Guid userId)
        {
            return _clientsAuthorized.TryGetValue(userId, out var clientAuthorized) ? clientAuthorized : (default);
        }
        public IUserConnectionTcpClientSSLDTO GetIdentity(TcpClient client)
        {
            if (_clientsAuthorized.Any(p => p.Value.Connections.Any(t => t.Client.GetHashCode() == client.GetHashCode())))
            {
                var connection = _clientsAuthorized.Values.FirstOrDefault(s => s.Connections.Any(t => t.Client.GetHashCode() == client.GetHashCode()));
                return connection;
            }

            return default;
        }
        public IUserConnectionTcpClientSSLDTO GetClientAuthorized(TcpClient client)
        {
            return _clientsAuthorized.Any(p => p.Value.Connections.Any(t => t.Client.GetHashCode() == client.GetHashCode()))
                ? _clientsAuthorized.Values.FirstOrDefault(s => s.Connections.Any(t => t.Client.GetHashCode() == client.GetHashCode()))
                : (default);
        }
        public ConnectionTcpClientSSLDTO GetClientUnauthorized(TcpClient client)
        {
            return _clientsUnauthorized.TryGetValue(client.GetHashCode(), out var clientUnauth) ? clientUnauth : (default);
        }
        public ICollection<ConnectionTcpClientSSLDTO> GetAllClientsUnauthorized()
        {
            return _clientsUnauthorized.Values.ToList();
        }
        public ICollection<IUserConnectionTcpClientSSLDTO> GetAllIdentitiesAuthorized()
        {
            return _clientsAuthorized.Values.ToList();
        }

        public bool AddClientUnauthorized(TcpClient client, StreamReader reader, StreamWriter writer)
        {
            return !_clientsUnauthorized.ContainsKey(client.GetHashCode()) ? _clientsUnauthorized.TryAdd(client.GetHashCode(), new ConnectionTcpClientSSLDTO
            {
                Reader = reader,
                Writer = writer,
                Client = client
            }) : false;
        }
        public IUserConnectionTcpClientSSLDTO AddConnectionAuthorized(Guid userId, TcpClient client, StreamReader reader, StreamWriter writer)
        {
            if (_clientsAuthorized.TryGetValue(userId, out var userConnection))
            {
                userConnection = new UserConnectionTcpClientSSLDTO
                {
                    UserId = userId,
                    Connections = new List<ConnectionTcpClientSSLDTO>()
                };
                _clientsAuthorized.TryAdd(userId, userConnection);
            }

            if (!userConnection.Connections.Any(s => s.Client.GetHashCode() == client.GetHashCode()))
            {
                userConnection.Connections.Add(new ConnectionTcpClientSSLDTO
                {
                    Client = client,
                    Reader = reader,
                    Writer = writer
                });

                return userConnection;
            }

            return null;
        }
        public void RemoveClientUnauthorized(ConnectionTcpClientSSLDTO client, bool isDisconnect)
        {
            if (_clientsUnauthorized.TryRemove(client.GetHashCode(), out var connection))
            {
                if (isDisconnect)
                {
                    connection.Client.Close();
                    connection.Client.Dispose();
                }
            }
        }
        public void RemoveConnectionAuthorized(ConnectionTcpClientSSLDTO connection)
        {
            if (_clientsAuthorized.Any(p => p.Value.Connections.Any(t => t.Client.GetHashCode() == connection.Client.GetHashCode())))
            {
                var client = _clientsAuthorized.First(s => s.Value.Connections.Any(t => t.Client.GetHashCode() == connection.Client.GetHashCode())).Value;
                var instance = client.Connections.First(s => s.GetHashCode() == connection.GetHashCode());
                client.Connections.Remove(instance);

                if (!client.Connections.Any())
                {
                    _clientsAuthorized.TryRemove(client.UserId, out client);
                }

                instance.Client.Close();
                instance.Client.Dispose();
                instance.Reader.Dispose();
                instance.Writer.Dispose();
            }
        }

        public bool IsClientUnauthorized(TcpClient client)
        {
            return !IsClientAuthorized(client) ? _clientsUnauthorized.ContainsKey(client.GetHashCode()) : false;
        }
        public bool IsClientAuthorized(TcpClient client)
        {
            return _clientsAuthorized.Values.Any(s => s.Connections.Any(t => t.Client.GetHashCode() == client.GetHashCode()));
        }
        public bool IsUserConnected(Guid userId)
        {
            return _clientsAuthorized.ContainsKey(userId);
        }
    }
}
