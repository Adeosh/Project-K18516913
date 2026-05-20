using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Errors;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Authentication
{
    internal sealed class LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider) : IRequestHandler<LoginCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return Result<string>.Failure(IdentityErrors.InvalidCredentials);
            }

            bool isPasswordValid = passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Result<string>.Failure(IdentityErrors.InvalidCredentials);
            }

            string token = jwtProvider.Generate(user);

            return token;
        }
    }
}
