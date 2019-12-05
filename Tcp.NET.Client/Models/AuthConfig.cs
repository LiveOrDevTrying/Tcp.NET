namespace Tcp.NET.Client.Models
{
    public struct AuthConfig
    {
        public string AuthorityUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
    }
}
