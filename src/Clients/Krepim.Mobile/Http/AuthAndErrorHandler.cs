using Krepim.Mobile.Exceptions;
using Krepim.Mobile.Features.Auth.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Krepim.Mobile.Http
{
    public class AuthAndErrorHandler : DelegatingHandler
    {
        private readonly ILogger<AuthAndErrorHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AuthAndErrorHandler(ILogger<AuthAndErrorHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? token = AuthService.RawToken;

            if (string.IsNullOrEmpty(token))
                token = await SecureStorage.Default.GetAsync("krepim_token");

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                ProblemDetails? problem = null;

                try
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    problem = JsonSerializer.Deserialize<ProblemDetails>(content);
                }
                catch{ }

                problem ??= new ProblemDetails
                {
                    Status = status,
                    Title = response.ReasonPhrase ?? "Unknown Error",
                    Detail = "Произошла неизвестная ошибка."
                };

                _logger.LogError($"[API Error {status}] {problem.Title}: {problem.Detail}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        _logger.LogWarning("Сессия устарела. Перенаправление на вход...");

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            var authService = _serviceProvider.GetRequiredService<AuthService>();
                            await authService.LogoutAsync();
                        });
                        break;

                    case HttpStatusCode.Forbidden:
                        _logger.LogError("Доступ запрещен.");
                        break;

                    case HttpStatusCode.BadRequest:
                        if (problem.Errors != null)
                        {
                            foreach (var err in problem.Errors)
                                _logger.LogWarning($"Валидация [{err.Code}]: {err.Description}");
                        }
                        break;

                    case HttpStatusCode.Conflict:
                        _logger.LogError($"Конфликт состояния данных: {problem.Detail}");
                        break;

                    case HttpStatusCode.InternalServerError:
                        _logger.LogError("Критическая ошибка сервера. Обратитесь к администратору.");
                        break;

                    case HttpStatusCode.BadGateway:
                        _logger.LogError("Шлюз недоступен (502). Один из микросервисов упал.");
                        problem.Detail = "Сервис временно недоступен. Повторите попытку позже.";
                        break;
                }

                throw new ApiException(problem);
            }

            return response;
        }
    }
}
