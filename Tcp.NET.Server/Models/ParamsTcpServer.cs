using PHS.Networking.Models;
using System;
using System.Linq;
using System.Text;

namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServer : ParamsPort
    {
        public byte[] EndOfLineBytes { get; protected set; }
        public byte[] PingBytes { get; protected set; }
        public byte[] PongBytes { get; protected set; }
        public string ConnectionSuccessString { get; protected set; }
        public int PingIntervalSec { get; protected set; }
        public bool OnlyEmitBytes { get; protected set; }

        public ParamsTcpServer(int port, string endOfLineCharacters, string connectionSuccessString = null, int pingIntervalSec = 120, bool onlyEmitBytes = false, string pingCharacters = "ping", string pongCharacters = "pong") : base(port)
        {
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

            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionSuccessString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true is a connectionSuccesString is specified");
            }

            EndOfLineBytes = Encoding.UTF8.GetBytes(endOfLineCharacters);
            PingBytes = Encoding.UTF8.GetBytes(pingCharacters);
            PongBytes = Encoding.UTF8.GetBytes(pongCharacters);
            ConnectionSuccessString = connectionSuccessString;
            PingIntervalSec = pingIntervalSec;
            OnlyEmitBytes = onlyEmitBytes;
        }

        public ParamsTcpServer(int port, byte[] endOfLineBytes, string connectionSuccessString = null, int pingIntervalSec = 120, bool onlyEmitBytes = false, byte[] pingBytes = null, byte[] pongBytes = null) : base(port)
        {
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
        }
    }
}
