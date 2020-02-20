namespace Tcp.NET.Client.Models
{
    public interface IParamsTcpClient
    {
        string Uri { get; set; }
        int Port { get; set; }
        string EndOfLineCharacters { get; set; }
        bool IsSSL { get; set; }
    }
}