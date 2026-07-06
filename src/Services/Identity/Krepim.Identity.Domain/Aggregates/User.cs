using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Events;
using Krepim.SharedKernel.Domain;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.ValueObjects;

namespace Krepim.Identity.Domain.Aggregates
{
    public sealed class User : AggregateRoot<Guid>
    {
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public Role Role { get; private set; }
        public string? PhoneNumber { get; private set; }
        public Address? DefaultAddress { get; private set; }

        private User(Guid id, string email, string passwordHash, Role role, string? phoneNumber) : base(id)
        {
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            PhoneNumber = phoneNumber;
        }

        public static Result<User> Create(string email, string passwordHash, Role role, string? phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<User>.Failure(new Error("User.InvalidEmail", "Email cannot be empty", ErrorType.Validation));

            var user = new User(Guid.NewGuid(), email, passwordHash, role, phoneNumber);

            return user;
        }

        public Result UpdateProfile(string email, string? phoneNumber, Address? defaultAddress)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure(new Error("User.InvalidEmail", "Email cannot be empty", ErrorType.Validation));

            Email = email;
            PhoneNumber = phoneNumber;
            DefaultAddress = defaultAddress;

            return Result.Success();
        }

        public void UpdatePasswordHash(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            RaiseDomainEvent(new UserPasswordChangedDomainEvent(Id));
        }
    }
}
