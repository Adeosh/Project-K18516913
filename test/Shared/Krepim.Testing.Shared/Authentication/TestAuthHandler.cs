using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Krepim.Testing.Shared.Authentication
{
    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "Test";
        public const string DefaultUserId = "00000000-0000-0000-0000-000000000001";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey("X-Test-Anonymous"))
                return Task.FromResult(AuthenticateResult.NoResult());

            var role = Request.Headers.TryGetValue("X-Test-Role", out var customRole)
                ? customRole.ToString()
                : "Client";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, DefaultUserId),
                new(ClaimTypes.Name, "IntegrationTestUser"),
                new(ClaimTypes.Role, role)
            };

            if (!Request.Headers.ContainsKey("X-Test-Role"))
                claims.Add(new Claim(ClaimTypes.Role, "Manager"));

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
