using Tcp.NET.Server.SSL.Models;

namespace Tcp.NET.Server.SSL.Auth.Interfaces
{
    public interface IParamsTcpSSLAuthServer : IParamsTcpServerSSL
    {
        string ConnectionSuccessString { get; set; }
        string UnauthorizedString { get; set; }
    }
}