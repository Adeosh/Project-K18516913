using Krepim.Ordering.Application.Consumers;
using Krepim.Ordering.Application.Interfaces;
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
