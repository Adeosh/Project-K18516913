using Krepim.Ordering.Application.Consumers;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.Ordering.Infrastructure.Database;
using Krepim.Ordering.Infrastructure.Database.Repositories;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krepim.Ordering.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderingDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("orderingdb")));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderingDbContext>());
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<BasketCheckoutEventConsumer>();
                x.AddConsumer<PaymentStatusChangedEventConsumer>();

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
