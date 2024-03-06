namespace Tcp.NET.Server.Models
{
    public interface IParamsTcpServerAuth : IParamsTcpServer
    {
        string ConnectionUnauthorizedString { get; }
    }
}