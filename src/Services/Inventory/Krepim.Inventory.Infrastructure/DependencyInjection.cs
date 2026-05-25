using Krepim.Inventory.Application.Interfaces;
using Krepim.Inventory.Infrastructure.Database;
using Krepim.Inventory.Infrastructure.Database.Repositories;
using Krepim.SharedKernel.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krepim.Inventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("inventorydb")));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InventoryDbContext>());
            services.AddScoped<IInventoryRepository, InventoryRepository>();

            return services;
        }
    }
}
