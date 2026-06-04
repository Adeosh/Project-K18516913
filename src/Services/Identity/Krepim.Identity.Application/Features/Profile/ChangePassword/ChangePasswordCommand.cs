using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Profile.ChangePassword
{
    public sealed record ChangePasswordCommand(
        Guid UserId,
        string OldPassword,
        string NewPassword) : IRequest<Result>;
}
