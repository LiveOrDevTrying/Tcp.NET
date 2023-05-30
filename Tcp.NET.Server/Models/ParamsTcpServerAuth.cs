using System;

namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServerAuth : ParamsTcpServer
    {
        public string ConnectionUnauthorizedString { get; protected set; }

        public ParamsTcpServerAuth(int port, string endOfLineCharacters, string connectionSuccessString = null, string connectionUnauthorizedString = null, bool onlyEmitBytes = false, int pingIntervalSec = 120, string pingCharacters = "ping", string pongCharacters = "pong", bool sendDisconnectBytes = true, byte[] disconnectBytes = null) : base(port, endOfLineCharacters, connectionSuccessString, onlyEmitBytes, pingIntervalSec, pingCharacters, pongCharacters, sendDisconnectBytes, disconnectBytes)
        {
            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionUnauthorizedString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true is a connectionUnauthorizedString is specified");
            }

            ConnectionUnauthorizedString = connectionUnauthorizedString;
        }
    }
}
