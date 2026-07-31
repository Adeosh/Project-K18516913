using Krepim.EventBus.Events.Identity;
using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Identity.Domain.Enums;
using Krepim.Identity.Domain.Errors;
using Krepim.Identity.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Krepim.SharedKernel.Results;
using MassTransit;
using MediatR;

namespace Krepim.Identity.Application.Features.Registration
{
    internal sealed class RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IPublishEndpoint publishEndpoint,
        IUnitOfWork unitOfWork) : IRequestHandler<RegisterCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (!await userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
                return Result<Guid>.Failure(IdentityErrors.EmailNotUnique);

            string passwordHash = passwordHasher.Hash(request.Password);

            Result<User> userResult = User.Create(request.Email, passwordHash, Role.Client, request.PhoneNumber);

            if (userResult.IsFailure)
                return Result<Guid>.Failure(userResult.Error);

            await userRepository.AddAsync(userResult.Value, cancellationToken);

            UserRegisteredIntegrationEvent integrationEvent = new UserRegisteredIntegrationEvent(
                userResult.Value.Id,
                userResult.Value.Email,
                userResult.Value.Role.ToString());

            await publishEndpoint.Publish(integrationEvent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return userResult.Value.Id;
        }
    }
}
