using Krepim.SharedKernel.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Krepim.SharedKernel.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddKrepimJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();

            if (jwtOptions is null || string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
                throw new InvalidOperationException("Настройки JWT не найдены в конфигурации (секция 'Jwt').");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                                        .CreateLogger("Authentication");

                            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                                         ?? context.Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                            if (userId != null)
                            {
                                var cache = context.HttpContext.RequestServices.GetService<IDistributedCache>();

                                if (cache != null)
                                {
                                    try
                                    {
                                        var isBlocked = await cache.GetStringAsync($"jwt-blocklist:{userId}");

                                        if (isBlocked != null)
                                            context.Fail("Токен отозван в связи со сменой пароля.");
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.LogWarning(ex, "Ошибка при проверке блэклиста в Redis для пользователя {UserId}", userId);
                                    }
                                }
                            }
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public static async Task RevokeTokenAsync(this IDistributedCache cache, string userId, TimeSpan expiration)
        {
            await cache.SetStringAsync($"jwt-blocklist:{userId}", "revoked", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }
    }
}
