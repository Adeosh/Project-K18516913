using Krepim.Mobile.Exceptions;

namespace Krepim.Mobile.Extensions
{
    public static class ApiExceptionExtensions
    {
        public static string ToUserFriendlyMessage(this ApiException ex)
        {
            if (ex.Problem == null)
                return "Неизвестная ошибка сети.";

            if (ex.Problem.Status == 400 && ex.Problem.Errors != null && ex.Problem.Errors.Count > 0)
                return string.Join("\n", ex.Problem.Errors.Select(e => e.Description));

            var standardTitles = new[] { "Conflict", "Bad Request", "Not Found", "Internal Server Error" };
            if (!string.IsNullOrWhiteSpace(ex.Problem.Detail) && !standardTitles.Contains(ex.Problem.Detail))
                return ex.Problem.Detail;

            return ex.Problem.Status switch
            {
                400 => "Пожалуйста, проверьте правильность введенных данных.",
                401 => "Неверный email или пароль.",
                403 => "У вас нет прав для выполнения этого действия.",
                404 => "Запрашиваемые данные не найдены.",
                409 => "Пользователь с такими данными уже существует.",
                422 => "Ошибка валидации данных. Проверьте заполненные поля.",
                500 => "Внутренняя ошибка сервера. Повторите попытку позже.",
                502 or 503 or 504 => "Сервер временно недоступен. Проверьте интернет-соединение.",
                _ => $"Произошла непредвиденная ошибка (Код: {ex.Problem.Status})."
            };
        }
    }
}
