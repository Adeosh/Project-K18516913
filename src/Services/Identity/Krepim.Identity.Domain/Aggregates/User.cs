using Krepim.Identity.Domain.Enums;
using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;

namespace Krepim.Identity.Domain.Aggregates
{
    public sealed class User : AggregateRoot<Guid>
    {
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public Role Role { get; private set; }

        private User(Guid id, string email, string passwordHash, Role role) : base(id)
        {
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public static Result<User> Create(string email, string passwordHash, Role role)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<User>.Failure(new Error("User.InvalidEmail", "Email cannot be empty", ErrorType.Validation));

            var user = new User(Guid.NewGuid(), email, passwordHash, role);

            // добавить генерацию доменного события

            return user;
        }
    }
}
