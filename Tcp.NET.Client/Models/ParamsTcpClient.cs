namespace Tcp.NET.Client.Models
{
    public class ParamsTcpClient
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
        public bool IsSSL { get; set; }
    }
}
