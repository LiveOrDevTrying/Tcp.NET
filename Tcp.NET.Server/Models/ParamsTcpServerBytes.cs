using PHS.Networking.Models;
using PHS.Networking.Utilities;
using System;
using System.Linq;
using System.Text;

namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServerBytes : IParamsTcpServer
    {
        public int Port { get; protected set; }
        public byte[] EndOfLineBytes { get; protected set; }
        public byte[] PingBytes { get; protected set; }
        public byte[] PongBytes { get; protected set; }
        public string ConnectionSuccessString { get; protected set; }
        public int PingIntervalSec { get; protected set; }
        public bool OnlyEmitBytes { get; protected set; }
        public bool UseDisconnectBytes { get; protected set; }
        public byte[] DisconnectBytes { get; protected set; }

        public ParamsTcpServerBytes(int port, byte[] endOfLineBytes, string connectionSuccessString = null, bool onlyEmitBytes = false, int pingIntervalSec = 120, byte[] pingBytes = null, byte[] pongBytes = null, bool useDisconnectBytes = true, byte[] disconnectBytes = null) : base()
        {
            if (port <= 0)
            {
                throw new ArgumentException("Port is not valid");
            }

            if (endOfLineBytes.Length <= 0 || endOfLineBytes.All(x => x == 0))
            {
                throw new ArgumentException("End of Line Characters are not valid");
            }

            if (pingBytes == null || pingBytes.Length <= 0 || pingBytes.All(x => x == 0))
            {
                pingBytes = Encoding.UTF8.GetBytes("ping");
            }

            if (pongBytes == null || pongBytes.Length <= 0 || pingBytes.All(x => x == 0))
            {
                pongBytes = Encoding.UTF8.GetBytes("pong");
            }

            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionSuccessString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true is a connectionSuccesString is specified");
            }

            Port = port;
            EndOfLineBytes = endOfLineBytes;
            ConnectionSuccessString = connectionSuccessString;
            PingBytes = pingBytes;
            PongBytes = pongBytes;
            PingIntervalSec = pingIntervalSec;
            OnlyEmitBytes = onlyEmitBytes;
            UseDisconnectBytes = useDisconnectBytes;
            DisconnectBytes = disconnectBytes;

            if (UseDisconnectBytes && (DisconnectBytes == null || Statics.ByteArrayEquals(DisconnectBytes, Array.Empty<byte>())))
            {
                DisconnectBytes = new byte[] { 3 };
            }
        }
    }
}
