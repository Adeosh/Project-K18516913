using Krepim.Identity.Infrastructure.Database;
using Krepim.Testing.Shared.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Krepim.Identity.IntegrationTests.Infrastructure;

public class IdentityWebApplicationFactory : BaseIntegrationTestFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("identity_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override async Task StartContainersAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override async Task StopContainersAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:SecretKey", "SUPER_SECRET_KEY_FOR_INTEGRATION_TESTS_KREPIM_123!"},
                {"Jwt:Issuer", "Krepim.Identity"},
                {"Jwt:Audience", "Krepim.Clients"},
                {"Jwt:ExpiryInMinutes", "60"}
            });
        });

        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureCustomServices(IServiceCollection services)
    {
        services.RemoveAll(typeof(DbContextOptions<IdentityDbContext>));
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(_dbContainer.GetConnectionString()));

        var hostedServices = services.Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService)).ToList();
        foreach (var service in hostedServices)
        {
            if (service.ImplementationType?.FullName?.Contains("MassTransit") == true)
            {
                services.Remove(service);
            }
        }

        services.AddMassTransitTestHarness(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}