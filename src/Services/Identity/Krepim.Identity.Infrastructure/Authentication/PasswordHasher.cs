using Krepim.Identity.Application.Interfaces;

namespace Krepim.Identity.Infrastructure.Authentication
{
    internal sealed class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        public bool Verify(string password, string hash) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }
}
