using System;

namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServerAuth : ParamsTcpServer
    {
        public string ConnectionUnauthorizedString { get; protected set; }

        public ParamsTcpServerAuth(int port, string endOfLineCharacters, string connectionSuccessString = null, string connectionUnauthorizedString = null, int pingIntervalSec = 120, bool onlyEmitBytes = false, string pingCharacters = "ping", string pongCharacters = "pong") : base(port, endOfLineCharacters, connectionSuccessString, pingIntervalSec, onlyEmitBytes, pingCharacters, pongCharacters)
        {
            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionUnauthorizedString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true is a connectionUnauthorizedString is specified");
            }

            ConnectionUnauthorizedString = connectionUnauthorizedString;
        }

        public ParamsTcpServerAuth(int port, byte[] endOfLineBytes, string connectionSuccessString = null, string connectionUnauthorizedString = null, int pingIntervalSec = 120, bool onlyEmitBytes = false, byte[] pingBytes = null, byte[] pongBytes = null) : base(port, endOfLineBytes, connectionSuccessString, pingIntervalSec, onlyEmitBytes, pingBytes, pongBytes)
        {
            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionUnauthorizedString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true is a connectionUnauthorizedString is specified");
            }

            ConnectionUnauthorizedString = connectionUnauthorizedString;
        }
    }
}
