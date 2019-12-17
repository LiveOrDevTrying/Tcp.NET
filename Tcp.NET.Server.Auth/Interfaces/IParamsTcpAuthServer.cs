using Tcp.NET.Server.Models;

namespace Tcp.NET.Server.Auth.Interfaces
{
    public interface IParamsTcpAuthServer : IParamsTcpServer
    {
        string ConnectionSuccessString { get; set; }
        string UnauthorizedString { get; set; }
    }
}