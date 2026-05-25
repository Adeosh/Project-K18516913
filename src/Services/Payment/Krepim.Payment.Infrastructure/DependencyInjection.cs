using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Infrastructure.Database;
using Krepim.Payment.Infrastructure.Database.Repositories;
using Krepim.Payment.Infrastructure.ExternalServices;
using Krepim.SharedKernel.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krepim.Payment.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("paymentdb")));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentDbContext>());
            services.AddScoped<IPaymentRepository, PaymentRepository>();


            services.AddHttpClient<StripePaymentService>(client =>
            {
                client.BaseAddress = new Uri(configuration["PaymentSettings:GatewayUrl"] ?? "[https://api.stripe.com](https://api.stripe.com)");
                client.Timeout = TimeSpan.FromSeconds(10); // тайм-аут на один запрос
            })
            .AddStandardResilienceHandler(options => // Настройка Retry (Повторные попытки)
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential; // Экспоненциальное увеличение задержки

                options.CircuitBreaker.FailureRatio = 0.5; // Если 50% запросов за окно падают — размыкаем цепь
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30); // Окно сбора статистики
                options.CircuitBreaker.MinimumThroughput = 4; // Минимальное количество запросов в окне для срабатывания
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15); // Время, на которое предохранитель закрывает доступ к банку
            });

            return services;
        }
    }
}
