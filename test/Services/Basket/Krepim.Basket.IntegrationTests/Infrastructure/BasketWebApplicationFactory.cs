using Krepim.Testing.Shared.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Redis;

namespace Krepim.Basket.IntegrationTests.Infrastructure
{
    public class BasketWebApplicationFactory : BaseIntegrationTestFactory<Program>
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
            .Build();

        protected override async Task StartContainersAsync()
        {
            await _redisContainer.StartAsync();
        }

        protected override async Task StopContainersAsync()
        {
            await _redisContainer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Этот блок можно оставить пустым, он нам больше не нужен, 
            // так как конфигурация переопределяется ниже напрямую в DI
            base.ConfigureWebHost(builder);
        }

        protected override void ConfigureCustomServices(IServiceCollection services)
        {
            // Вот магия! Мы переписываем опции Redis УЖЕ ПОСЛЕ того, 
            // как Program.cs попытался их зарегистрировать.
            services.Configure<RedisCacheOptions>(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
                options.InstanceName = "Basket_";
            });
        }
    }
}
