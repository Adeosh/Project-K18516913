using Krepim.Catalog.Infrastructure.Database;
using Krepim.Testing.Shared.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Krepim.Catalog.IntegrationTests.Infrastructure
{
    public class CatalogWebApplicationFactory : BaseIntegrationTestFactory<Program>
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("catalog_test_db")
            .Build();

        private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7.0")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3-management-alpine")
            .Build();

        protected override async Task StartContainersAsync()
        {
            await Task.WhenAll(
                _postgresContainer.StartAsync(),
                _mongoContainer.StartAsync(),
                _rabbitMqContainer.StartAsync()
            );

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        protected override async Task StopContainersAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _mongoContainer.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:rabbitmq"] = _rabbitMqContainer.GetConnectionString()
                });
            });

            base.ConfigureWebHost(builder);
        }

        protected override void ConfigureCustomServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(DbContextOptions<CatalogDbContext>));
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            services.RemoveAll(typeof(IMongoClient));
            services.RemoveAll(typeof(IMongoDatabase));

            var mongoClient = new MongoClient(_mongoContainer.GetConnectionString());
            services.AddScoped<IMongoClient>(_ => mongoClient);
            services.AddScoped<IMongoDatabase>(_ => mongoClient.GetDatabase("KrepimCatalogTestDb"));
        }
    }
}
