using Krepim.Identity.Api.Endpoints.Authentication;
using Krepim.Identity.Api.Endpoints.Registration;
using Krepim.Identity.Application;
using Krepim.Identity.Infrastructure;
using Krepim.Identity.Infrastructure.Database;
using Krepim.SharedKernel.Exceptions;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.ApplyMigrationsAsync<IdentityDbContext>();

app.UseExceptionHandler();

var apiGroup = app.MapGroup("/api/users")
    .WithTags("Users")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapRegisterEndpoint();
apiGroup.MapLoginEndpoint();

app.Run();
