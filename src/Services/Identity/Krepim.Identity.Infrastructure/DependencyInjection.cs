using Krepim.Identity.Application.Interfaces;
using Krepim.Identity.Infrastructure.Authentication;
using Krepim.Identity.Infrastructure.Database;
using Krepim.Identity.Infrastructure.Database.Repositories;
using Krepim.SharedKernel.Authentication;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krepim.Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("identitydb")));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<IdentityDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });
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
