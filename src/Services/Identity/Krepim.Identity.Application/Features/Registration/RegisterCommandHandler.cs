using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Errors;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Registration
{
    internal sealed class RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
        : IRequestHandler<RegisterCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (!await userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
                return Result<Guid>.Failure(IdentityErrors.EmailNotUnique);

            string passwordHash = passwordHasher.Hash(request.Password);

            Result<User> userResult = User.Create(request.Email, passwordHash, request.Role);

            if (userResult.IsFailure)
                return Result<Guid>.Failure(userResult.Error);

            await userRepository.AddAsync(userResult.Value, cancellationToken);

            return userResult.Value.Id;
        }
    }
}
