namespace Tcp.NET.Core.Enums
{
    public enum TcpConnectionAuthType
    {
        Authorized,
        Unauthorized
    }

    public enum TcpConnectionType
    {
        Connected,
        Disconnect,
        ServerStart,
        ServerStop,
        Connecting,
        MaxConnectionsReached,
        AuthorizationClient,
        Authorization
    }
}
