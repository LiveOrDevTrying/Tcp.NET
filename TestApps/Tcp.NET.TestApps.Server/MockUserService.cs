using PHS.Networking.Server.Services;
using System;
using System.Threading.Tasks;

namespace Tcp.NET.TestApps.Server
{
    public class MockUserService : IUserService<Guid>
    {
        public Task<Guid> GetIdAsync(string token)
        {
            return Task.FromResult(Guid.NewGuid());
        }
        public void Dispose()
        {
        }
    }
}
