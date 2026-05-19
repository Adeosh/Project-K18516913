using Krepim.SharedKernel.Results;
using MediatR;

namespace Krepim.Identity.Application.Features.Authentication
{
    public sealed record LoginCommand(string Email, string Password) : IRequest<Result<string>>;
}
