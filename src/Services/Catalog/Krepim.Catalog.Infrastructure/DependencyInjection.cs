using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Infrastructure.Consumers;
using Krepim.Catalog.Infrastructure.Database;
using Krepim.Catalog.Infrastructure.Database.Repositories;
using Krepim.Catalog.Infrastructure.Storage;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Krepim.Catalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("catalogdb")));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
            services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("KrepimCatalogViewDb");
            });
            services.AddScoped<IProductReadRepository, ProductReadRepository>();
            services.AddSingleton<IFileStorageService, MinioFileStorageService>();
            services.Configure<MinioOptions>(configuration.GetSection("Minio"));

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });
                x.AddConsumer<ProductCreatedConsumer>();
                x.AddConsumer<ProductUpdatedConsumer>();
                x.AddConsumer<ProductStatusChangedConsumer>();
                x.AddConsumer<ProductDeletedConsumer>();
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
