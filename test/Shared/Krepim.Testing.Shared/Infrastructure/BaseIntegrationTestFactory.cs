using Krepim.Testing.Shared.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Krepim.Testing.Shared.Infrastructure
{
    public abstract class BaseIntegrationTestFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
        where TEntryPoint : class
    {
        protected abstract Task StartContainersAsync();
        protected abstract Task StopContainersAsync();

        protected abstract void ConfigureCustomServices(IServiceCollection services);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });

                services.AddAuthorizationBuilder()
                    .AddPolicy("Manager", policy =>
                    {
                        policy.AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme);
                        policy.RequireRole("Manager");
                    })
                    .AddPolicy("Client", policy =>
                    {
                        policy.AddAuthenticationSchemes(TestAuthHandler.AuthenticationScheme);
                        policy.RequireRole("Client");
                    });

                ConfigureCustomServices(services);
            });
        }

        public async ValueTask InitializeAsync()
        {
            await StartContainersAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await StopContainersAsync();
            await base.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
