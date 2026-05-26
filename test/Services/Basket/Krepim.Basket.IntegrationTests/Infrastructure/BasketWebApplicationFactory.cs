using Krepim.Testing.Shared.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Redis;

namespace Krepim.Basket.IntegrationTests.Infrastructure
{
    public class BasketWebApplicationFactory : BaseIntegrationTestFactory<Program>
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
            .Build();

        protected override async Task StartContainersAsync() => await _redisContainer.StartAsync();
        protected override async Task StopContainersAsync() => await _redisContainer.DisposeAsync();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
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
            services.RemoveAll(typeof(IDistributedCache));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
                options.InstanceName = "BasketTest_";
            });

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
