using PHS.Networking.Server.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tcp.NET.TestApps.Server
{
    public class MockUserService : IUserService<Guid>
    {
        public Task<bool> TryGetIdAsync(string token, out Guid id, CancellationToken cancellationToken = default)
        {
            id = Guid.NewGuid();

            return token == "testToken" ? Task.FromResult(true) : Task.FromResult(false);
        }
    }
}
