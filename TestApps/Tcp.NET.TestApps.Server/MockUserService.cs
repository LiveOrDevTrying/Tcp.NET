using PHS.Networking.Server.Services;
using PHS.Networking.Utilities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tcp.NET.TestApps.Server
{
    public class MockUserService : IUserService<Guid>
    {
        public Task<Guid> GetIdAsync(byte[] token, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task<bool> IsValidTokenAsync(byte[] token, CancellationToken cancellationToken = default)
        {
            return Statics.ByteArrayEquals(token, Encoding.UTF8.GetBytes("testToken")) ? Task.FromResult(true) : Task.FromResult(false);
        }
    }
}
