using Krepim.SharedKernel.Results;
using Error = Krepim.SharedKernel.Results.Error;

namespace Krepim.Identity.Domain.Errors
{
    public static class IdentityErrors
    {
        public static readonly Error EmailNotUnique = new(
            "Identity.EmailNotUnique",
            "The provided email is already in use.",
            ErrorType.Conflict);

        public static readonly Error UserNotFound = new(
            "Identity.UserNotFound",
            "The user with the specified identifier was not found.",
            ErrorType.NotFound);

        public static readonly Error InvalidCredentials = new(
            "Identity.InvalidCredentials",
            "Invalid email or password.",
            ErrorType.Failure);
    }
}
