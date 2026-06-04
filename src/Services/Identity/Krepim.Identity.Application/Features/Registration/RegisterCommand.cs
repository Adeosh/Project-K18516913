using Krepim.Identity.Domain.Enums;
using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Registration
{
    public sealed record RegisterCommand(
        string Email,
        string Password,
        Role Role,
        string? PhoneNumber) : IRequest<Result<Guid>>;
}
