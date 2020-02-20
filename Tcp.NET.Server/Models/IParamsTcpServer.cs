namespace Tcp.NET.Server.Models
{
    public interface IParamsTcpServer
    {
        int Port { get; set; }
        string EndOfLineCharacters { get; set; }
        string ConnectionSuccessString { get; set; }
    }
}