using Krepim.Payment.Infrastructure.Database;
using Krepim.Testing.Shared.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Krepim.Payment.IntegrationTests.Infrastructure
{
    public class PaymentWebApplicationFactory : BaseIntegrationTestFactory<Program>
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("payment_integration_db")
            .Build();

        protected override async Task StartContainersAsync() => await _dbContainer.StartAsync();
        protected override async Task StopContainersAsync() => await _dbContainer.DisposeAsync();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:paymentdb"] = _dbContainer.GetConnectionString(),
                    ["PaymentSettings:WebhookSecret"] = "whsec_test_secret_key_12345",
                    ["Jwt:SecretKey"] = "Super_Secret_Key_For_Krepim_System_Must_Be_Long_Enough_256bits!",
                    ["Jwt:Issuer"] = "Krepim.Identity",
                    ["Jwt:Audience"] = "Krepim.Clients"
                });
            });

            base.ConfigureWebHost(builder);
        }

        protected override void ConfigureCustomServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(DbContextOptions<PaymentDbContext>));
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            services.AddMassTransitTestHarness(x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
