using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using Krepim.SharedKernel.ValueObjects;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.UpdateUserProfile
{
    internal sealed class UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserProfileCommand, Result>
    {
        public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
                return Result.Failure(new Error("User.NotFound", "User not found", ErrorType.NotFound));

            if (user.Email != request.Email)
            {
                if (!await userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
                    return Result.Failure(new Error("User.EmailNotUnique", "Этот email уже зарегистрирован", ErrorType.Conflict));
            }

            Address? address = request.DefaultAddress is not null
                ? new Address(request.DefaultAddress.FullAddress ?? string.Empty, request.DefaultAddress.Latitude, request.DefaultAddress.Longitude, request.DefaultAddress.Flat)
                : null;

            user.UpdateProfile(request.Email, request.PhoneNumber, address);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
