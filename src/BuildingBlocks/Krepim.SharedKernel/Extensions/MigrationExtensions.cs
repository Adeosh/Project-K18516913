using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Krepim.SharedKernel.Extensions
{
    public static class MigrationExtensions
    {
        /// <summary>
        /// Автоматически проверяет и загружает миграции для указанного DbContext с защитой от сбоя старта СУБД.
        /// </summary>
        public static async Task ApplyMigrationsAsync<TDbContext>(this IApplicationBuilder app, int maxRetries = 5, int delaySeconds = 2)
            where TDbContext : DbContext
        {
            await using var scope = app.ApplicationServices.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
            var contextName = typeof(TDbContext).Name;

            int retries = maxRetries;
            while (retries > 0)
            {
                try
                {
                    logger.LogInformation("[{Context}] Проверка миграций... Осталось попыток: {Retries}", contextName, retries);

                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                    {
                        await context.Database.MigrateAsync();
                        logger.LogInformation("[{Context}] Миграции успешно применены.", contextName);
                    }
                    else
                        logger.LogInformation("[{Context}] База данных находится в актуальном состоянии.", contextName);

                    break;
                }
                catch (Exception ex)
                {
                    retries--;
                    if (retries == 0)
                    {
                        logger.LogCritical(ex, "[{Context}] База данных так и не ответила после всех попыток. Приложение аварийно завершает работу.", contextName);
                        throw;
                    }

                    logger.LogWarning("[{Context}] База данных еще не готова. Ожидание {Delay} сек... Ошибка: {Message}",
                        contextName, delaySeconds, ex.Message);

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
    }
}
