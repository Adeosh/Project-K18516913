using Krepim.Inventory.Application.Consumers;
using Krepim.Inventory.Infrastructure.Database;
using Krepim.Testing.Shared.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Krepim.Inventory.IntegrationTests.Infrastructure
{
    public class InventoryWebApplicationFactory : BaseIntegrationTestFactory<Program>
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .Build();

        protected override async Task StartContainersAsync() => await _dbContainer.StartAsync();
        protected override async Task StopContainersAsync() => await _dbContainer.DisposeAsync();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:inventorydb"] = _dbContainer.GetConnectionString(),
                    ["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@localhost:5672",
                    ["Jwt:SecretKey"] = "Super_Secret_Key_For_Krepim_System_Must_Be_Long_Enough_256bits!",
                    ["Jwt:Issuer"] = "Krepim.Identity",
                    ["Jwt:Audience"] = "Krepim.Clients"
                });
            });

            base.ConfigureWebHost(builder);
        }

        protected override void ConfigureCustomServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(DbContextOptions<InventoryDbContext>));
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<OrderCreatedEventConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
