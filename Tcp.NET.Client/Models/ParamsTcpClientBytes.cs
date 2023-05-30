using PHS.Networking.Utilities;
using System;
using System.Linq;
using System.Text;

namespace Tcp.NET.Client.Models
{
    public class ParamsTcpClientBytes : IParamsTcpClient
    {
        public string Host { get; protected set; }
        public int Port { get; protected set; }
        public byte[] EndOfLineBytes { get; protected set; }
        public bool UsePingPong { get; protected set; }
        public byte[] PingBytes { get; protected set; }
        public byte[] PongBytes { get; protected set; }
        public bool IsSSL { get; protected set; }
        public bool OnlyEmitBytes { get; protected set; }
        public byte[] Token { get; protected set; }
        public bool UseDisconnectBytes { get; protected set; }
        public byte[] DisconnectBytes { get; protected set; }

        public ParamsTcpClientBytes(string host, int port, byte[] endOfLineBytes, byte[] token = null, bool isSSL = true, bool onlyEmitBytes = true, bool usePingPong = true, byte[] pingBytes = null, byte[] pongBytes = null, bool useDisconnectBytes = true, byte[] disconnectBytes = null)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host is not valid");
            }

            if (port <= 0)
            {
                throw new ArgumentException("Port is not valid");
            }

            if (endOfLineBytes == null || endOfLineBytes.Length <= 0 || Statics.ByteArrayEquals(endOfLineBytes, Array.Empty<byte>()))
            {
                throw new ArgumentException("End of Line Characters are not valid");
            }

            if (token != null && token.Where(x => x != 0).ToArray().Length <= 0)
            {
                throw new ArgumentException("Token is not valid");
            }

            if (usePingPong && (pingBytes == null || pingBytes.Length <= 0 || Statics.ByteArrayEquals(pingBytes, Array.Empty<byte>())))
            {
                pingBytes = Encoding.UTF8.GetBytes("ping");
            }

            if (usePingPong && (pongBytes == null || pongBytes.Length <= 0 || Statics.ByteArrayEquals(pongBytes, Array.Empty<byte>())))
            {
                pongBytes = Encoding.UTF8.GetBytes("pong");
            }

            Host = host;
            Port = port;
            EndOfLineBytes = endOfLineBytes;
            UsePingPong = usePingPong;
            PingBytes = pingBytes;
            PongBytes = pongBytes;
            IsSSL = isSSL;
            OnlyEmitBytes = onlyEmitBytes;
            UseDisconnectBytes = useDisconnectBytes;
            DisconnectBytes = disconnectBytes;
            Token = token;

            if (UseDisconnectBytes && (DisconnectBytes == null || Statics.ByteArrayEquals(DisconnectBytes, Array.Empty<byte>())))
            {
                DisconnectBytes = new byte[] { 3 };
            }
        }
    }
}
