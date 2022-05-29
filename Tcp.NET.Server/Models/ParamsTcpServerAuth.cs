namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServerAuth : ParamsTcpServer
    {
        public ParamsTcpServerAuth(int port, string endOfLineCharacters, string connectionSuccessString = null, string connectionUnauthorizedString = null, int pingIntervalSec = 120, bool onlyEmitBytes = false, string pingCharacters = "ping", string pongCharacters = "pong") : base(port, endOfLineCharacters, connectionSuccessString, pingIntervalSec, onlyEmitBytes, pingCharacters, pongCharacters)
        {
            ConnectionUnauthorizedString = connectionUnauthorizedString;
        }

        public ParamsTcpServerAuth(int port, byte[] endOfLineBytes, string connectionSuccessString = null, string connectionUnauthorizedString = null, int pingIntervalSec = 120, bool onlyEmitBytes = false, byte[] pingBytes = null, byte[] pongBytes = null) : base(port, endOfLineBytes, connectionSuccessString, pingIntervalSec, onlyEmitBytes, pingBytes, pongBytes)
        {
            ConnectionUnauthorizedString = connectionUnauthorizedString;
        }

        public string ConnectionUnauthorizedString { get; set; }
    }
}
