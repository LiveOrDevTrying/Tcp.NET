using PHS.Networking.Server.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tcp.NET.TestApps.Server
{
    public class MockUserService : IUserService<Guid>
    {
        public Task<Guid> GetIdAsync(string token, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return token == "testToken" ? Task.FromResult(true) : Task.FromResult(false);
        }
    }
}
