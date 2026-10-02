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
            services.AddScoped<IPaymentGateway, MockPaymentService>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("rabbitmq"));
                    cfg.PrefetchCount = 20; // не более 20 сообщений одновременно на один инстанс
                    cfg.UseConcurrencyLimit(20);
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
