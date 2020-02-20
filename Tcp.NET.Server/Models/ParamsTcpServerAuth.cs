namespace Tcp.NET.Server.Models
{
    public struct ParamsTcpServerAuth : IParamsTcpServerAuth
    {
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public string ConnectionSuccessString { get; set; }
        public string ConnectionUnauthorizedString { get; set; }
    }
}
