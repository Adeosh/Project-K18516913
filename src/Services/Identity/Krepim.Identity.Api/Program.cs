using Krepim.Identity.Api.Endpoints.Authentication;
using Krepim.Identity.Api.Endpoints.Profile;
using Krepim.Identity.Api.Endpoints.Registration;
using Krepim.Identity.Application;
using Krepim.Identity.Infrastructure;
using Krepim.Identity.Infrastructure.Database;
using Krepim.SharedKernel.Authentication;
using Krepim.SharedKernel.Exceptions;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddKrepimJwtAuth(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.AddRedisDistributedCache("redis");

var app = builder.Build();

await app.ApplyMigrationsAsync<IdentityDbContext>();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

var apiGroup = app.MapGroup("/api/identity")
    .WithTags("Identity")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapRegisterEndpoint();
apiGroup.MapLoginEndpoint();
apiGroup.MapProfileEndpoints();

app.Run();
