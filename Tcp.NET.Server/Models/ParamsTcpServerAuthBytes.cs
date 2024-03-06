using System;

namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServerAuthBytes : ParamsTcpServerBytes, IParamsTcpServerAuth
    {
        public string ConnectionUnauthorizedString { get; protected set; }

        public ParamsTcpServerAuthBytes(int port, byte[] endOfLineBytes, string connectionSuccessString = null, string connectionUnauthorizedString = null, bool onlyEmitBytes = false, int pingIntervalSec = 120, byte[] pingBytes = null, byte[] pongBytes = null, bool sendDisconnectBytes = true, byte[] disconnectBytes = null) : base(port, endOfLineBytes, connectionSuccessString, onlyEmitBytes, pingIntervalSec, pingBytes, pongBytes, sendDisconnectBytes, disconnectBytes)
        {
            if (onlyEmitBytes && !string.IsNullOrWhiteSpace(connectionUnauthorizedString))
            {
                throw new ArgumentException("onlyEmitBytes can not be true if a connectionUnauthorizedString is specified");
            }

            ConnectionUnauthorizedString = connectionUnauthorizedString;
        }
    }
}
