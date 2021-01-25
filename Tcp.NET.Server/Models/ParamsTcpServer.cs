namespace Tcp.NET.Server.Models
{
    public class ParamsTcpServer : IParamsTcpServer
    {
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public string ConnectionSuccessString { get; set; }
    }
}
