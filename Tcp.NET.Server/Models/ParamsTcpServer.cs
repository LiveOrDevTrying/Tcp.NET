﻿using PHS.Networking.Utilities;
using System;
using System.Text;

namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServer : IParamsTcpServer
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

        public ParamsTcpServer(int port, string endOfLineCharacters, string connectionSuccessString = null, bool onlyEmitBytes = false, int pingIntervalSec = 120, string pingCharacters = "ping", string pongCharacters = "pong", bool useDisconnectBytes = true, byte[] disconnectBytes = null) : base()
        {
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

            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionSuccessString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true is a connectionSuccesString is specified");
            }

            Port = port;
            EndOfLineBytes = Encoding.UTF8.GetBytes(endOfLineCharacters);
            PingBytes = Encoding.UTF8.GetBytes(pingCharacters);
            PongBytes = Encoding.UTF8.GetBytes(pongCharacters);
            ConnectionSuccessString = connectionSuccessString;
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
