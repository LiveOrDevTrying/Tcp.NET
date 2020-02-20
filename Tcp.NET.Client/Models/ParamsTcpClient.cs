namespace Tcp.NET.Client.Models
{
    public struct ParamsTcpClient : IParamsTcpClient
    {
        public string Uri { get; set; }
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public bool IsSSL { get; set; }
    }
}
