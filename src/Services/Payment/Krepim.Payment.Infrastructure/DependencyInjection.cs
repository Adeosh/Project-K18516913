using Krepim.Payment.Application.Consumers;
using Krepim.Payment.Application.Interfaces;
using Krepim.Payment.Domain.Interfaces;
using Krepim.Payment.Infrastructure.Database;
using Krepim.Payment.Infrastructure.Database.Repositories;
using Krepim.Payment.Infrastructure.ExternalServices;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
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


            services.AddHttpClient<IPaymentGateway, StripePaymentService>(client =>
            {
                client.BaseAddress = new Uri(configuration["PaymentSettings:GatewayUrl"] ?? "https://api.stripe.com");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;

                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.MinimumThroughput = 4;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
            });

            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("rabbitmq"));
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
