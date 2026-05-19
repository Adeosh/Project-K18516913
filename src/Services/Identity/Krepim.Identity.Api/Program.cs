using Krepim.Identity.Api.Endpoints.Authentication;
using Krepim.Identity.Api.Endpoints.Registration;
using Krepim.Identity.Api.Infrastructure.Filters;
using Krepim.Identity.Application;
using Krepim.Identity.Infrastructure;
using Krepim.SharedKernel.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

var apiGroup = app.MapGroup("/api/users")
    .WithTags("Users")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapRegisterEndpoint();
apiGroup.MapLoginEndpoint();

app.Run();
