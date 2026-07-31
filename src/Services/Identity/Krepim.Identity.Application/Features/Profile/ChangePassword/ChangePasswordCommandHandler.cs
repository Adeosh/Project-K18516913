using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.ChangePassword
{
    internal sealed class ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, Result>
    {
        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
                return Result.Failure(new Error("User.NotFound", "Пользователь не найден", ErrorType.NotFound));

            bool isOldPasswordValid = passwordHasher.Verify(request.OldPassword, user.PasswordHash);

            if (!isOldPasswordValid)
                return Result.Failure(new Error("User.InvalidPassword", "Неверный текущий пароль", ErrorType.Validation));

            string newPasswordHash = passwordHasher.Hash(request.NewPassword);
            user.UpdatePasswordHash(newPasswordHash);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
