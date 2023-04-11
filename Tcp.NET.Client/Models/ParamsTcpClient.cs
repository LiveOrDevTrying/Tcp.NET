using PHS.Networking.Models;
using System;
using System.Linq;
using System.Text;

namespace Tcp.NET.Client.Models
{
    public class ParamsTcpClient : Params
    {
        public string Host { get; protected set; }
        public int Port { get; protected set; }
        public byte[] EndOfLineBytes { get; protected set; }
        public byte[] PingBytes { get; protected set; }
        public byte[] PongBytes { get; protected set; }
        public bool IsSSL { get; protected set; }
        public bool OnlyEmitBytes { get; protected set; }
        public byte[] Token { get; protected set; }

        public ParamsTcpClient(string host, int port, string endOfLineCharacters, bool isSSL, string token = "", bool onlyEmitBytes = false, string pingCharacters = "ping", string pongCharacters = "pong")
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host is not valid");
            }

            if (port <= 0)
            {
                throw new ArgumentException("Port is not valid");
            }

            if (string.IsNullOrEmpty(endOfLineCharacters))
            {
                throw new ArgumentException("End of Line Characters are not valid");
            }

            if (string.IsNullOrEmpty(pingCharacters))
            {
                throw new ArgumentException("Ping Characters are not valid");
            }

            if (string.IsNullOrEmpty(pongCharacters))
            {
                throw new ArgumentException("Pong Characters are not valid");
            }

            Host = host;
            Port = port;
            EndOfLineBytes = Encoding.UTF8.GetBytes(endOfLineCharacters);
            PingBytes = Encoding.UTF8.GetBytes(pingCharacters);
            PongBytes = Encoding.UTF8.GetBytes(pongCharacters);
            IsSSL = isSSL;
            OnlyEmitBytes = onlyEmitBytes;

            if (!string.IsNullOrWhiteSpace(token))
            {
                Token = Encoding.UTF8.GetBytes(token);
            }
        }
        public ParamsTcpClient(string host, int port, byte[] endOfLineBytes, bool isSSL, byte[] token = null, bool onlyEmitBytes = true, byte[] pingBytes = null, byte[] pongBytes = null)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host is not valid");
            }

            if (port <= 0)
            {
                throw new ArgumentException("Port is not valid");
            }

            if (endOfLineBytes == null || endOfLineBytes.Length <= 0 || endOfLineBytes.All(x => x == 0))
            {
                throw new ArgumentException("End of Line Characters are not valid");
            }

            if (token != null && (token.Length <= 0 || token.All(x => x == 0)))
            {
                throw new ArgumentException("Token is not valid");
            }

            if (pingBytes == null || pingBytes.Length <= 0 || pingBytes.All(x => x == 0))
            {
                pingBytes = Encoding.UTF8.GetBytes("ping");
            }

            if (pongBytes == null || pongBytes.Length <= 0 || pingBytes.All(x => x == 0))
            {
                pongBytes = Encoding.UTF8.GetBytes("pong");
            }

            Host = host;
            Port = port;
            EndOfLineBytes = endOfLineBytes;
            PingBytes = pingBytes;
            PongBytes = pongBytes;
            IsSSL = isSSL;
            OnlyEmitBytes = onlyEmitBytes;
            Token = token;
        }
    }
}
