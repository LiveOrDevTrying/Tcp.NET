namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServerAuth : ParamsTcpServer, IParamsTcpServerAuth
    {
        public string ConnectionUnauthorizedString { get; set; }
    }
}
