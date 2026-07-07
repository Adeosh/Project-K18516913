using Krepim.SharedKernel.Results;
using Error = Krepim.SharedKernel.Results.Error;

namespace Krepim.Identity.Domain.Errors
{
    public static class IdentityErrors
    {
        public static readonly Error EmailNotUnique = new(
            "Identity.EmailNotUnique",
            "Указанный адрес электронной почты уже используется.",
            ErrorType.Conflict);

        public static readonly Error UserNotFound = new(
            "Identity.UserNotFound",
            "Пользователь с указанным идентификатором найден не был.",
            ErrorType.NotFound);

        public static readonly Error InvalidCredentials = new(
            "Identity.InvalidCredentials",
            "Неверный адрес электронной почты или пароль.",
            ErrorType.Validation);
    }
}
